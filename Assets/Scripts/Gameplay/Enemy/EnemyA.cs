/*
 * Represents the main enemy of the game.
 * It is a robot that patrols areas, guards entities, and exhibits intelligent behavior based on player actions.
 * 
 * Patrol and Movement:
 * - Patrol points are managed by the PatrolHandler, while paths between points are calculated using the 
 *   NavMeshAgent.
 * - The NavMeshAgent is used exclusively for AI pathfinding and avoidance behaviors.
 *   Movement is handled manually using the CharacterController.
 * 
 * Behavior:
 * - The enemy normally patrols its designated area, as assigned by the PatrolHandler.
 * - If it hears a noise, it pauses its patrolling routine, switches its state to "Warning," investigates the 
 *   noise, and then resumes patrolling with its state set to "Clear."
 * - If it sees the player:
 *   * The "Game Over" event is triggered, and the enemy shoots at the player (purely for animation purposes).
 *   * If contact occurs between the enemy and the player, the enemy teleports to an accessible area with 
 *     visibility of the player and shoots the player (resulting in "Game Over"), regardless of whether the 
 *     player is invisible or disguised as a box.
 * 
 * Special Note:
 * - As part of the story, the robot possesses the ability to teleport, immobilize the player, and eliminate 
 *   the player with a single laser hit.
 * 
 * Interactions with Disguised Players (Boxes):
 * - The enemy ignores stationary boxes unless:
 *   * The box is moving, in which case the enemy identifies it as the player and triggers "Game Over" by 
 *     shooting at it.
 *   * The box obstructs the enemy's patrolling path, which annoys the enemy, causing it to destroy the box 
 *     (killing the player in the process).
 * 
 * Guarding Behavior (if GuardHandler.Infos is not null):
 * - In addition to patrolling, the enemy guards specific entities. While guarding, the same behavior applies 
 *   as mentioned above, with the following additions:
 *   * If the player, disguised as a box, blocks the enemy's vision of the guarded entity (e.g., by hiding it), 
 *     the enemy shoots at the box to clear its vision.
 *   * If the guarded entity disappears or is deemed unsafe, the event associated with the GuardHandler is 
 *     triggered.
 * 
 * Patrol Adjustment:
 * - The patrol assigned may change due to an external event. The enemy will follow the newly assigned patrol 
 *   and, when the patrol ends, will return to its normal patrolling behavior.
 *
 * The enemy assumes the following states:
 * - Clear (eyes blue): Normal state.
 * - Warning (eyes yellow): An anomaly has been detected and requires investigation.
 * - Alert (eyes red): The player has been detected (GameOver) or the enemy is preparing to launch an attack.
 */


using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController), typeof(NavMeshAgent))]
public class EnemyA : Enemy, IStateSaveable, IDamageable, IPatrol
{
    //Enemy Equip
    [SerializeField] private MeshRenderer eyesRenderer;
    [SerializeField] private Transform projectileSpawnpoint;
    [SerializeField] private GameObject energySphere;
    [SerializeField] private LineRenderer lineRenderer;

    //Animation
    private Animator animator = null;
    //Animation hash: (Optimized hash for accessing to animator parameter)
    private int shootESphereStateHash;

    //Movement
    private bool isMoving = false;

    //Enemy spotted
    private bool playerSpotted = false;

    //Settings
    [SerializeField] private int health = 1;

    //Patrol
    [SerializeField] private PatrolHandler.PatrolState patrolState = PatrolHandler.PatrolState.CLEAR;
    [SerializeField] private PatrolHandler patrol;          //Default patrol
    [SerializeField] private PatrolHandler currentPatrol;
    [SerializeField] private bool isPatrolling = true;
    private bool checkStarted = false;
    private bool checkFinished = false;
    [SerializeField] private float checkDistance = 1f;
    private Vector3 noisePosition;
    private bool heardNoise = false;
    private bool movingToNoise = false;
    [SerializeField] private float noiseCheckTime;
    private float startTime = -1f;
    private Coroutine checkProcessCoroutine = null;
    private Coroutine checkNoiseCoroutine = null;

    //Guard 
    [SerializeField] private GuardHandler guardHandler;

    [SerializeField] private int damage = 1;

    protected override void Awake()
    {
        base.Awake();

        //Animation
        animator = GetComponent<Animator>();
        shootESphereStateHash = Animator.StringToHash("ShootEnergySphere");

        //LineRenderer
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;

        //NavMesh
        //From the NavMesh, only AI-related functionalities are required, excluding movement handling.
        navAgent.updatePosition = false;
        navAgent.updateRotation = false;
        navAgent.autoBraking = false; //To prevent slowdowns when approaching the destination point.
        navAgent.speed = speed;
        navAgent.angularSpeed = rotationSpeed;
    }

    private void Start()
    {
        //To ensure that the initialization in the Start method does not override the initialization
        //from LoadState.
        if (!initialized)
        {
            currentPatrol = patrol;
            SetDestination(currentPatrol.GetCurrentPoint().Position);
            initialized = true;
        }
    }

    private void OnDisable()
    {
        CheckProcessDispose();
        CheckNoiseDispose();
        Utilities.HandleSound(audioSource, null, Utilities.HandleSoundState.STOP, 0);
    }

    //Properties
    public bool IsPatrolling { get { return isPatrolling; } }

    /*
     *  Why FixedUpdate instead of Update:
     *  NavAgent relies on the frame rate to update its position and direction. 
     *  If the game runs at an average FPS below 30, the calculations made by the NavAgent can become incorrect, 
     *  leading to issues when following the path using the steeringTarget. 
     *  This problem becomes more noticeable as the entity approaches its destination,
     *  where the steering target may not return the next value to follow, but instead an incorrect one 
     *  (likely the previous value). 
     *  In SetMovingDirection, this results in a new direction pointing to a previous point, 
     *  forcing the entity to turn around and then turn back to aim at the next position.
     *  FixedUpdate provides more consistent updates, solving the issue effectively!
     *  Tested with an average FPS of: <10, 10, 28, >30.
     *  
     *  Note: At lower FPS (<30), rotation may cause issues with the character controller. 
     *        During rotation, the entity's position might unexpectedly change, even while standing still 
     *        and only rotating. 
     *        (This is likely due to collision calculations handled by the CharacterController.)
     *        Nevertheless, using FixedUpdate appears to resolve this problem!
     */
    private void FixedUpdate()
    {
        if (!initialized || playerSpotted) //Either not initialized or behavior is stopped.
        {
            return;
        }

        HandleGravity();
        HandleAI();
        HandleRotation();
        Move();
        CheckFOV();
    }

    private void Update()
    {
        //When the game is paused with Time.timeScale = 0, FixedUpdate is not executed.
        //Therefore, the audio must be stopped directly from this method.
        if (Time.timeScale == 0 || !initialized || playerSpotted) //Game paused/Entity not initialized/Behaviour stopped
        {
            Utilities.HandleSound(audioSource, null, Utilities.HandleSoundState.PAUSE, 0);   //Pause SFX
        }
        else
        {
            Utilities.HandleSound(audioSource, null, Utilities.HandleSoundState.UNPAUSE, 0); //Unpause SFX
        }

    }

    //Functions
    private void SetDestination(Vector3 destination)
    {
        navAgent.SetDestination(destination);
    }

    private void DestinationReached()
    {
        if(navAgent.hasPath)
        {
            navAgent.ResetPath();
        }
    }

    private void SetMovingDirection()
    {
        if (canMove)
        {
            if (!navAgent.pathPending)
            {
                Vector3 move = (navAgent.steeringTarget - transform.position).normalized;
                velocity.x = move.x * speed;
                velocity.z = move.z * speed;
            }
        }
        else
        {
            velocity.x = velocity.z = 0f;
        }

        if (canRotate)
        {
            if (!navAgent.pathPending)
            {
                Vector3 move = (navAgent.steeringTarget - transform.position).normalized;
                rotationDir.x = move.x;
                rotationDir.z = move.z;
                rotationDir = rotationDir.normalized;
            }
        }
        else
            rotationDir = Vector3.zero;
    }

    protected override void Move()
    {
        if (!canMove)
            return;

        //Generate sound 
        Vector2 movement = new Vector2(velocity.x, velocity.z);

        if (!isMoving && movement.sqrMagnitude > 0) //If the entity is moving.
        {
            isMoving = true;
            Utilities.HandleSound(audioSource, defaultAudioMap.movements[0], Utilities.HandleSoundState.PLAY, 1); //loop = true
        }
        else if (isMoving && movement.sqrMagnitude == 0) //If the entity is not moving.
        {
            isMoving = false;
            //Allow the sound to stop naturally by disabling the loop.
            Utilities.HandleSound(audioSource, defaultAudioMap.movements[0], Utilities.HandleSoundState.PLAY, 2); //loop = false
        }

        //Move
        controller.Move(velocity * Time.deltaTime);

        navAgent.nextPosition = transform.position;
    }

    private void HandleAI()
    {
        if (isPatrolling) //Indicates that the entity is on patrol duty.
        {
            PatrolPoint currentPoint = currentPatrol.GetCurrentPoint();
            Vector3 dest = currentPoint.Position;
            Vector3 lookAtDir = currentPoint.GetCurrentLookAt().GetForward();

            if (IsInPlace(transform.position, dest)) //Destination has been reached.
            {
                velocity.x = velocity.z = 0f;
                DestinationReached();

                if (Vector3.Angle(lookAtDir, transform.forward) == 0) //The entity is facing the correct direction.
                {
                    rotationDir = Vector3.zero;

                    if (!checkStarted)
                    {
                        CheckProcessDispose();
                        checkProcessCoroutine = StartCoroutine(
                            CheckProcessCoroutine(currentPoint.GetCurrentLookAt().Time)
                        );
                    }
                    else if (checkStarted && checkFinished)
                    {
                        checkStarted = false;
                        checkFinished = false;

                        if (!currentPoint.SetNextLookAt()) //If no other direction to look at exists.
                        {
                            if (currentPatrol.SetNextPoint())  //Assign the next patrolling point.
                                SetDestination(currentPatrol.GetCurrentPoint().Position);
                            else //End of patrolling: Reset to the default patrolling behavior.
                            {
                                currentPatrol = patrol;
                                currentPatrol.ResetPatrol();
                                SetDestination(currentPatrol.GetCurrentPoint().Position);
                                SetClear();
                            }
                        }
                    }
                }
                else //Fix the entity's looking direction.
                {
                    rotationDir = lookAtDir;
                }
            }
            else //Moving toward the assigned destination.
            {

                SetMovingDirection();
            }
        }
        else //Engaged in another activity (e.g., investigating the source of a noise).
        {
            if (heardNoise)
            {
                if (movingToNoise)
                {
                    if (Vector3.Distance(transform.position, noisePosition) <= 1f)
                    {
                        velocity.x = velocity.z = 0f;
                        DestinationReached();

                        if (!checkStarted)
                        {
                            checkNoiseCoroutine = StartCoroutine(CheckNoiseCoroutine(noiseCheckTime));
                        }
                        else if (checkStarted && checkFinished)
                        {
                            checkStarted = false;
                            checkFinished = false;

                            //Restore the patrolling routine.
                            SetDestination(currentPatrol.GetCurrentPoint().Position);
                            isPatrolling = true;
                            movingToNoise = false;
                            heardNoise = false;
                            SetClear();
                        }
                    }
                    else //Moving toward the location of the noise.
                    {
                        SetMovingDirection();
                    }
                }
                else
                {
                    CheckProcessDispose();
                    CheckNoiseDispose();
                    /*
                     * Once the enemy completes the check, it will return to its patrolling routine.
                     * However, it will restart the check process at the last position where it was previously 
                     * located.
                     */
                    currentPatrol.GetCurrentPoint().ResetPatrol();
                    SetDestination(noisePosition);
                    movingToNoise = true;
                }
            }
            else
                isPatrolling = true;
        }
    }

    private void CheckFOV()
    {
        if (!canSee)
            return;

        FOVHandler.FOVCheckState checkState = FOV.CheckFOV(
            EngineConf.Layer.EnemySearchMask,
            EngineConf.Layer.EnemyObstructionMask
            );

        if (checkState.gameOver)
        {
            GameOver();
            return;
        }
        else if (checkState.boxSpotted)
        {
            if (IsBoxBlockingPath(
                PlayerManager.Instance.Player as PlayerBox,
                EngineConf.Layer.EnemySearchMask
            ))
            {
                GameOver();
                return;
            }
        }

        if (guardHandler.Infos == null)
            return;

        foreach (GuardBehaviour info in guardHandler.Infos)
        {
            if (!info.completed)
            {
                FOVHandler.GuardCheckState guardCheckState =
                    FOV.CheckGuardableState(info.Entity, EngineConf.Layer.GuardableObstructionMask);

                if (guardCheckState.visible)
                {
                    if (!info.Entity.IsSecure())
                        info.InvokeResponse();
                }
                else
                {
                    if (guardCheckState.boxSpotted)
                    {
                        GameOver();
                        return;
                    }
                }
            }

        }
    }

    private bool IsBoxBlockingPath(PlayerBox player, LayerMask targetLayer)
    {
        if (navAgent.path.corners.Length > 1) //If == 1: Destination reached, no movement required.
        {
            if (Vector3.Distance(player.transform.position, transform.position) <= (checkDistance + controller.radius))
            {
                for (int i = 0; i < navAgent.path.corners.Length - 1; i++)
                {
                    if (IsLineIntersectingBox(
                        navAgent.path.corners[i],
                        navAgent.path.corners[i + 1],
                        targetLayer,
                        controller.bounds.extents.y)
                    )
                        return true;
                }
            }
        }

        return false;
    }

    bool IsLineIntersectingBox(Vector3 start, Vector3 end, LayerMask targetLayer, float yOffset)
    {
        //Check if the "line" segment intersects the box. 
        //Note: The "line" is actually a segment with the dimensions of a box (matching the size of the enemy).

        RaycastHit hit;
        start.y += yOffset;
        end.y += yOffset;

        if (Physics.BoxCast(
            start, controller.bounds.extents, (end - start).normalized, out hit,
            Quaternion.identity, Vector3.Distance(start, end), targetLayer
        ))
            return true;

        return false;
    }

    private void GameOver()
    {
        playerSpotted = true;
        SoundManager.Instance.GenerateTempSFX(transform.position, defaultAudioMap.spottedPlayer, false);
        Player player = PlayerManager.Instance.Player;

        //Rotate the entity to face the player and initiate shooting.
        controller.enabled = false;
        Vector3 destDir = (player.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(destDir);
        controller.enabled = true;

        //Trigger the shooting animation.
        Vector3 dest = player.transform.position;
        dest.y = projectileSpawnpoint.position.y;

        lineRenderer.SetPosition(0, projectileSpawnpoint.position);
        lineRenderer.SetPosition(1, projectileSpawnpoint.position);

        animator.Play(shootESphereStateHash);

        SetAlert();
        LevelManager.Instance.GameOver(true);
    }

    //Invoked by the "shootESphereState" animation
    public void GenerateLaserSound()
    {
        Vector3 dest = PlayerManager.Instance.Player.transform.position;
        dest.y = projectileSpawnpoint.position.y;

        float dist = Vector3.Distance(projectileSpawnpoint.position, dest);

        SoundManager.Instance.GenerateTempSFX(projectileSpawnpoint.position, defaultAudioMap.shootingSounds[0], maxDistance: dist + 5f);

        RaycastHit hit;
        Vector3 dir = (dest - projectileSpawnpoint.position).normalized;

        if (Physics.Raycast(projectileSpawnpoint.position, dir, out hit, dist, 
            EngineConf.Layer.LaserHitMask & ~(1 << gameObject.layer), 
            QueryTriggerInteraction.Ignore))
        {
            lineRenderer.SetPosition(1, hit.point);
            IDamageableComponent damageableC = hit.transform.GetComponent<IDamageableComponent>();
            damageableC?.GetDamageable()?.TakeDamage(damage);
        }
        else
            lineRenderer.SetPosition(1, projectileSpawnpoint.position + dir * 10f);
    }

    //Coroutines
    private IEnumerator CheckProcessCoroutine(float time)
    {
        startTime = Time.time;
        checkStarted = true;
        checkFinished = false;
        yield return new WaitForSeconds(time);
        checkFinished = true;
        checkProcessCoroutine = null;
        startTime = -1f;
    }

    private void CheckProcessDispose()
    {
        if (checkProcessCoroutine != null)
        {
            StopCoroutine(checkProcessCoroutine);
            checkProcessCoroutine = null;
            startTime = -1f;
            checkStarted = false; //
            checkFinished = false;
        }
    }

    private IEnumerator CheckNoiseCoroutine(float time)
    {
        startTime = Time.time;
        checkStarted = true;
        checkFinished = false;
        yield return new WaitForSeconds(time);
        checkFinished = true;
        checkNoiseCoroutine = null;
        startTime = -1f;
    }

    private void CheckNoiseDispose()
    {
        if (checkNoiseCoroutine != null)
        {
            StopCoroutine(checkNoiseCoroutine);
            checkNoiseCoroutine = null;
            startTime = -1f;
            checkStarted = false; //
            checkFinished = false;
        }
    }

    //Handle collision with the player and sound detection.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == EngineConf.Layer.PLAYER)
        {
            //Collision with a body part of the player
            if (other.CompareTag(EngineConf.Tag.PLAYER_BODY_PART))
            {
                Player player = PlayerManager.Instance.Player;
                Vector3 dir = (player.transform.position - transform.position).normalized;
                float distance = player.ControllerRadius + controller.radius + 1;

                for (int i = 0; i < 8; i++)
                {
                    //Locate a position for the entity to warp by evaluating positions encircling the player.
                    Vector3 pos = player.transform.position + distance * (Quaternion.AngleAxis(i * 45, Vector3.up) * dir);
                    Vector3? warpPosition = GetReachable(pos);

                    if (warpPosition != null)
                    {
                        controller.enabled = false;

                        //Warp 
                        transform.position = warpPosition.Value;
                        //In the GameOver method, the entity will rotate to face the player and initiate shooting.

                        controller.enabled = true;
                        break;
                    }
                }

                GameOver();
                return;
            }

            //Noise or sound made by the player has been detected.
            if (other.CompareTag(EngineConf.Tag.SOUNDNOISE) && !playerSpotted)
            {
                noisePosition = other.transform.position;
                isPatrolling = false;
                heardNoise = true;
                movingToNoise = false;
                checkStarted = false;
                checkFinished = false;
                SetWarning();
                return;
            }
        }
    }

    //Given a position, determine a nearby reachable position (or return null if none can be found).
    public Vector3? GetReachable(Vector3 position)
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(position, out hit, 1f, navAgent.areaMask))
        {
            NavMeshPath path = new NavMeshPath();

            if (navAgent.CalculatePath(hit.position, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    return hit.position;
                }
            }
        }
        return null;
    }

    private void SetClear()
    {
        patrolState = PatrolHandler.PatrolState.CLEAR;
        eyesRenderer.material.color = clearColor;
        FOV.SetClear();
    }

    private void SetWarning()
    {
        patrolState = PatrolHandler.PatrolState.WARNING;
        eyesRenderer.material.color = warningColor;
        FOV.SetWarning();
    }

    private void SetAlert()
    {
        patrolState = PatrolHandler.PatrolState.ALERT;
        eyesRenderer.material.color = alertColor;
        FOV.SetAlert();
    }

    //IDamageable
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
            Kill();
    }

    public void Kill()
    {
        SoundManager.Instance.GenerateTempSFX(transform.position, defaultAudioMap.getKilled[0], maxDistance: 20f);
        gameObject.SetActive(false);
    }

    //IPatrol
    public void ChangePatrol(PatrolHandler handler, PatrolHandler.PatrolState state)
    {
        CheckProcessDispose();
        currentPatrol = handler;
        SetDestination(currentPatrol.GetCurrentPoint().Position);

        switch (state)
        {
            case PatrolHandler.PatrolState.WARNING:
                SetWarning();
                break;

            case PatrolHandler.PatrolState.ALERT:
                SetAlert();
                break;

            default:
                SetClear();
                break;
        }
    }

    //Entity State
    public IState SaveState()
    {
        CustomEntityState state = new CustomEntityState();

        //Transform
        state.position = transform.position;
        state.rotation = transform.rotation;
        state.scale = transform.localScale;

        //Movement
        state.velocity = velocity;
        state.rotationDir = rotationDir;
        state.speed = speed;
        state.rotationSpeed = rotationSpeed;

        //Gravity
        state.gravityMultiplier = gravityMultiplier;
        state.isFalling = isFalling;
        state.isGrounded = isGrounded;

        //Settings
        state.useGravity = useGravity;
        state.canMove = canMove;
        state.canRotate = canRotate;
        state.canSee = canSee;
        state.canHear = canHear;
        state.isVisible = isVisible;
        state.health = health;

        //Patrol
        state.patrolState = patrolState;
        state.patrol = currentPatrol;
        state.isPatrolling = isPatrolling;
        state.checkStarted = checkStarted;
        state.checkFinished = checkFinished;
        state.checkDistance = checkDistance;
        state.noisePosition = noisePosition;
        state.heardNoise = heardNoise;
        state.movingToNoise = movingToNoise;
        state.noiseCheckTime = noiseCheckTime;

        if (startTime < 0)
            state.timer = -1;
        else
            state.timer = Time.time - startTime;
        //Elapsed time since the start (using the current patrol point timer to compute the remaining time).

        //Guard
        state.guardState = guardHandler.SaveState();

        return state;
    }

    public void LoadState(IState state)
    {
        CustomEntityState _state = state as CustomEntityState;
        controller.enabled = false;

        //Transform
        transform.position = _state.position;
        transform.rotation = _state.rotation;
        transform.localScale = _state.scale;
        navAgent.Warp(_state.position);

        //Movement
        velocity = _state.velocity;
        rotationDir = _state.rotationDir;
        speed = _state.speed;
        navAgent.speed = speed;
        rotationSpeed = _state.rotationSpeed;
        navAgent.angularSpeed = rotationSpeed;

        //Gravity
        gravityMultiplier = _state.gravityMultiplier;
        isFalling = _state.isFalling;
        isGrounded = _state.isGrounded;

        //Settings
        useGravity = _state.useGravity;
        canMove = _state.canMove;
        canRotate = _state.canRotate;
        canSee = _state.canSee;
        canHear = _state.canHear;
        isVisible = _state.isVisible;
        health = _state.health;

        //Patrol
        switch (_state.patrolState)
        {
            case PatrolHandler.PatrolState.WARNING:
                SetWarning();
                break;

            case PatrolHandler.PatrolState.ALERT:
                SetAlert();
                break;

            default:
                SetClear();
                break;
        }
        currentPatrol = _state.patrol;
        isPatrolling = _state.isPatrolling;
        checkStarted = _state.checkStarted;
        checkFinished = _state.checkFinished;
        checkDistance = _state.checkDistance;
        noisePosition = _state.noisePosition;
        heardNoise = _state.heardNoise;
        movingToNoise = _state.movingToNoise;
        noiseCheckTime = _state.noiseCheckTime;

        controller.enabled = true;

        //Guard
        guardHandler.LoadState(_state.guardState);

        if (health <= 0)
        {
            initialized = true;
            //Kill(); => Generate SFX
            gameObject.SetActive(false);
            return;
        }

        if (_state.timer < 0) //If timer < 0: No coroutine has started, proceed with moving to the destination.
        {
            if (isPatrolling)
            {
                SetDestination(currentPatrol.GetCurrentPoint().Position);

                if (checkStarted) //Check started, but no coroutine started: Trigger an error.
                {
                    checkStarted = false; //Fix the error 
                    checkFinished = false;
                }
            }
            else
            {
                if (heardNoise)
                {
                    if (movingToNoise)
                    {
                        SetDestination(noisePosition);

                        if (checkStarted) //Check started, but no coroutine started: Trigger an error.
                        {
                            checkStarted = false; //Fix the error 
                            checkFinished = false;
                        }
                    }
                }
                else
                {
                    isPatrolling = true;
                    checkStarted = false;
                    checkFinished = false;
                }
            }
        }
        else //Restore the coroutine
        {
            CheckProcessDispose();
            CheckNoiseDispose();

            if (isPatrolling)
                checkProcessCoroutine = StartCoroutine(
                    CheckProcessCoroutine(
                        currentPatrol.GetCurrentPoint().GetCurrentLookAt().Time - _state.timer
                    )
                );
            else
                checkNoiseCoroutine = StartCoroutine(CheckNoiseCoroutine(noiseCheckTime - _state.timer));
        }

        initialized = true;
    }

    [System.Serializable]
    private class CustomEntityState : IState
    {
        //Transform
        [SerializeField] public Vector3 position;
        [SerializeField] public Quaternion rotation;
        [SerializeField] public Vector3 scale;

        //Movement
        [SerializeField] public Vector3 velocity;
        [SerializeField] public Vector3 rotationDir;
        [SerializeField] public float speed;
        [SerializeField] public float rotationSpeed;

        //Gravity
        [SerializeField] public float gravityMultiplier;
        [SerializeField] public bool isFalling;
        [SerializeField] public bool isGrounded;

        //Settings
        [SerializeField] public bool useGravity;
        [SerializeField] public bool canMove;
        [SerializeField] public bool canRotate;
        [SerializeField] public bool canSee;
        [SerializeField] public bool canHear;
        [SerializeField] public bool isVisible;
        [SerializeField] public int health;

        //Patrol
        [SerializeField] public PatrolHandler.PatrolState patrolState;
        [SerializeField] public PatrolHandler patrol;
        [SerializeField] public bool isPatrolling;
        [SerializeField] public bool checkStarted;
        [SerializeField] public bool checkFinished;
        [SerializeField] public float checkDistance;
        [SerializeField] public Vector3 noisePosition;
        [SerializeField] public bool heardNoise;
        [SerializeField] public bool movingToNoise;
        [SerializeField] public float noiseCheckTime;
        [SerializeField] public float timer;

        //Guard
        [SerializeReference] public IState guardState;
    }
}
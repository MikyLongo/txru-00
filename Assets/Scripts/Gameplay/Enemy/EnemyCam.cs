/*
 * Represents an enemy camera.
 * It is a stationary camera that patrols areas by rotating, guards entities, and exhibits 
 * intelligent behavior based on player actions.
 * 
 * Patrol:
 * - PatrolHandler defines the angles to monitor.
 * 
 * Behavior:
 * - The camera typically patrols its designated area as assigned by the PatrolHandler.
 * - If it detects the player:
 *   * The "Game Over" event is triggered.
 * 
 * Interactions with Disguised Players (Boxes):
 * - The enemy ignores stationary boxes unless:
 *   * The box is moving, in which case the enemy identifies it as the player and triggers "Game Over."
 * 
 * Guarding Behavior (if GuardHandler.Infos is not null):
 * - Alongside patrolling, the enemy guards specific entities. During guarding, the same behavior applies 
 *   as mentioned above, with the following additions:
 *   * If the player, disguised as a box, obstructs the enemy's vision of the guarded entity (e.g., by hiding it), 
 *     the camera concludes that the box is the player, as it recognizes that there was no box previously
 *     obstructing the guarded entity.
 *   * If the guarded entity disappears or is deemed unsafe, the event associated with the GuardHandler is 
 *     triggered.
 * 
 * Patrol Adjustment:
 * - The assigned patrol may change due to external events. The enemy will follow the new patrol 
 *   and, once it ends, will return to its normal patrolling behavior.
 *
 * The enemy assumes the following states:
 * - Clear (eyes/lens blue): Normal state.
 * - Warning (eyes/lens yellow): An anomaly has been detected and requires investigation.
 * - Alert (eyes/lens red): The player has been detected (GameOver).
 * - Powered off (eyes/lens gray): The camera is non-functional because it is powered off.
 */

using System.Collections;
using UnityEngine;

public class EnemyCam : Enemy, IStateSaveable, IPatrol
{
    //Equip
    [SerializeField] private Transform cam; //Transform of the rotating body part
    [SerializeField] private MeshRenderer camEyesRenderer;
    //Patrol
    [SerializeField] private PatrolHandler.PatrolState patrolState = PatrolHandler.PatrolState.CLEAR;
    [SerializeField] private PatrolHandler patrol; //Default patrol
    [SerializeField] private PatrolHandler currentPatrol;
    [SerializeField] private GuardHandler guardHandler;
    [SerializeField] private bool isWorking = true;
    [SerializeField] private Color notWorkingColor; //Color of the eye/lens when the camera is powered off.
    private bool checkStarted = false;
    private bool checkFinished = false;
    private bool playerSpotted = false;
    private float startTime = -1f;
    private Coroutine checkProcessCoroutine = null;

    protected override void Awake()
    {
        //base.Awake();
    }

    private void Start()
    {
        //To ensure that the initialization in the Start method does not override the initialization
        //from LoadState.
        if (!initialized)
        {
            currentPatrol = patrol;
            initialized = true;
        }
    }

    private void OnDisable()
    {
        CheckProcessDispose();
    }

    private void Update()
    {
        //Game paused / not initialized / behaviour stopped / powered off 
        if (Time.timeScale == 0 || !initialized || playerSpotted || !isWorking) 
            return;

        HandleAI();
        HandleRotation();
        CheckFOV();
    }

    protected override void Move()
    {
        //Unable to move
    }

    //Differs from the base class method as it moves the camera GameObject here.
    protected override void HandleRotation()
    {
        if (canRotate && rotationDir != Vector3.zero)
        {
            //Gets the current rotation, which defines the Camera's current direction
            Quaternion current = cam.rotation;
            //Gets the rotation corresponding to the direction to look at
            Quaternion target = Quaternion.LookRotation(rotationDir);
            //Interpolates the rotation
            //cam.rotation = Quaternion.Slerp(current, target, rotationSpeed * Time.deltaTime);
            cam.rotation = Quaternion.RotateTowards(current, target, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleAI()
    {
        PatrolPoint currentPoint = currentPatrol.GetCurrentPoint();
        Vector3 lookAtDir = currentPoint.GetCurrentLookAt().GetForward();

        if (Vector3.Angle(lookAtDir, cam.forward) == 0) //The entity is facing the correct direction.
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
                    if(!currentPatrol.SetNextPoint()) //End of patrolling: Reset to the default patrolling behavior.
                    {
                        currentPatrol = patrol;
                        currentPatrol.ResetPatrol();
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

    /*
    void OnDrawGizmos()
    {
        if (FOV == null) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(FOV.transform.position, FOV.transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(FOV.Distance * 2, FOV.Height * 2, FOV.Distance * 2));
    }
    */
    private void CheckFOV()
    {
        if (!canSee)
            return;

        FOVHandler.FOVCheckState checkState = FOV.CheckFOV(
            EngineConf.Layer.EnemySearchMask, 
            EngineConf.Layer.EnemyObstructionMask
            );

        if(checkState.gameOver)
        {
            GameOver();
            return;
        }

        if (guardHandler.HasGuardDuty())
        {
            foreach (GuardBehaviour info in guardHandler.Infos)
            {
                if (!info.completed)
                {
                    FOVHandler.GuardCheckState guardCheckState =
                        FOV.CheckGuardableState(info.Entity, EngineConf.Layer.GuardableObstructionMask);

                    if (guardCheckState.visible)
                    {
                        if (!info.Entity.IsSecure())
                        {
                            info.InvokeResponse();
                            SetWarning();
                            PlaySawSomething();
                        }
                    }
                    else
                    {
                        /*
                         * If a box obstructs the view, it's game over,
                         * as the camera detects that this obstruction was not there before.
                         */
                        if (guardCheckState.boxSpotted)
                        {
                            GameOver();
                            return;
                        }
                    }
                }
            }
        }
    }

    private void PlaySawSomething() //Emit a sound upon detecting an anomaly.
    {
        audioSource.clip = defaultAudioMap.shootingSounds[0];
        audioSource.volume = SoundManager.Instance.GetSoundEffectsVolume();
        audioSource.Play();
    }

    private void GameOver()
    {
        playerSpotted = true;
        PlaySawSomething();
        SoundManager.Instance.GenerateTempSFX(transform.position, defaultAudioMap.spottedPlayer, false);
        SetAlert();
        LevelManager.Instance.GameOver(true);
    }

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

    public bool IsWorking 
    {
        get { return isWorking; }
        set 
        {
            FOV.EnableMeshRenderer(value);

            if (value)
            {
                switch(patrolState)
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
            else
            {
                camEyesRenderer.material.color = notWorkingColor;
            }

            isWorking = value;
        }
    }

    private void SetClear()
    {
        patrolState = PatrolHandler.PatrolState.CLEAR;
        camEyesRenderer.material.color = clearColor;
        FOV.SetClear();
    }

    private void SetWarning()
    {
        patrolState = PatrolHandler.PatrolState.WARNING;
        camEyesRenderer.material.color = warningColor;
        FOV.SetWarning();
    }

    private void SetAlert()
    {
        patrolState = PatrolHandler.PatrolState.ALERT;
        camEyesRenderer.material.color = alertColor;
        FOV.SetAlert();
    }

    //IPatrol
    public void ChangePatrol(PatrolHandler handler, PatrolHandler.PatrolState state)
    {
        CheckProcessDispose();
        currentPatrol = handler;

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

        //Cam Transform
        state.position = cam.position;
        state.rotation = cam.rotation;
        state.scale = cam.localScale;

        //Movement
        state.rotationDir = rotationDir;
        state.rotationSpeed = rotationSpeed;

        //Settings
        state.canRotate = canRotate;
        state.canSee = canSee;

        //Patrol
        state.patrolState = patrolState;
        state.patrol = currentPatrol;
        state.isWorking = IsWorking;
        state.checkStarted = checkStarted;
        state.checkFinished = checkFinished;

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

        //Cam Transform
        cam.position = _state.position;
        cam.rotation = _state.rotation;
        cam.localScale = _state.scale;

        //Movement
        rotationDir = _state.rotationDir;
        rotationSpeed = _state.rotationSpeed;

        //Settings
        canRotate = _state.canRotate;
        canSee = _state.canSee;

        //Patrol
        patrolState = _state.patrolState;
        currentPatrol = _state.patrol;
        IsWorking = _state.isWorking;
        checkStarted = _state.checkStarted;
        checkFinished = _state.checkFinished;

        //Guard
        guardHandler.LoadState(_state.guardState);

        if (!IsWorking)
        {
            initialized = true;
            return;
        }

        if (_state.timer < 0) //If timer < 0: No coroutine has started, proceed with moving to the destination.
        {
            if (checkStarted) //Check started, but no coroutine started: Trigger an error.
            {
                checkStarted = false; //Fix the error 
                checkFinished = false;
            }
        }
        else //Restore the coroutine
        {
            CheckProcessDispose();

            checkProcessCoroutine = StartCoroutine(
                CheckProcessCoroutine(
                    currentPatrol.GetCurrentPoint().GetCurrentLookAt().Time - _state.timer
                )
            );
        }

        initialized = true;
    }

    [System.Serializable]
    private class CustomEntityState : IState
    {
        //Cam Transform
        [SerializeField] public Vector3 position;
        [SerializeField] public Quaternion rotation;
        [SerializeField] public Vector3 scale;

        //Movement
        [SerializeField] public Vector3 rotationDir;
        [SerializeField] public float rotationSpeed;

        //Settings
        [SerializeField] public bool canRotate;
        [SerializeField] public bool canSee;

        //Patrol
        [SerializeField] public PatrolHandler.PatrolState patrolState;
        [SerializeField] public PatrolHandler patrol;
        [SerializeField] public bool isWorking;
        [SerializeField] public bool checkStarted;
        [SerializeField] public bool checkFinished;
        [SerializeField] public float timer;

        //Guard
        [SerializeReference] public IState guardState;
    }
}

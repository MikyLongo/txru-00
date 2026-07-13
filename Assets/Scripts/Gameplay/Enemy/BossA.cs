/*
 * Represents the demo Boss.
 * It is a stationary robot that exhibits intelligent behavior based on player actions.
 * When the boss sees the player, it will begin its battle with the player.
 * The boss has two types of attacks, both consisting of shooting lasers but in different manners.
 * 
 * - The first is a large laser that can reach long range, but before launching it, the boss needs to have
 *   its four chargers fully charged (upon use, they will be emptied) and prepare the attack.
 * - The second attack is launched without requiring full chargers. It consists of firing four small lasers 
 *   coming from the chargers (this does not empty the chargers) when the player is near the boss 
 *   (close-range laser).
 * 
 * Behavior:
 * The boss is a stationary unit and will stay idle until it spots the player for the first time.
 * The boss has a large spherical FOV, so it is able to see in every direction.
 * When it spots the player for the first time, the boss changes from Idle state to Battle state.
 * 
 * Idle State: Does nothing.
 * 
 * Battle State:
 * - When the player is visible (Alert mode), the boss keeps rotating around itself with high rotation speed to
 *   aim its laser cannon at the player.
 * - When the player is no longer visible, the boss transitions to Warning mode and continues looking at the
 *   player's last known position for a certain period. After that, it starts rotating more slowly, searching 
 *   for the player.
 * - If the distance between the player and the boss is small (<= weakLaserRange), the boss will attack with
 *   the 4 weak lasers (the attack will stop if the player moves out of range or becomes invisible).
 * - If the distance is greater, the boss will prepare to launch its strong attack.
 * 
 * Strong Attack:
 * - If the player is not near the boss and the chargers are not fully charged, the boss will keep rotating to 
 *   face the player.
 * - When all the chargers are full, the boss will prepare to launch its strong attack.
 * - While preparing to attack, the boss will start rotating around itself (z-axis) while aiming at the player.
 * - After some time, an energy orb will appear. The boss will start rotating faster, and the energy orb will
 *   grow larger.
 * - When the orb reaches its maximum size, the boss will fire the laser at the player's last known position.
 * - Note that during this process, if the player becomes invisible, the boss will not search for the player but will 
 *   continue facing the last known position.
 * - When ready to shoot, the boss will remain stationary and unable to rotate, even if the player becomes visible, 
 *   as it locks onto the shooting position.
 * 
 * Note: The charger does not charge while executing an attack!
 * 
 * The enemy assumes the following states:
 * - Clear (eyes): Normal state (Idle).
 * - Warning (eyes): Battle state - The player is no longer visible.
 * - Alert (eyes): Battle state - The player is visible.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BossA : Enemy, IDamageable
{
    //Equip
    [SerializeField] private MeshRenderer eyes;
    [SerializeField] private Transform body;

    //T: Top, B: Bottom, L: Left, R: Right
    [SerializeField] private Transform strongLaserPivot;
    [SerializeField] private Transform[] weakLaserPivots;       //4 (TL, TR, BL, BR) 

    [SerializeField] private GameObject strongLaser;
    [SerializeField] private LineRenderer[] weakLaserRenderers; //4 (TL, TR, BL, BR)

    [SerializeField] private MeshRenderer[] chargerRenderers;   //4 (TL, TR, BL, BR)
    [SerializeField] private Color chargeEmptyColor;
    [SerializeField] private Color chargeFullColor;

    [SerializeField] private AudioSource[] weakLaserAS;         //4 (TL, TR, BL, BR)
    [SerializeField] private AudioSource strongLaserAS;

    //State
    [SerializeField] private int health = 4;
    [SerializeField] private float charge = 0f;
    [SerializeField] private float chargingTime = 5f;

    //Patrol
    [SerializeField] private PatrolHandler.PatrolState patrolState = PatrolHandler.PatrolState.CLEAR;
    [SerializeField] private bool isAlertToWarningTransition = false;
    //Maintain the current direction if transitioning from alert to warning state
    [SerializeField] private float alertToWarningDelay = 0.8f;
    private Coroutine alertToWarningCoroutine = null;

    //Rotation
    [SerializeField] private float spottedRotatingSpeed = 180f;
    [SerializeField] private float searchRotatingSpeed = 120f;

    //Attack
    [SerializeField] private int weakLaserDamage = 1;
    [SerializeField] private float weakLaserRange = 5f;
    [SerializeField] private float strongLaserRange = 13f;

    //Attack animation
    [SerializeField] private bool isAttackingStrong = false;
    [SerializeField] private bool isAttackingStrongStarted = false;
    [SerializeField] private bool isAttackingStrongEnded = false;
    [SerializeField] private float strongAtkRotTime = 2f;
    [SerializeField] private float strongAtkLaserTime = 1f;
    [SerializeField] private float bodyMinRotSpeed = 270f;
    [SerializeField] private float bodyMaxRotSpeed = 540f;
    private Coroutine strongAtkCoroutine = null;

    [SerializeField] private bool isAttackingWeak = false;

    [SerializeField] private ParticleSystem takeDamagePS = null;
    [SerializeField] private ParticleSystem deathPS = null;
    [SerializeField] private UnityEvent deathEvent = null; //Event triggered upon death

    protected override void Awake()
    {
        rotationSpeed = searchRotatingSpeed;
        for (int i = 0; i < weakLaserRenderers.Length; i++)
        {
            weakLaserRenderers[i].enabled = false;
            weakLaserRenderers[i].positionCount = 2;
            weakLaserAS[i].clip = defaultAudioMap.shootingSounds[1];
        }

        strongLaserAS.clip = defaultAudioMap.shootingSounds[0];

        strongLaser.SetActive(false);
    }

    private void OnDisable()
    {
        StrongAttackAnimDispose();
    }

    private void Update()
    {
        if (Time.timeScale == 0) //Game paused
        {
            //Pause SFX
            Utilities.HandleSound(audioSource, null, Utilities.HandleSoundState.PAUSE, 0);

            for(int i=0; i<weakLaserAS.Length; i++)
            {
                Utilities.HandleSound(weakLaserAS[i], null, Utilities.HandleSoundState.PAUSE, 0);
            }

            Utilities.HandleSound(strongLaserAS, null, Utilities.HandleSoundState.PAUSE, 0);

            return;
        }
        else
        {
            //Unpause SFX
            Utilities.HandleSound(audioSource, null, Utilities.HandleSoundState.UNPAUSE, 0);

            for (int i = 0; i < weakLaserAS.Length; i++)
            {

                Utilities.HandleSound(weakLaserAS[i], null, Utilities.HandleSoundState.UNPAUSE, 0);
            }

            Utilities.HandleSound(strongLaserAS, null, Utilities.HandleSoundState.UNPAUSE, 0);
        }


        CheckFOV();         //Check if the player is visible
        CheckAttack();      //Determine if the entity can attack and its rotation
        HandleRotation();   //Rotate the entity
        HandleAnimation();  //Handle the attack animation process
    }

    protected override void Move()
    {
        //Unable to move
    }

    private bool IsAttacking { get { return isAttackingStrong || isAttackingWeak; } }

    private void CheckFOV()
    {
        FOVHandler.FOVCheckState checkState = FOV.CheckFOV(
            EngineConf.Layer.EnemySearchMask,
            EngineConf.Layer.EnemyObstructionMask
            );

        if(checkState.gameOver) //If gameOver is true, it indicates the player has been spotted
        {
            if (patrolState == PatrolHandler.PatrolState.WARNING)
                AlertToWarningDispose();

            SetAlert();
        }
        else
        {
            if(patrolState == PatrolHandler.PatrolState.ALERT)
            {
                AlertToWarningDispose();
                alertToWarningCoroutine = StartCoroutine(AlertToWarningCoroutine());

                SetWarning();
            }
        }
    }

    private void CheckAttack()
    {
        /*
         * Executed after CheckFOV:
         * - Alert: The player is visible.
         * - Warning: The player is no longer visible.
         */
        Vector3 playerPos = PlayerManager.Instance.Player.transform.position;

        if(isAlertToWarningTransition) //Maintain direction during transition from alert to warning state
            return;

        if (IsAttacking)
        {
            //If the target is still visible, continue following it
            if (patrolState == PatrolHandler.PatrolState.ALERT) 
                rotationDir = (playerPos - transform.position).normalized;

            return;
        }
        
        if (patrolState == PatrolHandler.PatrolState.ALERT) //Alert = Player visible
        {
            if (Vector3.Distance(transform.position, playerPos) <= weakLaserRange)
                isAttackingWeak = true;
            else if(charge >= 1f) //(distance is <= strongLaserRange)
                isAttackingStrong = true;

            //Rotate the entity to face the player.
            rotationDir = (playerPos - transform.position).normalized;
            rotationSpeed = spottedRotatingSpeed;
        }
        else if(patrolState == PatrolHandler.PatrolState.WARNING) //The player was visible but is no longer in sight
        {
            //Rotate around
            rotationDir = transform.right;
            rotationSpeed = searchRotatingSpeed;
        }
    }

    private void HandleAnimation()
    {
        if (isAttackingStrong && !isAttackingStrongStarted) //Starting animation
        {
            isAttackingStrongStarted = true;
            isAttackingStrongEnded = false;
            StrongAttackAnimDispose();
            strongAtkCoroutine = StartCoroutine(StrongAttackAnimCoroutine());
            return;
        }
        else if (isAttackingStrong && isAttackingStrongStarted && isAttackingStrongEnded) //Ending animation
        {
            canRotate = true; //Around the Y Axis
            isAttackingStrong = false;
            isAttackingStrongStarted = false;
            EmptyCharger();
            return;
        }
        else if (isAttackingStrong)
            return; //Avoid invoking the FillCharger() method


        if (isAttackingWeak)
        {
            Vector3 pPos = PlayerManager.Instance.Player.transform.position;

            //Stop attack
            if (
                patrolState == PatrolHandler.PatrolState.WARNING ||
                Vector3.Distance(pPos, transform.position) > weakLaserRange
            ) 
            {
                for (int i = 0; i < weakLaserRenderers.Length; i++)
                {
                    weakLaserRenderers[i].enabled = false;
                    Utilities.HandleSound(weakLaserAS[i], null, Utilities.HandleSoundState.STOP, 0);
                }

                isAttackingWeak = false;
                return;
            }

            //Start/Continue Attack
            RaycastHit hit;
            Vector3 dir;
            float dist;
            Vector3 cPos = pPos + (PlayerManager.Instance.Player.Height/2)*Vector3.up;
            Vector3 hPos = cPos;

            for (int i = 0; i < weakLaserRenderers.Length; i++)
            {
                dir = (cPos - weakLaserPivots[i].position).normalized;
                dist = Vector3.Distance(cPos, weakLaserPivots[i].position);
                
                if (Physics.Raycast(weakLaserPivots[i].position, dir, out hit, dist,
                     EngineConf.Layer.LaserHitMask & ~(1 << gameObject.layer),
                     QueryTriggerInteraction.Ignore)
                )
                {
                    IDamageableComponent damageableC = hit.transform.GetComponent<IDamageableComponent>();
                    damageableC?.GetDamageable()?.TakeDamage(weakLaserDamage);
                    hPos = hit.point;
                }

                weakLaserRenderers[i].SetPosition(0, weakLaserPivots[i].position);
                weakLaserRenderers[i].SetPosition(1, hPos);
                weakLaserRenderers[i].enabled = true;
                Utilities.HandleSound(weakLaserAS[i], defaultAudioMap.shootingSounds[1], Utilities.HandleSoundState.PLAY, 2);
            }
            return;
        }

        FillCharger();
    }

    private void FillCharger()
    {
        charge = Mathf.Clamp(charge + Time.deltaTime * 1 / chargingTime, 0f, 1f);

        if (charge >= 1f)
            return;

        int c = (int)(charge * 4) % 4; //Retrieve the current index of the charger being charged

        for (int i = 0; i < chargerRenderers.Length; i++)
        {
            if (i == c)
            {
                Color color = chargerRenderers[i].material.color;
                color.r = Mathf.Lerp(chargeEmptyColor.r, chargeFullColor.r, (charge - 0.25f * i) * 4);
                color.g = Mathf.Lerp(chargeEmptyColor.g, chargeFullColor.g, (charge - 0.25f * i) * 4);
                color.b = Mathf.Lerp(chargeEmptyColor.b, chargeFullColor.b, (charge - 0.25f * i) * 4);
                chargerRenderers[i].material.color = color;
            }
            else if (i < c) //The previous charger is fully charged
            {
                chargerRenderers[i].material.color = chargeFullColor;
            }
            else if(i > c) //The charger is still empty
            {
                chargerRenderers[i].material.color = chargeEmptyColor;
            }

        }
    }

    private void EmptyCharger()
    {
        for (int i = 0; i < chargerRenderers.Length; i++)
        {
            chargerRenderers[i].material.color = chargeEmptyColor;
        }

        charge = 0f;
    }

    private void SetClear()
    {
        if (patrolState == PatrolHandler.PatrolState.CLEAR)
            return;

        patrolState = PatrolHandler.PatrolState.CLEAR;
        eyes.material.color = clearColor;
        FOV.SetClear();
    }

    private void SetWarning()
    {
        if (patrolState == PatrolHandler.PatrolState.WARNING)
            return;

        patrolState = PatrolHandler.PatrolState.WARNING;
        eyes.material.color = warningColor;
        FOV.SetWarning();
    }

    private void SetAlert()
    {
        if (patrolState == PatrolHandler.PatrolState.ALERT)
            return;

        patrolState = PatrolHandler.PatrolState.ALERT;
        eyes.material.color = alertColor;
        FOV.SetAlert();
    }

    private IEnumerator StrongAttackAnimCoroutine()
    {
        float t = 0f;

        //The laser will reach the player's last known position
        Vector3 playerPos = PlayerManager.Instance.Player.transform.position;

        //Start rotating around itself while facing the player's last known position
        while (t<=strongAtkRotTime/2)
        {
            body.localEulerAngles = new Vector3(0f, 0f, body.localEulerAngles.z + Time.deltaTime * bodyMinRotSpeed);

            yield return 0;
            t += Time.deltaTime;

            if(patrolState == PatrolHandler.PatrolState.ALERT) //If the player is still visible, update their position
                playerPos = PlayerManager.Instance.Player.transform.position;
        }

        float scale = 0.5f;
        float timeScale = 1f / t;

        //Make the energy orb appear
        strongLaserPivot.localScale = new Vector3(scale,scale,scale);
        strongLaser.SetActive(true);

        //Increase rotation speed and enlarge the energy orb
        while (t <= strongAtkRotTime)
        {
            body.localEulerAngles = new Vector3(0f, 0f, body.localEulerAngles.z + Time.deltaTime * bodyMaxRotSpeed);

            scale = Mathf.Lerp(0.5f, 1f, (t - strongAtkRotTime / 2)); 
            strongLaserPivot.localScale = new Vector3(scale,scale,scale);

            yield return 0;
            t += Time.deltaTime;

            if (patrolState == PatrolHandler.PatrolState.ALERT) //If the player is still visible, update their position
                playerPos = PlayerManager.Instance.Player.transform.position;
        }

        //Stop rotating around the Y axis (equivalent to ceasing to follow the player's position)
        canRotate = false;

        //Attack:
        Utilities.HandleSound(strongLaserAS, defaultAudioMap.shootingSounds[0], Utilities.HandleSoundState.PLAY, 2);
        //Expand the energy orb towards the target (player)
        Vector3 scaleVec = strongLaserPivot.localScale;
        scaleVec.z = Mathf.Min(Vector3.Distance(transform.position, playerPos), strongLaserRange);
        strongLaserPivot.localScale= scaleVec;

        //Attack delay, during which the laser remains tangible
        while (t<=strongAtkRotTime + strongAtkLaserTime)
        {
            yield return 0;
            t += Time.deltaTime;
        }

        strongAtkCoroutine = null;
        strongLaserPivot.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        strongLaser.SetActive(false);

        body.localEulerAngles = new Vector3(0f, 0f, 45f);

        isAttackingStrongEnded = true;
    }

    private void StrongAttackAnimDispose()
    {
        if (strongAtkCoroutine != null)
        {
            StopCoroutine(strongAtkCoroutine);
            strongAtkCoroutine = null;
        }

        strongLaserPivot.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        strongLaser.SetActive(false);
        body.localEulerAngles = new Vector3(0f, 0f, 45f); 
        canRotate = true; //Around the Y axis 
    }

    private IEnumerator AlertToWarningCoroutine()
    {
        isAlertToWarningTransition = true;
        yield return new WaitForSeconds(alertToWarningDelay);
        isAlertToWarningTransition = false;
        alertToWarningCoroutine = null;
    }

    private void AlertToWarningDispose()
    {
        if(alertToWarningCoroutine != null)
        {
            StopCoroutine(alertToWarningCoroutine);
            alertToWarningCoroutine = null;
            isAlertToWarningTransition = false;
        }
    }

    //IDamageable
    public void TakeDamage(int damage)
    {
        health -= damage;
        Utilities.HandleSound(audioSource, defaultAudioMap.getKilled[1], Utilities.HandleSoundState.PLAY, 2);
        
        if (takeDamagePS.isPlaying)
            takeDamagePS.Stop();

        takeDamagePS.Clear();
        takeDamagePS.Play();
        
        if(health <= 0)
            Kill();
    }

    public void Kill()
    {
        SoundManager.Instance.GenerateTempSFX(transform.position, defaultAudioMap.getKilled[0], maxDistance: 30f);
        Utilities.GenerateTempParticleSystem(deathPS, transform.position, transform.rotation);

        deathEvent?.Invoke();
        gameObject.SetActive(false);
    }
}

/*
 * Defines a pressure plate with an integrated laser cannon, acting as a switch that triggers the firing of 
 * the cannon.
 * Upon contact with the pressure plate, if the cannon is charged, the laser will be fired and will then require
 * time to recharge.
 * The switch glows red when not charged and changes to a glowing green when fully charged.
 */

using System.Collections;
using UnityEngine;

public class FixedLaserCannon : MonoBehaviour, IStateSaveable
{
    [SerializeField] private GameObject rootGO;

    //Laser
    [SerializeField] private LineRenderer laser;
    [SerializeField] private Transform laserPivot;
    [SerializeField] private Vector3 target;
    [SerializeField] private AudioSource laserAudioSource;
    [SerializeField] private float chargeTime = 5f;
    [SerializeField] private float chargeTimer = 0f;
    [SerializeField] private float laserShowTime = 0.5f;
    [SerializeField] private int damage = 1;
    private Coroutine laserCoroutine = null;

    //PressurePlate
    [SerializeField] private MeshRenderer pressureMeshRenderer;
    [SerializeField] private Light spotLight;
    [SerializeField] private Color chargedColor = Color.green;
    [SerializeField] private Color unchargedColor = Color.red;
    [SerializeField] private AudioSource pressureAudioSource;

    private bool initialized = false;

    private void Awake()
    {
        laser.enabled = false;
        laser.positionCount = 2;
        laser.SetPosition(0, laserPivot.position);
        laser.SetPosition(1, target);
    }

    private void Start()
    {
        if (initialized)
            return;

        initialized = true;
        spotLight.color = unchargedColor;
        pressureMeshRenderer.material.color = unchargedColor;
    }

    private void OnDisable()
    {
        DisposeLaserCoroutine();
    }

    private void Update()
    {
        if (Time.deltaTime == 0 || chargeTimer >= chargeTime) //Pause or Charged
            return;
        
        chargeTimer += Time.deltaTime;

        if (chargeTimer >= chargeTime)
        {
            spotLight.color = chargedColor;
            pressureMeshRenderer.material.color = chargedColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (EngineConf.Tag.IsEntitiesBody(other.tag))
        {
            if(chargeTimer>=chargeTime)
            {
                pressureAudioSource.volume = SoundManager.Instance.GetSoundEffectsVolume();
                pressureAudioSource.Play();

                spotLight.color = unchargedColor;
                pressureMeshRenderer.material.color = unchargedColor;
                chargeTimer = 0f;

                float distance = Vector3.Distance(target, laserPivot.position);
                Vector3 dir = (target - laserPivot.position).normalized;

                RaycastHit hit;
                if (Physics.Raycast(laserPivot.position, dir, out hit, distance, EngineConf.Layer.LaserHitMask, QueryTriggerInteraction.Ignore))
                {
                    laser.SetPosition(1, hit.point);

                    //Can destroy damageable buildings
                    IDamageableComponent damageableC = hit.transform.GetComponent<IDamageableComponent>();
                    damageableC?.GetDamageable()?.TakeDamage(damage);
                }
                else
                    laser.SetPosition(1, target);
                
                DisposeLaserCoroutine();
                laserCoroutine = StartCoroutine(LaserCoroutine());
            }
        }
    }

    private IEnumerator LaserCoroutine()
    {
        laser.enabled = true;
        laserAudioSource.volume = SoundManager.Instance.GetSoundEffectsVolume();
        laserAudioSource.Play();
        yield return new WaitForSeconds(laserShowTime);
        laser.enabled = false;
    }

    private void DisposeLaserCoroutine()
    {
        if(laserCoroutine != null)
        {
            StopCoroutine(laserCoroutine);
            laserCoroutine = null;
            laser.enabled = false;
        }
    }

    public IState SaveState()
    {
        CustomEntityState state = new CustomEntityState();

        state.chargeTimer = chargeTimer;

        return state;
    }

    public void LoadState(IState state)
    {
        CustomEntityState _state = state as CustomEntityState;

        initialized = true;
        chargeTimer = _state.chargeTimer;

        if(chargeTimer>=chargeTime)
        {
            pressureMeshRenderer.material.color = chargedColor;
            spotLight.color = chargedColor;
        }
        else
        {
            pressureMeshRenderer.material.color = unchargedColor;
            spotLight.color = unchargedColor;
        }
    }

    [System.Serializable]
    public class CustomEntityState : IState
    {
        public float chargeTimer;
    }
}

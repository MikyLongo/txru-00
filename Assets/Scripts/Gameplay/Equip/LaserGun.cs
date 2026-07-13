/*
 * Defines a LaserGun equip by extending the Equip class and implementing custom logic for the Use method.
 * Implements shooting mechanics and the corresponding "animation."
 * The laser is visualized using the LineRenderer component.
 */

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserGun : Equip
{
    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private LineRenderer lineRenderer = null;
    [SerializeField] private Transform origin = null;
    [SerializeField] private float distance = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float shootTime = 0.1f;
    private Coroutine shootCoroutine = null;


    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
    }

    public override void Use()
    {
        lineRenderer.SetPosition(0, origin.position);
        Vector3 dir = origin.forward;
        RaycastHit hit;
        if(Physics.Raycast(origin.position, dir, out hit, distance, 
            EngineConf.Layer.LaserHitMask & ~(1 << gameObject.layer),
            QueryTriggerInteraction.Ignore)
        )
        {
            lineRenderer.SetPosition(1, hit.point);
            //Capable of destroying buildings that are damageable.
            IDamageableComponent damageableC = hit.transform.GetComponent<IDamageableComponent>();
            damageableC?.GetDamageable()?.TakeDamage(damage);
        }
        else
        {
            lineRenderer.SetPosition(1, origin.position + dir*distance);
        }
        ShootCoroutineDispose();
        shootCoroutine = StartCoroutine(ShootCoroutine());
    }

    private IEnumerator ShootCoroutine()
    {
        lineRenderer.enabled = true;
        audioSource.volume = SoundManager.Instance.GetSoundEffectsVolume();
        audioSource.Play();
        yield return new WaitForSeconds(shootTime);
        lineRenderer.enabled = false;
        shootCoroutine = null;
    }

    private void ShootCoroutineDispose()
    {
        if(shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
            shootCoroutine = null;
            lineRenderer.enabled = false;
        }
    }
}

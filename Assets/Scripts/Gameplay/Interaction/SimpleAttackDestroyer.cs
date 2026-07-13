/*
 * A script that allows a GameObject to inflict damage on a DamageableComponent through physical (trigger-based) 
 * contact.
 */

using UnityEngine;

public class SimpleAttackDestroyer : MonoBehaviour
{
    [SerializeField] private int damage;

    private void OnTriggerEnter(Collider other)
    {
        IDamageableComponent damageableC = other.GetComponent<IDamageableComponent>();
        damageableC?.GetDamageable()?.TakeDamage(damage);
    }
}

//Wrapper class for the IDamageableComponent interface
using UnityEngine;

public class DamageableBodyPart : MonoBehaviour, IDamageableComponent
{
    /*
     * Unity does not support the serialization of interface instances in the Inspector. 
     * Therefore, we utilize damageableGO to retrieve the IDamageable instance.
     */
    [SerializeField] private GameObject damageableGO = null;
    [SerializeField] private IDamageable damageable = null;

    private void Awake()
    {
        damageable = damageableGO.GetComponent<IDamageable>();
    }

    public IDamageable GetDamageable()
    {
        return damageable;
    }

    private void OnValidate() //Validate input from the Inspector
    {
        if (damageableGO == null)
        {
            damageable = null;
            return;
        }

        damageable = damageableGO.GetComponent<IDamageable>();
        if (damageable == null)
            damageableGO = null;
    }
}

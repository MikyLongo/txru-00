/*
 * Script that defines a damageable building and manages its life cycle.
 * A damageable building can drop an object upon destruction.
 */

using UnityEngine;

public class DamageableBuilding : MonoBehaviour, IStateSaveable, IDamageable
{
    [SerializeField] private GameObject rootGO;
    [SerializeField] private AudioClip destructionClip;
    [SerializeField] private int health = 1;
    [SerializeField] private GameObject dropPrefab;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
            Kill();
    }

    public void Kill()
    {
        if(destructionClip != null)
            SoundManager.Instance.GenerateTempSFX(rootGO.transform.position, destructionClip, maxDistance: 15f);

        if(dropPrefab != null) //Instantiate the drop
        {
            Instantiate(dropPrefab, rootGO.transform.position, Quaternion.identity);
        }

        rootGO.SetActive(false);
    }

    public IState SaveState()
    {
        CustomEntityState state = new CustomEntityState();
        state.health = health;
        return state;

        /*
         * TODO: Handle the save state of the drop.
         * Ideally, the state of the drop (looted/unlooted) should be managed when the building is destroyed
         * and drops an item. Currently, this functionality is not implemented.
         * This omission does not cause issues because buildings with drops are not used in levels with
         * checkpoints, so their state does not need to be saved.
         */
    }

    public void LoadState(IState state)
    {
        CustomEntityState _state = state as CustomEntityState;
        health = _state.health;
        if(health <= 0)
            rootGO.SetActive(false);
    }

    [System.Serializable]
    public class CustomEntityState : IState
    {
        public int health;
    }
}

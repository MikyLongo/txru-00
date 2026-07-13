using UnityEngine;

public class DestructibleWall : MonoBehaviour, IDamageable, IStateSaveable
{
    [SerializeField] private GameObject wall;
    [SerializeField] private AudioClip destructionClip = null;
    [SerializeField] private int health = 1;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if(health <= 0)
            Kill();
    }

    public void Kill()
    {
        if(destructionClip != null)
            SoundManager.Instance.GenerateTempSFX(wall.transform.position, destructionClip);

        wall.SetActive(false);
    }

    public IState SaveState()
    {
        CustomEntityState state = new CustomEntityState();

        state.health = health;

        return state;
    }

    public void LoadState(IState state)
    {
        CustomEntityState _state = state as CustomEntityState;
        
        health = _state.health;
        
        if(health <= 0)
            wall.SetActive(false);
    }

    [System.Serializable]
    public class CustomEntityState : IState
    {
        public int health;
    }
}

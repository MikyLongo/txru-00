using UnityEngine;

public class OutOfBoundZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (EngineConf.Layer.IsInMask(other.gameObject.layer, EngineConf.Layer.AliveEntityMask))
        {
            if (other.gameObject.layer == EngineConf.Layer.PLAYER)
            {
                LevelManager.Instance.GameOver(false);
                return;
            }

            IDamageable damageable = other.transform.root.GetComponentInChildren<IDamageable>();
            if(damageable != null)
            {
                damageable.Kill();
            }
            else
                other.transform.root.gameObject.SetActive(false);
        }
    }
}

//Script that manages the player's interaction with the checkpoint.

using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private ParticleSystem ps;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(EngineConf.Tag.PLAYER))
        {
            LevelManager.Instance.CheckPointReached();
            SoundManager.Instance.GenerateTempSFX(transform.position,clip);
            Utilities.GenerateTempParticleSystem(ps,transform.position,transform.rotation); 
            gameObject.SetActive(false);
        }
    }
}

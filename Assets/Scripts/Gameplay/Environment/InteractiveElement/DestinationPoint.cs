//Script that manages the player's interaction with the destination and provides a way to enable or disable
//the destination.
using UnityEngine;

public class DestinationPoint : MonoBehaviour
{
    public void EnableDestination(bool enable)
    {
        GetComponent<Animator>().enabled = enable;
        GetComponent<BoxCollider>().enabled = enable;
        transform.GetChild(1).gameObject.SetActive(enable);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(EngineConf.Tag.PLAYER))
            LevelManager.Instance.EndReached();
    }
}

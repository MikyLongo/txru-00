/*
 * This is a component of the SwitchLaserTrap and represents the laser emitted by the trap.
 * It manages its interaction with the Player, causing an instant Game Over upon contact, and provides 
 * a method to enable or disable the laser (it is a trigger collider in a GameObject displaying a LineRenderer).
 * See the LaserEmitter class for more information.
 */


using UnityEngine;

public class LaserInteraction : MonoBehaviour
{
    public void EnableInteraction(bool enable)
    {
        gameObject.SetActive(enable);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == EngineConf.Layer.PLAYER && other.CompareTag(EngineConf.Tag.PLAYER))
        {
            PlayerManager.Instance.Kill();
        }
    }
}

/*
 * This is a component of the SwitchLaserTrap and represents the pressure plate.
 * Upon contact, the pressure plate triggers the activation of all lasers in the LaserEmitter 
 * (see the LaserEmitter class for more information).
 */

using UnityEngine;

public class SwitchLaserPlate : MonoBehaviour
{
    [SerializeField] private LaserEmitter laserEmitter;

    private void OnTriggerEnter(Collider other)
    {
        if (
            !EngineConf.Tag.IsPlayerInteractionToIgnore(other.transform.tag) &&
            other.gameObject.layer == EngineConf.Layer.PLAYER
        )
        {
            laserEmitter.ActiveAllLaser();
        }
    }
}

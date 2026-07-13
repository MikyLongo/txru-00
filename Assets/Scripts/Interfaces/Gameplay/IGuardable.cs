/*
 * Interface that defines a guardable entity.
 * A guardable entity is one that must be kept safe by another entity.
 * For more information, see the GuardHandler and GuardBehaviour classes.
 */

using UnityEngine;

public interface IGuardable 
{
    public bool IsSecure(); //Indicates whether the guarded entity is safe (not lost, not dead if alive, etc.)

    public Vector3 Position { get; }
}

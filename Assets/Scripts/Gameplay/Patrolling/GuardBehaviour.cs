/*
 * Defines the entity to guard (see IGuardable) and the event to trigger when the entity is not safe/secure.
 */
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GuardBehaviour : IStateSaveable
{
    /*
     * Unity does not support serialization of interface instances from the Inspector. 
     * Therefore, the GameObject goEntity is defined, from which the IGuardable is extracted.
     */
    public GameObject goEntity = null; 
    private IGuardable entity = null;  
    public UnityEvent eventResponse = null; //Event triggered when the entity is not secured
    public bool completed = false; //true: No further need to guard it

    public IGuardable Entity {  get { return entity; } }

    public void InvokeResponse()
    {
        eventResponse?.Invoke();
        completed = true;
    }

    //Validates the goEntity field
    public bool Validate() //Returns: True => Valid configuration | False => Invalid configuration
    {
        if (goEntity != null)
        {
            entity = goEntity.GetComponentInChildren<IGuardable>();
            if (entity != null)
                return true;

            goEntity = null;
        }

        entity = null;
        return false;
    }

    //Entity State
    public IState SaveState()
    {
        CustomEntityState state = new CustomEntityState();

        state.completed = completed;

        return state;
    }

    public void LoadState(IState state)
    {
        CustomEntityState _state = state as CustomEntityState;

        completed = _state.completed;
    }

    [System.Serializable]
    private class CustomEntityState : IState
    {
        [SerializeField] public bool completed;
    }
}

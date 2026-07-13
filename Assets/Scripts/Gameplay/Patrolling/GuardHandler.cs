/*
 * MonoBehaviour that defines a list of entities to guard and the behavior to adopt 
 * (see GuardBehaviour) while performing guard duties for a patroller or guard.
 */

using System.Collections.Generic;
using UnityEngine;

public class GuardHandler : MonoBehaviour, IStateSaveable
{
    [SerializeField] private List<GuardBehaviour> infos = null;
    
    public List<GuardBehaviour> Infos { get { return infos; } }

    public bool HasGuardDuty()
    {
        if (infos != null && infos.Count > 0)
        {
            foreach(GuardBehaviour info in infos)
            {
                if (!info.completed)
                    return true;
            }
        }

        return false;
    }

    private void Awake()
    {
        ValidateList();
    }

    private void OnValidate() //Invoked by the inspector when changes occur
    {
        ValidateList();
    }

    private void ValidateList() // Ensures the list is properly populated
    {
        if (infos == null || infos.Count == 0)
            return;

        //Used to validate Infos configurations
        foreach (GuardBehaviour info in infos)
            info.Validate();
    }

    //EntityState
    public IState SaveState()
    {
        CustomEntityState state = new CustomEntityState();

        if (infos == null || infos.Count == 0)
            state.infosState = null;
        else
        {
            state.infosState = new List<IState>();
            foreach(GuardBehaviour info in infos)
            {
                state.infosState.Add(info.SaveState());
            }
        }

        return state;
    }

    public void LoadState(IState state)
    {
        CustomEntityState _state = state as CustomEntityState;

        if(_state != null )
        {
            for(int i=0; i<_state.infosState.Count; i++)
            {
                infos[i].LoadState(_state.infosState[i]);
            }
        }
    }

    [System.Serializable]
    private class CustomEntityState : IState
    {
        [SerializeReference] public List<IState> infosState;
    }
}



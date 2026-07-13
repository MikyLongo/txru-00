/*
 * MonoBehaviour enabling event generation during gameplay.
 * Handles events related to Patrollers.
 * - Event: Change the patrol route and patrol state for a single patroller or a group of patrollers.
 *
 * Note: A UnityEvent will be defined externally to call one of the methods in this script.
 * See also: PatrolHandler and GuardHandler for complementary functionality.
 */

using System.Collections.Generic;
using UnityEngine;

public class ChangePatrolEvent : MonoBehaviour
{
    [SerializeField] private List<GameObject> patrollers = null; //Must implement IPatrol interface
    [SerializeField] private List<PatrolHandler> patrolsRoute = null;
    [SerializeField] private PatrolHandler.PatrolState patrolState = PatrolHandler.PatrolState.CLEAR;

    /*
     * patrollers: A list of GameObjects that must implement the IPatrol interface 
     * (Unity does not support interface serialization via the inspector).
     * patrolRoute: A list of new patrol routes (PatrolHandler).
     * patrolState: The patrol state to be assumed.
     * The elements of the two lists are matched by their respective indices.
     */

    public void ChangeSinglePatrol()
    {
        if(CheckError("ChangeSinglePatrol: Invalid parameters!"))
            return;

        IPatrol patrol = patrollers[0].GetComponentInChildren<IPatrol>();
        if(patrol != null )
            patrol.ChangePatrol(patrolsRoute[0], patrolState);
    }

    public void ChangeMultiplePatrol()
    {
        if(CheckError("ChangeMultiplePatrol: Invalid parameters!"))
            return;

        for(int i = 0; i< patrollers.Count; i++)
        {
            IPatrol patrol = patrollers[i].GetComponentInChildren<IPatrol>();
            if (patrol != null)
                patrol.ChangePatrol(patrolsRoute[i], patrolState);
        }
    }

    private bool CheckError(string msg)
    {
        if (patrollers == null || patrolsRoute == null || patrollers.Count == 0 || patrollers.Count != patrolsRoute.Count)
        {
            Debug.LogWarning(msg);
            return true;
        }
        
        return false;
    }
}

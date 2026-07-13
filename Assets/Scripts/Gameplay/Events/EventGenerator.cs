/*
 * MonoBehaviour enabling event generation during gameplay.
 * Acts as a collider that triggers UnityEvents defined with objects of type EventToGenerate, 
 * based on interactions with specified entities or the player.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventGenerator : MonoBehaviour, IStateSaveable
{
    [SerializeField] private EventToGenerate eventToGenerate;

    private void OnTriggerEnter(Collider other)
    {
        if (!eventToGenerate.whenEntering)
            return;

        HandleInteraction(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (eventToGenerate.whenEntering)
            return;

        HandleInteraction(other);
    }

    private void HandleInteraction(Collider other)
    {
        if(eventToGenerate.fromPlayer)
        {
            //if (other.transform.root.gameObject.Equals(PlayerManager.Instance.Player.gameObject))
            if (
                !EngineConf.Tag.IsPlayerInteractionToIgnore(other.transform.tag) &&
                other.transform.root.gameObject.Equals(PlayerManager.Instance.Player.gameObject)
            )
            {
                eventToGenerate.uEvent?.Invoke();

                if(eventToGenerate.numTimes > 0)
                {
                    eventToGenerate.numTimes--;

                    if(eventToGenerate.numTimes == 0)
                        gameObject.SetActive(false);
                }
                //else, allows infinite usage (only when < 0). A value of 0 will trigger SetActive(false).
            }
        }
        else
        {
            if(eventToGenerate.entities.Contains(other.transform.root.gameObject))
            {
                eventToGenerate.uEvent?.Invoke();

                if (eventToGenerate.numTimes > 0)
                {
                    eventToGenerate.numTimes--;

                    if (eventToGenerate.numTimes == 0)
                        gameObject.SetActive(false);
                }
                //else, allows infinite usage (only when < 0). A value of 0 will trigger SetActive(false).
            }
        }
    }

    [System.Serializable]
    public class EventToGenerate
    {
        public UnityEvent uEvent;
        public bool whenEntering; //false = when exiting
        public int numTimes; //<0 = infinite, 0 = ended, 1,2,3.... finite
        public bool fromPlayer;
        //If fromPlayer is false, this specifies the list of entities that can generate the event
        public List<GameObject> entities;
    }

    //Custom Entity
    public IState SaveState()
    {
        CustomEntityState state = new CustomEntityState();

        state.on = gameObject.activeSelf;
        state.numTimes = eventToGenerate.numTimes;

        return state;
    }

    public void LoadState(IState state)
    {
        CustomEntityState _state = state as CustomEntityState;

        eventToGenerate.numTimes = _state.numTimes;
        gameObject.SetActive(_state.on);
    }

    [System.Serializable]
    private class CustomEntityState : IState
    {
        public bool on;
        public int numTimes;
    }
}

/*
 * MonoBehaviour enabling event generation during gameplay.
 * Handles events related to Door (see IDoorInteractable interface)
 * - Event: Toggles the state (open/close) of each door in the specified list.
 *
 * Note: A UnityEvent will be defined elsewhere to trigger one of the methods in this script.
 */

using System.Collections.Generic;
using UnityEngine;

public class ChangeDoorStateEvent : MonoBehaviour
{
    [SerializeField] private List<DoorEvent> doorEvents;
    public void InvokeDoorEvents()
    {
        foreach(DoorEvent de in doorEvents)
        {
            if (de.open)
                de.door.Open();
            else
                de.door.Close();
        }
    }

    private void Awake()
    {
        foreach(DoorEvent de in doorEvents)
            de.Validate();
    }

    private void OnValidate()
    {
        foreach (DoorEvent de in doorEvents)
            de.Validate();
    }

    [System.Serializable]
    public class DoorEvent
    {
        public GameObject doorGO;
        public IDoorInteractable door;
        public bool open;

        public void Validate()
        {
            if(doorGO == null)
                door = null;
            else
            {
                door = doorGO.GetComponentInChildren<IDoorInteractable>();
                if(door == null)
                    doorGO = null;
            }
        }
    }
}

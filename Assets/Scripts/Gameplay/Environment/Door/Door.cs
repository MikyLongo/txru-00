/*
 * Since a "Door" can consist of multiple IDoorInteractable components, with these doors being nested 
 * child GameObjects, this class acts as a wrapper to interact with all of them collectively.
 */

using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IDoorInteractable
{
    [SerializeField] private List<DoorWrapper> doorWrappers;

    private void Awake()
    {
        Validate();
    }

    private void OnValidate()
    {
        Validate();
    }

    private void Validate()
    {
        foreach (DoorWrapper wrapper in doorWrappers)
        {
            if (wrapper.doorGO == null)
                wrapper.door = null;
            else
            {
                wrapper.door = wrapper.doorGO.GetComponentInChildren<IDoorInteractable>();
                if (wrapper.door == null)
                    wrapper.doorGO = null;
            }
        }
    }

    public void Open()
    {
        foreach(DoorWrapper wrapper in doorWrappers)
        {
            if (wrapper.reverseLogic)
                wrapper.door?.Close();
            else
                wrapper.door?.Open();
        }
    }

    public void Close()
    {
        foreach (DoorWrapper wrapper in doorWrappers)
        {
            if (wrapper.reverseLogic)
                wrapper.door?.Open();
            else
                wrapper.door?.Close();
        }
    }

    [System.Serializable]
    public class DoorWrapper
    {
        public GameObject doorGO;
        public IDoorInteractable door;
        public bool reverseLogic;
    }
}

/*
 * MonoBehaviour enabling event generation during gameplay.
 * Handles generic events related to GameObjects and MonoBehaviours.
 * - Event 1: Enables or disables MonoBehaviours in the specified list.
 * - Event 2: Enables or disables GameObjects in the specified list.
 *
 * Note: A UnityEvent will be defined externally to call one of the methods in this script.
 */

using System.Collections.Generic;
using UnityEngine;

public class GenericEvent : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> monoBehaviours;
    [SerializeField] private List<GameObject> gameObjects;

    public void EnableMonoBehaviours()
    {
        if (monoBehaviours == null || monoBehaviours.Count == 0)
            return;

        foreach (MonoBehaviour monoBehaviour in monoBehaviours)
        {
            monoBehaviour.enabled = true;
        }
    }

    public void DisableMonoBehaviours()
    {
        if (monoBehaviours == null || monoBehaviours.Count == 0)
            return;

        foreach (MonoBehaviour monoBehaviour in monoBehaviours)
        {
            monoBehaviour.enabled = false;
        }
    }

    public void EnableGameObjects()
    {
        if (gameObjects == null || gameObjects.Count == 0)
            return;

        foreach (GameObject go in gameObjects)
        {
            go.SetActive(true);
        }
    }

    public void DisableGameObjects()
    {
        if (gameObjects == null || gameObjects.Count == 0)
            return;

        foreach (GameObject go in gameObjects)
        {
            go.SetActive(false);
        }
    }
}

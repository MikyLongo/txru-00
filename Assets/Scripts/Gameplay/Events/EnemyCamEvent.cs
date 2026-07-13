/*
 * MonoBehaviour enabling event generation during gameplay.
 * Handles events specific to EnemyCam.
 * - Event: Disables all EnemyCam in the specified list.
 *
 * Note: A UnityEvent will be defined externally to invoke one of the methods in this script.
 */

using System.Collections.Generic;
using UnityEngine;

public class EnemyCamEvent : MonoBehaviour
{
    [SerializeField] private List<EnemyCam> cams;

    public void DisableCams()
    {
        if (CheckError("DisableCams: Parameter not valid"))
            return;

        foreach(EnemyCam cam in cams)
            cam.IsWorking = false;
    }

    private bool CheckError(string msg)
    {
        if(cams == null || cams.Count == 0)
        {
            Debug.LogWarning(msg);
            return true;
        }

        return false;
    }
}

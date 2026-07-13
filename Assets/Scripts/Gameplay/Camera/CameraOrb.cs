/*
 * In the game, there is only one camera (except the one for the minimap) that displays the game world on screen.
 * Since the camera must follow the player in every scene location with different settings 
 * (e.g., angle, position, FOV, etc.), we need a way to communicate these changes to the Main Camera's controller 
 * (see CameraController).
 * 
 * This script fulfills that purpose. It is associated with a trigger collider, and when the player interacts 
 * with it, the script instructs the CameraController to update its settings with the values contained here.
 * 
 * For more information on how these settings influence the CameraController, refer to the CameraController script.
 */
using UnityEngine;

[System.Serializable]
public class CameraOrb : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 minLimits = Vector3.zero;
    [SerializeField] private Vector3 maxLimits = Vector3.zero;
    [SerializeField] private Vector3 offSets = Vector3.zero;
    [SerializeField] private bool isFixed = false;
    [SerializeField] private float fov = 60f;
    [SerializeField] private float nearClipPlane = 1f;
    [SerializeField] private float farClipPlane = 1000f;
    [SerializeField] private float depth = -1;
    [SerializeField] private float transTimeFixedAxes = 0.5f;
    [SerializeField] private float transTimeVariableAxes = 0.1f;

    public Transform CameraTransform { get { return cameraTransform; } }
    public Vector3 MinLimits { get { return minLimits; } }
    public Vector3 MaxLimits { get { return maxLimits; } }
    public Vector3 OffSets { get { return offSets; } }
    public bool IsFixed { get { return isFixed; } }
    public float FOV { get { return fov; } }
    public float NearClipPlane { get { return nearClipPlane; } }
    public float FarClipPlane { get { return farClipPlane; } }
    public float Depth { get { return depth; } }
    public float TransTimeFixedAxes { get { return transTimeFixedAxes; } }
    public float TransTimeVariableAxes { get { return transTimeVariableAxes; } }

    private void OnTriggerStay(Collider other)
    {
        //The player that triggers contact with the orb must have a GameObject with this tag.
        if (other.CompareTag(EngineConf.Tag.CAMERA_INTERACTION))
        {
            LevelManager.Instance.CamController.SetCameraOrb(this);
        }
    }
}

/*
 * In every scene, there is only one camera, the MainCamera, except for the minimap camera.
 * The MainCamera must follow the player at all times, but depending on the location in the scene, it must 
 * adhere to specific rules.
 * These rules are defined by the CameraOrb (see CameraOrb class), with which the player interacts.
 * 
 * The rules affect the following:
 * - Position:
 *   The camera may be fixed or follow a path by moving along the axes within limits specified by
 *   min and max values (Vector3), with an offset from the player (distance) specified by offsets (Vector3).
 * - Angle:
 *   The rotation assumed by the camera is fixed and provided by the CameraTransform field of the CameraOrb.
 * - FOV (Field of View)
 * - Far and Near clip planes
 * - Depth
 * - TransTimeFixedAxes and TransTimeVariableAxes, which define the duration for the MainCamera to 
 *   transition from its current settings to the new settings provided by the CameraOrb.
 *   The transition is performed smoothly using a coroutine and affects position, rotation, and FOV.
 */

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour, IStateSaveable
{
    [SerializeField] private CameraOrb currentOrb = null;
    [SerializeField] private Camera mainCamera = null;
    [SerializeField] private Vector3 minLimits = Vector3.zero;
    [SerializeField] private Vector3 maxLimits = Vector3.zero;
    [SerializeField] private Vector3 offSets = Vector3.zero;
    [SerializeField] private bool isFixed = false;
    [SerializeField] private bool isInTransition = false;
    //[SerializeField] private float minStep = 0.001f;
    private Coroutine smoothCoroutine = null;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if(isFixed || isInTransition)
            return;
        
        Vector3 position = PlayerManager.Instance.Player.transform.position;
        Vector3 newPosition = new Vector3(
            Mathf.Clamp(position.x + offSets.x, minLimits.x, maxLimits.x),
            Mathf.Clamp(position.y + offSets.y, minLimits.y, maxLimits.y),
            Mathf.Clamp(position.z + offSets.z, minLimits.z, maxLimits.z)
        );

        for(int i=0; i<3; i++)
        {
            if (Mathf.Abs(transform.position[i] - newPosition[i]) < 0.03f)
                newPosition[i] = transform.position[i];
        }

        transform.position = newPosition;
    }

    public Vector3 MinLimits { get { return minLimits; } set {  minLimits = value; } }
    public Vector3 MaxLimits { get { return maxLimits; } set {  maxLimits = value; } }
    public Vector3 OffSets { get { return offSets; } set {  offSets = value; } }
    public bool IsFixed { get { return isFixed; } set { isFixed = value; } }

    public void SetCameraOrb(CameraOrb orb)
    {
        if (orb.Equals(currentOrb))
            return;

        SmoothCoroutineDispose();
        currentOrb = orb;
        smoothCoroutine = StartCoroutine(SmoothTransition());

        mainCamera.nearClipPlane = orb.NearClipPlane;
        mainCamera.farClipPlane = orb.FarClipPlane;
        mainCamera.depth = orb.Depth;
        minLimits = orb.MinLimits;
        maxLimits = orb.MaxLimits;
        offSets = orb.OffSets;
        isFixed = orb.IsFixed;
    }

    /*
     * FOV and rotation are fixed values, and the transition is handled using currentOrb.TransTimeFixedAxes.
     * If its value is 0, the transition occurs instantly.
     * Position can be fixed; in that case, currentOrb.isFixed == true, and the same logic as above is applied.
     * If the position is variable, at least one of the axes is variable.
     * For the fixed axes, the same logic as before is applied. For the variable axes, the transition
     * is handled using currentOrb.TransTimeVariableAxes. During the transition, the player's position
     * is followed, constrained by the rules defined by min and max limits and offsets.
     * If currentOrb.TransTimeVariableAxes is 0, the transition occurs instantly, maintaining the same constraints.
     */

    private IEnumerator SmoothTransition()
    {
        isInTransition = true;

        if(currentOrb.IsFixed && currentOrb.TransTimeFixedAxes == 0f) //Instant transition
        {
            transform.position = currentOrb.CameraTransform.position;
            transform.rotation = currentOrb.CameraTransform.rotation;
            mainCamera.fieldOfView = currentOrb.FOV;
        }
        else
        {
            float t = 0f;
            float timer = currentOrb.IsFixed ? currentOrb.TransTimeFixedAxes : Mathf.Max(currentOrb.TransTimeFixedAxes, currentOrb.TransTimeVariableAxes);
            float scaleFixed = (currentOrb.TransTimeFixedAxes == 0f) ? 0f : 1f / currentOrb.TransTimeFixedAxes;
            float scaleVariable = (currentOrb.TransTimeVariableAxes == 0f) ? 0f : 1f / currentOrb.TransTimeVariableAxes;

            Vector3 startingPos = transform.position;
            Quaternion startingRot = transform.rotation;
            float startingFOV = mainCamera.fieldOfView;

            while (t<timer)
            {
                //Position
                if (currentOrb.IsFixed) //All Fixed Axes
                    transform.position = Vector3.Lerp(startingPos, currentOrb.CameraTransform.position, Mathf.Clamp01(t * scaleFixed));
                else //Variable Axes (Fixed + Variable Axes)
                {
                    Vector3 playerPos = PlayerManager.Instance.Player.transform.position;
                    Vector3 newPos;

                    if (minLimits.x == maxLimits.x) //Fixed axes
                    {
                        if (scaleFixed > 0f)
                            newPos.x = Mathf.Lerp(startingPos.x, currentOrb.CameraTransform.position.x, Mathf.Clamp01(t * scaleFixed));
                        else //currentOrb.TransTimeFixedAxes == 0f (instant transition)
                            newPos.x = currentOrb.CameraTransform.position.x;
                    }
                    else //Variable axes
                    {
                        if (scaleVariable > 0f)
                        {
                            //Follow the movement of the player with a smooth movement
                            float x = Mathf.Clamp(playerPos.x + offSets.x, minLimits.x, maxLimits.x);
                            newPos.x = Mathf.Lerp(startingPos.x, x, Mathf.Clamp01(t * scaleVariable));
                        }
                        else //currentOrb.TransTimeVariableAxes == 0f (instant transition)
                        {
                            //Follow the movement of the player without a smooth movement
                            newPos.x = Mathf.Clamp(playerPos.x + offSets.x, minLimits.x, maxLimits.x);
                        }
                    }

                    if (minLimits.y == maxLimits.y) //Fixed axes
                    {
                        if (scaleFixed > 0f)
                            newPos.y = Mathf.Lerp(startingPos.y, currentOrb.CameraTransform.position.y, Mathf.Clamp01(t * scaleFixed));
                        else //currentOrb.TransTimeFixedAxes == 0f (instant transition)
                            newPos.y = currentOrb.CameraTransform.position.y;
                    }
                    else //Variable axes
                    {
                        if (scaleVariable > 0f)
                        {
                            //Follow the movement of the player with a smooth movement
                            float y = Mathf.Clamp(playerPos.y + offSets.y, minLimits.y, maxLimits.y);
                            newPos.y = Mathf.Lerp(startingPos.y, y, Mathf.Clamp01(t * scaleVariable));
                        }
                        else //currentOrb.TransTimeVariableAxes == 0f (instant transition)
                        {
                            //Follow the movement of the player without a smooth movement
                            newPos.y = Mathf.Clamp(playerPos.y + offSets.y, minLimits.y, maxLimits.y);
                        }
                    }

                    if (minLimits.z == maxLimits.z) //Fixed axes
                    {
                        if (scaleFixed > 0f)
                            newPos.z = Mathf.Lerp(startingPos.z, currentOrb.CameraTransform.position.z, Mathf.Clamp01(t * scaleFixed));
                        else //currentOrb.TransTimeFixedAxes == 0f (instant transition)
                            newPos.z = currentOrb.CameraTransform.position.z;
                    }
                    else //Variable axes
                    {
                        if (scaleVariable > 0f)
                        {
                            //Follow the movement of the player with a smooth movement
                            float z = Mathf.Clamp(playerPos.z + offSets.z, minLimits.z, maxLimits.z);
                            newPos.z = Mathf.Lerp(startingPos.z, z, Mathf.Clamp01(t * scaleVariable));
                        }
                        else //currentOrb.TransTimeVariableAxes == 0f (instant transition)
                        {
                            //Follow the movement of the player without a smooth movement
                            newPos.z = Mathf.Clamp(playerPos.z + offSets.z, minLimits.z, maxLimits.z);
                        }
                    }

                    transform.position = newPos;
                }

                if (scaleFixed > 0f)
                {
                    //Rotation
                    transform.rotation = Quaternion.Slerp(startingRot, currentOrb.CameraTransform.rotation, Mathf.Clamp01(t * scaleFixed));
                    //FOV
                    mainCamera.fieldOfView = Mathf.Lerp(startingFOV, currentOrb.FOV, Mathf.Clamp01(t * scaleFixed));
                }
                else //currentOrb.TransTimeFixedAxes == 0f (instant transition)
                {
                    //Rotation
                    transform.rotation = currentOrb.CameraTransform.rotation;
                    //FOV
                    mainCamera.fieldOfView = currentOrb.FOV;
                }

                yield return 0;
                t += Time.deltaTime;
            }

            //Ensure final states are applied

            //Position
            if (currentOrb.IsFixed) //All Fixed Axes
                transform.position = currentOrb.CameraTransform.position; 
            //else get handled in the LateUpdate

            //Rotation
            transform.rotation = currentOrb.CameraTransform.rotation;
            //FOV
            mainCamera.fieldOfView = currentOrb.FOV;
        }

        isInTransition = false;
        smoothCoroutine = null;

        yield return 0;
    }

    private void SmoothCoroutineDispose()
    {
        if(smoothCoroutine != null)
            StopCoroutine(smoothCoroutine);
        isInTransition = false;
        smoothCoroutine = null;
    }

    //Entity State
    public IState SaveState()
    {
        CustomEntityState state = new CustomEntityState();

        state.position = transform.position;
        state.rotation = transform.rotation;

        state.minLimits = minLimits;
        state.maxLimits = maxLimits;
        state.offSets = offSets;
        state.isFixed = isFixed;

        state.FOV = currentOrb.FOV;
        state.nearClipPlane = currentOrb.NearClipPlane;
        state.farClipPlane = currentOrb.FarClipPlane;
        state.depth = currentOrb.Depth;

        return state;
    }

    public void LoadState(IState state)
    {
        CustomEntityState entityState = state as CustomEntityState;

        currentOrb = null;

        transform.position = entityState.position;
        transform.rotation = entityState.rotation;

        minLimits = entityState.minLimits;
        maxLimits = entityState.maxLimits;
        offSets = entityState.offSets;
        isFixed = entityState.isFixed;

        mainCamera.fieldOfView = entityState.FOV;
        mainCamera.nearClipPlane = entityState.nearClipPlane;
        mainCamera.farClipPlane = entityState.farClipPlane;
        mainCamera.depth = entityState.depth;
    }

    [System.Serializable]
    private class CustomEntityState : IState
    {
        [SerializeField] public Vector3 position;
        [SerializeField] public Quaternion rotation;

        [SerializeField] public Vector3 minLimits;
        [SerializeField] public Vector3 maxLimits;
        [SerializeField] public Vector3 offSets;
        [SerializeField] public bool isFixed;

        [SerializeField] public float FOV;
        [SerializeField] public float nearClipPlane;
        [SerializeField] public float farClipPlane;
        [SerializeField] public float depth;
    }
}

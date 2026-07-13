/*
 * Defines a form/transformation of the Player.
 * In this form, the Player becomes a Box, allowing access to areas that would normally be inaccessible.
 * However, caution is required: if the Player moves in front of an enemy or occupies a location that interferes
 * with an enemy, they can be killed.
 * In Box form, the Player cannot jump or use items/equipment.
 */
using UnityEngine;

public class PlayerBox : Player
{
    private int smallPassage = 0;
    /*
     * smallPassage must be >= 0.
     * If > 0, it indicates that the Player is inside a small passage (e.g., a Tunnel) and cannot transform 
     * into another form. Conversely, if it is equal to 0, the Player can transform into other forms.
     */

    private void Update()
    {
        if (Time.timeScale == 0) //The game is paused
            return;

        previousPosition = transform.position;
        previousRotation = transform.rotation;

        HandleGravity();
        HandleRotation();

        //Movement
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(EngineConf.Tag.SMALL_PASSAGE))
            smallPassage++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(EngineConf.Tag.SMALL_PASSAGE))
            smallPassage--;
    }

    public override bool CanTransform
    {
        get
        {
            return canTransform && (smallPassage == 0);
        }
    }

    //Entity State
    public override IState SaveState()
    {
        CustomEntityState state = new CustomEntityState();

        //Transform
        state.position = transform.position;
        state.rotation = transform.rotation;
        state.scale = transform.localScale;

        //Movement
        state.velocity = velocity;
        state.rotationDir = rotationDir;
        state.speed = speed;
        state.rotationSpeed = rotationSpeed;

        //Gravity
        state.gravityMultiplier = gravityMultiplier;
        state.isFalling = isFalling;
        state.isGrounded = isGrounded;
        state.hReached = hReached;

        //Player settings
        state.useGravity = useGravity;
        state.isVisible = isVisible;
        state.canMove = canMove;
        state.canRotate = canRotate;
        state.canTransform = canTransform;

        //Sound
        state.currentAudioMap = currentAudioMap;

        return state;
    }

    public override void LoadState(IState state)
    {
        CustomEntityState entityState = state as CustomEntityState;

        controller.enabled = false;

        //Transform
        transform.position = entityState.position;
        transform.rotation = entityState.rotation;
        transform.localScale = entityState.scale;

        //Movement
        velocity = entityState.velocity;
        rotationDir = entityState.rotationDir;
        speed = entityState.speed;
        rotationSpeed = entityState.rotationSpeed;
        previousPosition = entityState.position;
        previousRotation = entityState.rotation;

        //Gravity
        gravityMultiplier = entityState.gravityMultiplier;
        isFalling = entityState.isFalling;
        isGrounded = entityState.isGrounded;
        hReached = entityState.hReached;

        //Player settings
        useGravity = entityState.useGravity;
        isVisible = entityState.isVisible;
        canMove = entityState.canMove;
        canRotate = entityState.canRotate;
        canTransform = entityState.canTransform;

        //Sound
        currentAudioMap = entityState.currentAudioMap == null? defaultAudioMap : entityState.currentAudioMap;

        controller.enabled = true;
    }

    [System.Serializable]
    private class CustomEntityState : IState
    {
        //Transform
        [SerializeField] public Vector3 position;
        [SerializeField] public Quaternion rotation;
        [SerializeField] public Vector3 scale;

        //Movement
        [SerializeField] public Vector3 velocity;
        [SerializeField] public Vector3 rotationDir;
        [SerializeField] public float speed;
        [SerializeField] public float rotationSpeed;

        //Gravity
        [SerializeField] public float gravityMultiplier;
        [SerializeField] public bool isFalling;
        [SerializeField] public bool isGrounded;
        [SerializeField] public float hReached;

        //Player settings
        [SerializeField] public bool useGravity;
        [SerializeField] public bool isVisible;
        [SerializeField] public bool canMove;
        [SerializeField] public bool canRotate;
        [SerializeField] public bool canTransform;

        //Sound
        [SerializeField] public ActionAudioMapper currentAudioMap;
    }
}

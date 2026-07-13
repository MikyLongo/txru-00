/*
 * Abstract class that defines a generic Player form/transformation.
 * The Player uses a CharacterController, can take damage, and has a state.
 * Note: The Player has a state, but it is not complete because the Player (and actually its derived child classes)
 * contains the state information of the form/transformation it is assuming, while the PlayerManager contains 
 * the general state common to all Player forms/transformations.
 * For this reason, the TakeDamage method of the IDamageable interface calls the TakeDamage method of the 
 * PlayerManager, as the update of the health state is managed by the PlayerManager.
 */

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public abstract class Player : MonoBehaviour, IStateSaveable, IDamageable
{
    //PlayerType
    [SerializeField] protected PlayerType playerType = PlayerType.BOT;

    //Movement
    protected CharacterController controller;
    protected Vector3 velocity = Vector3.zero;                  //Current velocity
    protected Vector3 rotationDir = Vector3.zero;               //Current rotating direction
    [SerializeField] protected float speed = 5f;                //Movement speed
    [SerializeField] protected float rotationSpeed = 15f;       //Rotation speed
    protected Vector3 previousPosition = Vector3.zero;          //Previous position
    protected Quaternion previousRotation = Quaternion.identity;//Previous rotation
    //Gravity
    [SerializeField] protected float gravity = -9.81f;        //Gravity
    [SerializeField] protected float groundedGravity = -0.5f; //Used to apply gravity when the Player is grounded
    [SerializeField] protected float gravityMultiplier = 1f;  //Used to alter the gravity state without changing the gravity value itself
    [SerializeField] protected float baseMultiplier = 1f;     //Default multiplier
    protected bool isFalling = false;
    protected float hReached = float.NaN;                    //Used to determine the maximum height reached before falling
    [SerializeField] protected float bigFallDistance = 0.5f; //Used to trigger a loud sound effect if the Player falls from a great height
    [SerializeField] protected bool isGrounded = true;
    [SerializeField] protected BoxCollider groundBox = null;
    /*
     * The groundBox is a disabled collider that provides information about the box size, which is defined in 
     * the inspector, and is used solely to create a custom check to determine if the Player is grounded.
     * The CharacterController's ground check doesn't work properly, as its values are based solely on the 
     * Player's movement and do not account for other scenarios (e.g., the Player is stationary, but the ground 
     * disappears).
     */


    //Action pressed
    protected bool movePressed;

    //Player settings
    [SerializeField] protected bool canMove = true;
    [SerializeField] protected bool canRotate = true;
    [SerializeField] protected bool useGravity = true;
    [SerializeField] protected bool isVisible = true;
    [SerializeField] protected bool canTransform = true;
    [SerializeField] protected List<Transform> boundPoints;
    /*
     * Bound points are used to determine the geometry of the Player.
     * They are utilized by the FOVHandler class in the CheckFOV method to assess whether the Player is visible.
     * In brief, checks using OverlapSphere, etc., only determine the pivot position of the Player/Collider.
     * If we simply use a raycast on the pivot, it may conclude that the Player is not visible because a building obstructs the view.
     * However, the obstruction might only cover part of the Player while other parts remain visible.
     * To account for this, the visibility check performs raycasts for each bound point.
     * For more details, refer to the FOVHandler class.
     */

    //Sound
    [SerializeField] protected AudioSource audioSource = null;
    [SerializeField] protected ActionAudioMapper defaultAudioMap = null;
    [SerializeField] protected ActionAudioMapper currentAudioMap = null;

    protected virtual void Awake()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        velocity.y = groundedGravity;
        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }

    //Player Action
    public virtual void Move(Vector2 move) //Does not affect the Y coordinate, as it is influenced by gravity
    {

        if (canMove)
        {
            velocity.x = move.x * speed;
            velocity.z = move.y * speed;
            movePressed = move.sqrMagnitude > 0;
        }
        else
        {
            velocity.x = velocity.z = 0f;
            movePressed = false;
        }

        if (canRotate)
        {
            rotationDir.x = move.x;
            rotationDir.z = move.y;
            rotationDir = rotationDir.normalized;
        }
        else
            rotationDir = Vector3.zero;
    }

    //Properties
    public virtual bool IsVisible { get { return isVisible; } set { isVisible = value; } }
    public virtual bool CanMove { get { return canMove; } set { canMove = value; } }
    public virtual bool IsMoving { 
        get 
        {
            //return movePressed || isFalling; 

            float angleDiff = Quaternion.Angle(transform.rotation, previousRotation);
            return Vector3.Distance(transform.position, previousPosition) > 0.01f || angleDiff > 0.01f;
        } 
    }
    public virtual bool CanRotate { get { return canRotate; } set { canRotate = value; } }
    public virtual bool UseGravity { get { return useGravity; } set { useGravity = value; } }
    public virtual bool CanTransform { get { return canTransform; } } 
    public virtual List<Transform> BoundPoints { get { return boundPoints; } }
    public virtual Bounds ControllerBounds { get { return controller.bounds; } }
    public virtual float ControllerRadius { get { return controller.radius; } }
    public virtual float Height { get { return controller.height; } }

    //Enums
    public enum PlayerPart //Defines the possible body parts of the Player; used for attaching equipment
    {
        ROOT,
        BODY,
        EYES,
        BACK,
        MOUTH,
        LEGS,
        RLEG,
        LLEG,
        RFOOT,
        LFOOT
    }

    public enum PlayerType
    {
        BOT,
        BOX
    }

    public PlayerType GetPlayerType(System.Type type)
    {
        if (type == typeof(PlayerBot))
            return PlayerType.BOT;
        else
            return PlayerType.BOX;
    }

    //Functions
    protected virtual void HandleRotation()
    {
        //Rotation
        if (canRotate && rotationDir.magnitude > 0)
        {
            //Gets the current rotation, which defines the Player's current direction
            Quaternion current = transform.rotation;
            //Gets the rotation corresponding to the direction to look at
            Quaternion target = Quaternion.LookRotation(rotationDir);
            //Interpolates the rotation
            //transform.rotation = Quaternion.RotateTowards(current, target, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(current, target, rotationSpeed * Time.deltaTime);
        }
    }

    protected virtual bool CheckIfGrounded()
    {
        isGrounded = Physics.CheckBox(groundBox.transform.position, groundBox.size/2, transform.rotation, EngineConf.Layer.GroundMask,QueryTriggerInteraction.Ignore);

        return isGrounded;
    }
    
    protected virtual void HandleGravity()
    {
        CheckIfGrounded();

        if(!useGravity)
            return;

        if(isGrounded && velocity.y < 0)  //velocity.y <0 :Prevents overriding the jump setup by checking the Y
        {                                 //coordinate (if Y > 0, the Player is jumping)
            if (isFalling)  //Is grounded and was falling = Landed
            {
                //Is it a big fall? Make sound
                if(!float.IsNaN(hReached) && Mathf.Abs(hReached-transform.position.y) >= bigFallDistance) 
                    Utilities.HandleSound(audioSource, currentAudioMap.jumpLand, Utilities.HandleSoundState.PLAY, 2);
            }

            velocity.y = groundedGravity;
            gravityMultiplier = baseMultiplier;
            isFalling = false;
            hReached = float.NaN;
        }
        else
        {
            velocity.y += gravity * gravityMultiplier * Time.deltaTime;
            isFalling = true;
            if(float.IsNaN(hReached))
                hReached = transform.position.y;
            else
            {
                hReached = (hReached > transform.position.y) ? hReached : transform.position.y;
            }
        }
    }

    //Entity State
    public abstract IState SaveState();

    public abstract void LoadState(IState state);
    
    //IDamageable
    public virtual void TakeDamage(int damage)
    {
        PlayerManager.Instance.TakeDamage(damage);
    }

    public virtual void Kill() //Called by the PlayerManager
    {
        Utilities.HandleSound(audioSource, currentAudioMap.death, Utilities.HandleSoundState.PLAY, 2);
    }
}

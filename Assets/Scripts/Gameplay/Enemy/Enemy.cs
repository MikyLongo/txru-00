/*
 * Abstract class representing a generic enemy.
 */

using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] protected bool initialized = false;
    //AI
    [SerializeField] protected NavMeshAgent navAgent;

    //Movement
    [SerializeField] protected CharacterController controller; 
    protected Vector3 velocity = Vector3.zero;              //Current velocity
    protected Vector3 rotationDir = Vector3.zero;           //Current rotating direction
    [SerializeField] protected float speed = 5f;            //Movement speed
    [SerializeField] protected float rotationSpeed = 15f;   //Rotation speed

    //Gravity
    [SerializeField] protected float gravity = -9.81f;      //Gravity
    [SerializeField] protected float groundedGravity = -1f; //Used to apply gravity when the Enemy is grounded
    [SerializeField] protected float gravityMultiplier = 1f;//Used to alter the gravity state without changing the gravity value itself
    [SerializeField] protected float baseMultiplier = 1f;   //Default multiplier
    protected bool isFalling = false;
    [SerializeField] protected bool isGrounded = true;
    [SerializeField] protected BoxCollider groundBox = null;
    /*
     * The groundBox is a disabled collider that provides information about the box size, which is defined in 
     * the inspector, and is used solely to create a custom check to determine if the Enemy is grounded.
     * The CharacterController's ground check doesn't work properly, as its values are based solely on the 
     * Enemy's movement and do not account for other scenarios (e.g., the Enemy is stationary, but the ground 
     * disappears).
     */

    //Settings
    [SerializeField] protected bool useGravity = true;
    [SerializeField] protected bool canMove = true;
    [SerializeField] protected bool canRotate = true;
    [SerializeField] protected bool canSee = true;
    [SerializeField] protected bool canHear = true;
    [SerializeField] protected bool isVisible = true;

    //Color of the "eyes", providing feedback on the enemy's state
    [SerializeField] protected Color clearColor = Color.blue;
    [SerializeField] protected Color warningColor = Color.yellow;
    [SerializeField] protected Color alertColor = Color.red;

    //FOV
    [SerializeField] protected FOVHandler FOV = null;

    //Sound
    [SerializeField] protected AudioSource audioSource= null;
    [SerializeField] protected EnemyAudioMapper defaultAudioMap = null;

    protected virtual void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        controller = GetComponent<CharacterController>();
    }

    //Properties
    public virtual bool UseGravity { get { return useGravity; } set { useGravity = value; } }
    public virtual bool CanMove { get { return canMove; } set { canMove = value; } }
    public virtual bool CanRotate { get { return canRotate; } set { canRotate = value; } }
    public virtual bool CanSee { get { return canSee; } set { canSee = value; } }
    public virtual bool CanHear { get { return canHear; } set { canHear = value; } }
    public virtual bool IsVisible { get { return isVisible; } set { isVisible = value; } }

    //Functions
    protected abstract void Move();

    protected virtual void HandleRotation()
    {

        if (canRotate && rotationDir != Vector3.zero)
        {
            //Gets the current rotation, which defines the Enemy's current direction
            Quaternion current = transform.rotation;
            //Gets the rotation corresponding to the direction to look at
            Quaternion target = Quaternion.LookRotation(rotationDir);
            //Interpolates the rotation
            Quaternion temp = Quaternion.RotateTowards(current, target, rotationSpeed * Time.deltaTime);

            transform.rotation = temp;
        }
    }

    protected virtual bool CheckIfGrounded()
    {
        isGrounded = Physics.CheckBox(groundBox.transform.position, groundBox.size / 2, transform.rotation, EngineConf.Layer.GroundMask, QueryTriggerInteraction.Ignore);

        return isGrounded;
    }

    protected virtual void HandleGravity()
    {
        CheckIfGrounded();

        if (!useGravity)
            return;

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = groundedGravity;
            gravityMultiplier = baseMultiplier;
            isFalling = false;
        }
        else
        {
            velocity.y += gravity * gravityMultiplier * Time.deltaTime;
            isFalling = true;
        }
    }

    protected virtual bool IsInPlace(Vector3 position, Vector3 destination)
    {
        /*
         * The character controller can provide an incorrect y position due to its calculations relative to 
         * collisions and skin width.
         * For example, a character controller grounded at y = 0 with a skin width of 0.01 will likely return 
         * y = 0.01.
         * This can be resolved by adjusting the collider settings, but it may introduce other issues.
         * To avoid such problems, the character controller will be considered at the correct position if:
         * position.x == destination.x
         * position.y is within the range [destination.y - skinWidth, destination.y + skinWidth]
         * position.z == destination.z
         */

        //Rounds the components to the fourth decimal place
        position.x = Mathf.Round(position.x * 10000f)/10000f;                  
        destination.x = Mathf.Round(destination.x * 10000f) / 10000f;
        position.y = Mathf.Round(position.y * 10000f)/10000f;                  
        destination.y = Mathf.Round(destination.y * 10000f) / 10000f;
        position.z = Mathf.Round(position.z * 10000f)/10000f;                  
        destination.z = Mathf.Round(destination.z * 10000f) / 10000f;

        return Mathf.Approximately(position.x, destination.x) && Mathf.Approximately(position.z, destination.z)
                && Mathf.Abs(position.y - destination.y) <= controller.skinWidth;
    }
}

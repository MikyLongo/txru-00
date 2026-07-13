/*
 * Defines a form/transformation of the Player.
 * This is the default form of the Player and represents a small robot.
 * In this form, the Player can jump, become invisible (if the required item is available), equip items, 
 * and use them.
 * The Player cannot attack enemies, except when equipped with a specific item.
 * Jumping produces a loud noise that may attract enemies.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerBot : Player, IJumpable, IInvisibility, IEquipable //, IDashable
{
    //Jump
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float jumpTime = 0.5f;
    private float jumpVelocity = 0f;
    private float jumpMultiplier = 1f;
    private bool  jumpPressed = false;

    //Player Equip
    [SerializeField] private GameObject pBody = null;
    [SerializeField] private GameObject pBack = null;
    [SerializeField] private GameObject pMouth = null;
    /*
     * These are references to the default body parts of the player, which can be modified when equipping items
     * or when equipment is attached to them.
     * Currently, the PlayerBot attaches equipment only to the body, such as the mouth or back.
     */

    private Dictionary<int,Equip> equipments = new Dictionary<int,Equip>();
    //This dictionary contains all the equipment that has been obtained and is available

    //Animation
    private Animator animator = null;
    //Animation hash: (Optimized hash for accessing to animator parameter)
    private int isWalkingHash;
    private int isJumpingHash;
    private int isSRLaserHash;
    //Animation state:
    private bool isWalkingAnim = false;
    private bool isJumpingAnim = false;
    private bool isSRLaserAnim = false;
    //Animation event: Jump
    private bool jumpEnded = true;
    //Animation event: Shooting
    private bool shootingStarted = false;
    private bool shootingReady = false;
    private bool shootingEnded = true;
    //Animation event: Shooting Laser (Single - Recoil)
    private bool shootingSRLaser = false;

    //Sound/Noise handler
    [SerializeField] private SphereCollider soundNoise = null;
    //This collider enables interaction with other entities, allowing them to "hear" the noise.
    [SerializeField] private bool audioPaused = false;
    //The game is paused by setting Time.timeScale to 0, but this doesn't affect the AudioSource.
    //For this reason, this boolean is used to track whether the sound has been paused or not.

    //Visibility handler
    private float invisibilityTime = -1f;
    [SerializeField] private bool canGoInvisible = true;
    private Dictionary<Material, Tuple<MyExtension.BlendMode,float,float,float>> meshColors = new();
    /*
     * This dictionary keeps track of each Material that composes the Player (default GameObject + equipped GameObject).
     * For each material, it stores the BlendMode defined by the MyExtension class, the alphaValue, the metallicValue,
     * and the smoothnessValue.
     * When the Player becomes invisible, all materials that compose them change their blending mode to invisible 
     * and adjust these values accordingly.
     * When the Player becomes visible again, all changes must be reverted back.
     * This functionality is implemented in the "Actions: Go Invisible" section.
     * For more information, see the Materials section in the MyExtension class.
     */

    //Coroutines
    Coroutine noiseCoroutine = null;
    Coroutine invisiblityCoroutine = null;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        isWalkingHash = Animator.StringToHash("isWalking");
        isJumpingHash = Animator.StringToHash("isJumping");
        isSRLaserHash = Animator.StringToHash("isSRLaser");
        Utilities.SetupJumpValues(jumpTime, jumpHeight, gravity, out jumpMultiplier, out jumpVelocity);
        GetMeshColor();
    }

    private void OnDisable()
    {
        NoiseCoroutineDispose();
        InvisibilityCoroutineDispose();
        Utilities.HandleSound(audioSource, null, Utilities.HandleSoundState.STOP, 0); //Stop sound
        InitializeAnimator();
        audioPaused = false;
    }

    private void Update()
    {
        if (Time.timeScale == 0) //Game paused
        {
            //Pause SFX
            if(audioSource.isPlaying)
            {
                audioSource.Pause();
                audioPaused = true;
            }
            return;
        }
        else
        {
            if (audioPaused) //Unpause SFX
            {
                audioSource.UnPause();
                audioPaused = false;
            }
        }

        previousPosition = transform.position;
        previousRotation = transform.rotation;

        HandleGravity();
        HandleAnimation();
        HandleRotation();

        //Movement
        if(IsJumping)
            controller.Move((new Vector3(0,velocity.y,0))*Time.deltaTime);
        else if(canMove)
            controller.Move(velocity * Time.deltaTime);
    }

    /*
     *  Actions
     */
    //Actions: Jump
    public void Jump()
    {
        //The Player can jump only if grounded and not already in the process of jumping (determined by jumpPressed)
        if (isGrounded && !jumpPressed)
        {
            jumpPressed = true;
            jumpEnded = false;
            canTransform = false;
            //If the Player is jumping, they can rotate but cannot change position
            canMove = false;
        }
    }

    public bool IsJumping 
    { 
        get 
        {
            return jumpPressed;
        } 
    }
    
    //Actions: Go Invisible
    public bool CanGoInvisible { get { return canGoInvisible; } set { canGoInvisible = value; } }

    public void GoInvisible(float duration)
    {
        InvisibilityCoroutineDispose();
        invisiblityCoroutine = StartCoroutine(InvisibilityCoroutine(duration));
    }

    private void GetMeshColor()
    {
        if (meshColors.Count == 0)
        {
            MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer mesh in meshes)
            {
                if (mesh.gameObject.layer == EngineConf.Layer.PLAYER && mesh.CompareTag(EngineConf.Tag.PLAYER_BODY_PART))
                {
                    meshColors.Add(
                        mesh.material,
                        Tuple.Create(
                            mesh.material.GetRenderingMode(),
                            mesh.material.color.a,
                            mesh.material.GetMetallicValue(),
                            mesh.material.GetSmoothnessValue()
                        )
                    );
                }
            }
        }
    }

    private void AddMesh(Material material)
    {
        if(!meshColors.ContainsKey(material))
            meshColors.Add(material, Tuple.Create(
                material.GetRenderingMode(),
                material.color.a,
                material.GetMetallicValue(),
                material.GetSmoothnessValue()
            ));
    }

    private void RemoveMesh(Material material)
    {
        if (meshColors.ContainsKey(material))
            meshColors.Remove(material);
    }

    private void RenderMesh(bool visible)
    {
        if(meshColors.Count == 0)
            GetMeshColor();

        if(meshColors.Count > 0)
        {
            foreach (KeyValuePair<Material, Tuple<MyExtension.BlendMode,float,float,float>> kvp in meshColors)
            {
                //Set the original rendering mode of the material or switch to Transparent mode (if invisible)
                if (visible)
                {
                    if(kvp.Key.GetRenderingMode() != kvp.Value.Item1)
                        kvp.Key.SetRenderingMode(kvp.Value.Item1);
                }
                else
                {
                    if (kvp.Key.GetRenderingMode() != MyExtension.BlendMode.Transparent)
                        kvp.Key.SetRenderingMode(MyExtension.BlendMode.Transparent);
                }

                Color color = kvp.Key.color;
                color.a = visible? kvp.Value.Item2 : 0.5f; //Restore or Set the alpha value!
                kvp.Key.color = color;

                //Restore or Set the metallic value
                kvp.Key.SetMetallicValue(visible? kvp.Value.Item3 : 0f);
                //Restore or Set the smoothness value
                kvp.Key.SetMetallicValue(visible? kvp.Value.Item4 : 0f);
            }
        }
    }



    /*
    //Actions: Dash 
    public void Dash(float distance, float dashTime)
    {
        NOT IMPLEMENTED
    }
    */
    //Actions: Use Equip

    /*
     * Unequip: true  => Unequip the default equipment or the currently equipped item relative to the part.
     * Unequip: false => Equip the default equipment relative to the part.
     */
    private bool HandlePart(Player.PlayerPart part, bool unequip, out GameObject parent )
    {
        parent = null;
        switch (part)
        {
            case PlayerPart.MOUTH:
                pMouth.SetActive(!unequip);
                parent = pBody;
                break;

            case PlayerPart.BACK:
                pBack.SetActive(!unequip);
                parent = pBody;
                break;

            default:
                parent = null;
                return false; 
        }

        //Unequip other equipment of the same type as the part
        if (unequip)
        {
            foreach (KeyValuePair<int, Equip> kvp in equipments)
            {
                Equip.AttachInfo attach = kvp.Value.GetAttachInfo(playerType).Value;
                if (attach.Part == part)
                {
                    if (kvp.Value.gameObject.activeSelf)
                    {
                        kvp.Value.gameObject.SetActive(false);
                        PlayerManager.Instance.UnquippedItem(kvp.Key);
                    }
                }
            }
        }

        return true;
    }

    public bool WearEquip(int key, Equip equipPrefab)
    {
        GameObject parent = null;
        Equip.AttachInfo? info = null;

        if (equipments.ContainsKey(key)) //Already loaded into the dictionary
        {
            if (equipments[key].gameObject.activeSelf) //Already equipped
                return true;
            else //Not equipped
            {
                info = equipments[key].GetAttachInfo(playerType);

                //If other equipment of the same type as the part is equipped, unequip it
                if (HandlePart(info.Value.Part, true, out parent))
                    equipments[key].gameObject.SetActive(true);
                else
                    return false;

                return true;
            }
        }
        else //Not loaded into the dictionary
        {
            if ((info = equipPrefab.GetAttachInfo(playerType)) != null)
            {
                //If other equipment of the same type or part is equipped, unequip it
                if (HandlePart(info.Value.Part, true, out parent))
                {
                    // Instantiate the GameObject representing the equipment and attach it to the body
                    Equip equip = Instantiate<Equip>(equipPrefab, parent.transform);
                    equip.gameObject.transform.localPosition = info.Value.Position;
                    equip.gameObject.SetActive(true);
                    equipments.Add(key, equip);
                    AddEquipMeshes(key);   //Register its mesh
                    RenderMesh(isVisible); //Set the appropriate rendering style (visible or invisible)

                    return true;
                }
            }
        }

        return false;
    }

    public bool UseEquip(int key)
    {
        if (IsEquipped(key))
        {
            switch(equipments[key].GetUseType())
            {
                case Item.UseType.DASH:
                    equipments[key].Use();
                break;

                case Item.UseType.SHOOT_SR_LASER: //Single shoot with recoil
                    if (shootingStarted) //Already shooting
                        return false;
                    
                    shootingSRLaser = true;
                    //Cannot move or rotate
                    canMove = false;
                    canRotate = false;
                    canTransform = false;
                    InitShootingAnimation();
                    StartCoroutine(WaitUntilShootingAnimIsReady(
                        () => shootingReady, 
                        () => equipments[key].Use()
                    ));
                break;
            }
            return true;
        }

        return false;
    }

    public void RemoveEquip(int key)
    {        
        if (IsEquipped(key)) //If equipped, wait for its usage to finish (if in use) before removing it!
        {
            switch (equipments[key].GetUseType())
            {
                case Item.UseType.DASH:
                    
                    break;

                case Item.UseType.SHOOT_SR_LASER: //Single shoot with recoil
                    StartCoroutine(WaitUntilUnequipConditionIsMet(() => !shootingSRLaser, key));
                    break;
            }
            return;
        }

        RemoveEquipMeshes(key);
        equipments.Remove(key);
    }

    public bool IsEquipped(int key)
    {
        if(equipments.ContainsKey(key))
        {
            return equipments[key].gameObject.activeSelf; //Equipped only if activated
        }

        return false;
    }

    private void AddEquipMeshes(int key)
    {
        if(!equipments.ContainsKey(key))
            return;

        MeshRenderer[] meshes = equipments[key].transform.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer mesh in meshes)
        {
            if (mesh.gameObject.layer == EngineConf.Layer.PLAYER && mesh.CompareTag(EngineConf.Tag.PLAYER))
            {
                AddMesh(mesh.material);
            }
        }
    }

    private void RemoveEquipMeshes(int key)
    {
        if (!equipments.ContainsKey(key))
            return;

        MeshRenderer[] meshes = equipments[key].transform.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer mesh in meshes)
        {
            if (mesh.gameObject.layer == EngineConf.Layer.PLAYER && mesh.CompareTag(EngineConf.Tag.PLAYER))
            {
                RemoveMesh(mesh.material);
            }
        }
    }

    /*
     *  Animations
     */
    private void InitializeAnimator()
    {
        /*
         * If the GameObject is disabled while an animation is playing, the default transform settings of the 
         * GameObject are overwritten by the transform settings from the current animation frame.
         * These default values are not restored when the GameObject is re-enabled.
         * To avoid this issue, use animator.WriteDefaultValues();
         */

        //animator.writeDefaultValuesOnDisable = true;
        animator.WriteDefaultValues();
        //Set the default state to Idle
        animator.SetBool(isWalkingHash, false);
        animator.SetBool(isJumpingHash, false);
        animator.SetBool(isJumpingHash, false);
        //Reset the animation variable
        jumpPressed = false;
        jumpEnded = false;
        shootingStarted = false;
        shootingReady = false;
        shootingEnded = true;
        shootingSRLaser = false;
    }

    private void HandleAnimation()
    {
        //Animation state
        isWalkingAnim = animator.GetBool(isWalkingHash);
        isJumpingAnim = animator.GetBool(isJumpingHash);
        isSRLaserAnim = animator.GetBool(isSRLaserHash);

        //Walking
        if (!isWalkingAnim && movePressed) //If not walking and movement is pressed
        {
            animator.SetBool(isWalkingHash, true);
            Utilities.HandleSound(audioSource, currentAudioMap.walk, Utilities.HandleSoundState.PLAY, 1); //Loop = true
        }
        else if (isWalkingAnim && !movePressed) //If walking and no movement input is detected
        {
            animator.SetBool(isWalkingHash, false);
            //Allow the sound to stop naturally by disabling the loop
            Utilities.HandleSound(audioSource, currentAudioMap.walk, Utilities.HandleSoundState.PLAY, 2); //Loop = false
        }

        //Jumping
        if (!isJumpingAnim && jumpPressed) //If not currently jumping and jump is pressed
        {
            animator.SetBool(isJumpingHash, true);
            //Note: The animation triggers the functions JumpStarted and JumpEnded to update the state
        }
        else if (isJumpingAnim && isGrounded && jumpPressed && jumpEnded)
        {   //If the animation has ended and the player has reached the ground, set the animation state to false
            animator.SetBool(isJumpingHash, false);
            jumpPressed = false;    //enable the jump action!
            canMove = true;         //enable movement
            canTransform = true;
            //Generate noise
            NoiseCoroutineDispose();
            soundNoise.radius = 10f;
            noiseCoroutine = StartCoroutine(NoiseCoroutine());
        }

        //Shooting Laser (Single shoot - Recoil)
        if (!isSRLaserAnim && shootingSRLaser && shootingStarted)
        {
            animator.SetBool(isSRLaserHash, true);
            //Note: The animation triggers the functions ShootingReady and ShootingEnded to update the state
        }
        else if (isSRLaserAnim && shootingSRLaser && shootingStarted && shootingEnded)
        {
            animator.SetBool(isSRLaserHash, false);
            shootingSRLaser = false;
            shootingStarted = false;
            shootingReady = false;
            canMove = true;
            canRotate = true;
            canTransform = true;
        }
    }

    //Animation Events: Jump
    public void JumpStarted() //Called by the animation
    {
        if(jumpPressed)
        {   
            velocity.y += jumpVelocity;
            gravityMultiplier = jumpMultiplier;
        }
    }

    public void JumpEnded() //Called by the animation
    {   
        if(jumpPressed)
        {
            jumpEnded = true;
        }
    }

    //Animation Events: Shooting
    private void InitShootingAnimation() //Called by the item/equipment that performs the shooting
    {
        shootingStarted = true;
        shootingReady = false;
        shootingEnded = false;
    }

    public void ShootingReady()
    {
        if (shootingStarted)
            shootingReady = true;
    }

    public void ShootingEnded()
    {
        if(shootingStarted)
            shootingEnded = true;
    }

    /*
     *  Coroutines
     */
    //Coroutine: Noise generator
    private IEnumerator NoiseCoroutine()
    {
        soundNoise.enabled = true;
        yield return new WaitForSeconds(0.1f);
        soundNoise.enabled = false;
        noiseCoroutine = null;
    }

    private void NoiseCoroutineDispose()
    {
        if(noiseCoroutine != null)
        {
            StopCoroutine(noiseCoroutine);
            noiseCoroutine = null;
            soundNoise.enabled = false;
        }
    }

    //Coroutine: Invisibility
    private IEnumerator InvisibilityCoroutine(float duration)
    {
        isVisible = false;
        RenderMesh(false);
        invisibilityTime = Time.time + duration;
        /*
         *  Note: 
         *  Time.time + duration is used to resume the invisibility coroutine with only the remaining 
         *  time from the checkpoint.
         *  To save the remaining time as the new duration, use: invisibilityTime - Time.time.
         *  If the result is <= 0, the coroutine has ended!
         */
        yield return new WaitForSeconds(duration);
        isVisible = true;
        RenderMesh(true);
        invisiblityCoroutine = null;
        invisibilityTime = -1f;
    }

    private void InvisibilityCoroutineDispose()
    {
        if(invisiblityCoroutine != null)
        {
            StopCoroutine(invisiblityCoroutine);    
            invisiblityCoroutine = null;
            invisibilityTime = -1f;
            isVisible = true;
            RenderMesh(true);
        }
    }

    //Coroutine: Equip With Animation/Timer/Conditions
    //Unequip an item currently in use only after its usage has ended!
    private IEnumerator WaitUntilUnequipConditionIsMet(System.Func<bool> condition, int key)
    {
        yield return new WaitUntil(condition);

        GameObject parent = null;
        //Equip the default body part
        HandlePart(equipments[key].GetAttachInfo(playerType).Value.Part, false, out parent);
        RemoveEquipMeshes(key);
        Destroy(equipments[key].gameObject);

        equipments.Remove(key);
    }

    //Coroutine: Shooting With Animation
    private IEnumerator WaitUntilShootingAnimIsReady(System.Func<bool> condition, System.Action action)
    {
        yield return new WaitUntil(condition);
        action();
    }

    /*
     *  Entity State
     */
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

        //Jump && Gravity
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

        //Visibility handler
        state.canGoInvisible = canGoInvisible;

        if (invisibilityTime > 0)
            state.invisibilityTime = invisibilityTime - Time.time;
        else
            state.invisibilityTime = -1f;

        /*
         * Note:
         * invisibilityTime is set as Time.time + duration!
         * More information can be found in the InvisibilityCoroutine.
         */

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

        //Jump && Gravity
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
        currentAudioMap = entityState.currentAudioMap == null ? defaultAudioMap : entityState.currentAudioMap;

        //Visibility handler
        canGoInvisible = entityState.canGoInvisible;
        if (canGoInvisible)
            RenderMesh(isVisible);

        if(entityState.invisibilityTime > 0f)
        {
            invisiblityCoroutine = StartCoroutine(InvisibilityCoroutine(entityState.invisibilityTime));
        }

        //The loading of equipment is handled by the PlayerManager, which will request to add or wear
        //equipment if necessary.

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

        //Jump && Gravity
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

        /*
         * Note:
         * The equip state is managed by the PlayerManager, as the manager maintains a list of collected items 
         * along with their state (numUse, equipped, etc.).
         */

        //Visibility handler
        [SerializeField] public bool canGoInvisible;
        [SerializeField] public float invisibilityTime;
    }
}

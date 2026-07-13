/*
 * Manages the player's state and interactions (input) during gameplay mode.
 * This manager is available only in scenes of type Gameplay 
 * (refer to LevelSO and LevelListSO for more information).
 */
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour, IStateSaveable, IDamageable
{
    private static PlayerManager _instance;

    //Player Prefabs
    [SerializeField] private PlayerBot playerBot = null;
    [SerializeField] private PlayerBox playerBox = null;

    //Player Inputs (actions defined in the InputActionAsset from the InputSystem package)
    InputAction moveAction;
    InputAction jumpAction;
    InputAction use1Action;
    InputAction use2Action;
    InputAction use3Action;
    InputAction pauseAction;

    //Player Settings
    [SerializeField] private Player player = null; 
    [SerializeField] private Player oldPlayer = null;
    //player: Represents the current active Player instance in use.
    //oldPlayer: Refers to the previous Player instance used before transitioning to the current "player".
    [SerializeField] private Player.PlayerType playerType = Player.PlayerType.BOT;
    //playerType: Indicates the current form or transformation of the player

    [SerializeField] private int health = 1;
    [SerializeField] private Item[] items = new Item[3];
    //items: Represent the items, equipment, or skills that the player has collected 
    //       and can use via the item/skill bar.
    //       These are copies obtained from the defined ScriptableObject (SO) [see LootableItem for more details].

    [SerializeField] private Keyboard currentKeyboard = null;
    [SerializeField] private Gamepad currentGamepad = null;
    //Reference to the current device in use that is supported by the game.
    //Currently, only the Keyboard and Gamepad are supported (recognized by the InputSystem package).
    //The game allows simultaneous use of both the Keyboard and Gamepad.

    public static PlayerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PlayerManager>(true);
                if (_instance == null)
                {
                    Debug.LogWarning("No PlayerManager available in the scene!");
                    return null;
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        if(_instance == this)
            _instance = null;
    }

    private void OnEnable()
    {
        /*
         * Retrieves a reference to the current devices in use.
         * Note: For each type, multiple devices can be connected, but the system will only consider 
         * the one actively in use (generally, the most recently used device).
         */
        currentKeyboard = Keyboard.current;
        currentGamepad = Gamepad.current;

        //Registers for the event that listens for changes in the state of any device
        InputSystem.onDeviceChange += OnDeviceChange;

        /*
         * Actions:
         * Retrieves a reference to the action and associates the corresponding delegate 
         * with the event triggered when the action starts.
         * Action started: Indicates that the user has begun pressing the button associated with the action.
         */
        moveAction = GameData.GameInput.GetMoveAction();
        //The state of this action (input value) is evaluated each frame by the Update method

        jumpAction = GameData.GameInput.GetJumpAction();
        jumpAction.started += JumpAction;

        use1Action = GameData.GameInput.GetUse1Action();
        use1Action.started += Use1Action;

        use2Action = GameData.GameInput.GetUse2Action();
        use2Action.started += Use2Action;

        use3Action = GameData.GameInput.GetUse3Action();
        use3Action.started += Use3Action;

        pauseAction = GameData.GameInput.GetPauseAction();
        pauseAction.started += PauseAction;
    }

    private void OnDisable()
    {
        currentKeyboard = null;
        currentGamepad = null;
        InputSystem.onDeviceChange -= OnDeviceChange;

        HandleActions(false); 
        jumpAction.started -= JumpAction;
        use1Action.started -=Use1Action;
        use2Action.started -= Use2Action;
        use3Action.started -= Use3Action;
        pauseAction.started -= PauseAction;
    }

    /*
     * Pauses the game if the currently used device is disconnected.
     * Note: It is assumed that if a Gamepad is connected, the game is prioritizing the Gamepad over the 
     * keyboard, even if the Gamepad is connected but the player is only using the keyboard.
     */
    private void OnDeviceChange(InputDevice device, InputDeviceChange change) //
    {
        if(change == InputDeviceChange.Removed)
        {
            //Prioritizes the Gamepad
            if (currentGamepad != null)
            {
                if (currentGamepad.deviceId == device.deviceId)
                {
                    GameManager.Instance.SetPause(true);
                    currentGamepad = Gamepad.current;
                }
            }
            else if (currentKeyboard != null && currentKeyboard.deviceId == device.deviceId)
            {
                GameManager.Instance.SetPause(true);
                currentKeyboard = Keyboard.current;
            }
        }
        else if (change == InputDeviceChange.Added)
        {
            //Updates the state of the connected device
            currentKeyboard = Keyboard.current;
            currentGamepad = Gamepad.current;
        }
    }

    private void Update()
    {
        if (Time.deltaTime == 0f) //Game paused
            return;

        MoveAction(moveAction.ReadValue<Vector2>());
    }
    
    public Player Player { get { return player; } }

    public void Spawn(Transform spawn)
    {
        if(player == null)
        {
            player = GameObject.FindObjectOfType<Player>(true);
        }

        if(player == null)
        {
            switch(playerType)
            {
                case Player.PlayerType.BOT:
                    player = Instantiate(playerBot, spawn.position, spawn.rotation);
                break;

                case Player.PlayerType.BOX:
                    player = Instantiate(playerBox, spawn.position, spawn.rotation);
                break;
            }
        }
        else
        {
            //Updates the transform without causing conflicts with GameCharacter
            player.gameObject.SetActive(false); 
            player.transform.position = spawn.position;
            player.transform.rotation = spawn.rotation;
        }

        player.gameObject.SetActive(true); 
    }

    private void MoveAction(Vector2 move)
    {
        player.Move(move);
    }

    private void JumpAction(InputAction.CallbackContext context)
    {
        (player as IJumpable)?.Jump();
    }

    private void Use1Action(InputAction.CallbackContext context) 
    {
        UseItem(0);
    }

    private void Use2Action(InputAction.CallbackContext context)
    {
        UseItem(1);
    }

    private void Use3Action(InputAction.CallbackContext context)
    {
        UseItem(2);
    }

    private void PauseAction(InputAction.CallbackContext context)
    {
        GameManager.Instance.SetPause(true);
    }

    //Method that allows other Managers or Scripts to request enabling or disabling input interactions
    public void RequestInteraction(bool interact) 
    {
        HandleActions(interact);
    }

    //Enables or disables input interactions (enabled only for available actions)
    private void HandleActions(bool enable)
    {
        if (enable)
        {
            moveAction.Enable();

            if (player is IJumpable)
                jumpAction.Enable();
            else
                jumpAction.Disable();

            pauseAction.Enable();

            if (items[0] == null)
                use1Action.Disable();
            else
                use1Action.Enable();

            if (items[1] == null)
                use2Action.Disable();
            else
                use2Action.Enable();

            if (items[2] == null)
                use3Action.Disable();
            else
                use3Action.Enable();
        }
        else
        {
            GameData.GameInput.DisableGameplayActionMap();
        }
    }
    
    public bool AddItem(Item item) //true: added | false: not added
    {
        //Checks whether the item has already been acquired
        for (int i=0; i<items.Length; i++)
        {
            if(items[i] != null && items[i].id == item.id) //If already acquired, increment the numUse value
            {
                if (items[i].numUse < 0) //The item has unlimited uses
                    return true;

                if (item.numUse >= 0)
                    items[i].numUse += item.numUse;
                else
                    items[i].numUse = -1; //If the item has infinite uses, set it to infinite

                //Updates the UI using UseItem and provides the updated numUse value
                UIManager.Instance.GetUIGameplay().UseItem(i, items[i].numUse);

                return true;
            }
        }

        //If not already acquired, place the item in the first available slot (if one exists)

        if (items[0] == null) 
        {
            items[0] = item;
            use1Action.Enable();
            UIManager.Instance.GetUIGameplay().GotItem(0, item.sprite, item.numUse, item.isEquipable);
            return true;
        }

        if (items[1] == null)
        {
            items[1] = item;
            use2Action.Enable();
            UIManager.Instance.GetUIGameplay().GotItem(1, item.sprite, item.numUse, item.isEquipable);
            return true;
        }

        if (items[2] == null)
        {
            items[2] = item;
            use3Action.Enable();
            UIManager.Instance.GetUIGameplay().GotItem(2, item.sprite, item.numUse, item.isEquipable);
            return true;
        }

        return false; //Not added
    }

    public void UnquippedItem(int key) //key represents the item's ID
    {
        for (int i=0; i<items.Length; i++)
        {
            if (items[i].id == key)
            {
                UIManager.Instance.GetUIGameplay().EquippedItem(i, false); //Update UI
                return;
            }
        }
    }

    //Use items:
    private void UseItem(int index)
    {
        if(items[index].isEquipable) //An equipable item must be equipped before it can be used
        {
            if (player is IEquipable eqPlayer) //See IEquipable interface
            {   
                if (!eqPlayer.IsEquipped(items[index].id)) //The item ID is used as the equip key for the player
                {
                    if (eqPlayer.WearEquip(items[index].id, items[index].prefabEquip))
                    {
                        UIManager.Instance.GetUIGameplay().EquippedItem(index, true);
                    }
                    return;
                }
            }
            else
                return; //The player cannot equip this item; therefore, it cannot be used!
        }

        //Uses the item (if it is equipable, it is already equipped)
        switch (items[index].useType)
        {
            case Item.UseType.BOX:
                UseBox();
            break;

            case Item.UseType.INVISIBLE:
                if (player is IInvisibility invPlayer)
                {
                    if(invPlayer.CanGoInvisible)
                        invPlayer.GoInvisible((items[index] as InvisibilityItem).duration);
                    else
                        return;  //Not used
                }
                else { return; } //Not used
            break;

            case Item.UseType.SHOOT:
            case Item.UseType.SHOOT_SR_LASER:
            case Item.UseType.DASH:
                if(!(player as IEquipable).UseEquip(items[index].id))
                    return; // If UseEquip returns false, the equipment is not used
            break;

            default:
                return;
        }

        if (items[index].numUse > 0) //-1 = infinite uses
        {
            items[index].numUse--;

            if (items[index].numUse == 0)
            {
                if (items[index].isEquipable)
                    (player as IEquipable).RemoveEquip(items[index].id);

                UIManager.Instance.GetUIGameplay().RemoveItem(index);

                items[index] = null;
                HandleActions(true);
            }
            else
                UIManager.Instance.GetUIGameplay().UseItem(index, items[index].numUse);
        }
    }

    public void UseBox()
    {
        if (!player.CanTransform)
            return;

        if (player is PlayerBox) //Is it currently a box?
        {
            playerType = Player.PlayerType.BOT; //Returns to bot
        }
        else
        {
            playerType = Player.PlayerType.BOX;
        }

        if (oldPlayer == null)
        {
            oldPlayer = player;
            oldPlayer.gameObject.SetActive(false);
            if (playerType == Player.PlayerType.BOX)
            {
                player = Instantiate(playerBox, player.transform.position, player.transform.rotation);
            }
            else
            {
                player = Instantiate(playerBot, player.transform.position, player.transform.rotation);
            }
        }
        else
        {
            Player tmp = player;
            player = oldPlayer;
            oldPlayer = tmp;

            player.transform.position = oldPlayer.transform.position;
            player.transform.rotation = oldPlayer.transform.rotation;
            oldPlayer.gameObject.SetActive(false);
            player.gameObject.SetActive(true);
        }

        HandleActions(true);
    }

    //IDamageable
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
            Kill();
    }

    public void Kill()
    {
        player.Kill();
        LevelManager.Instance.GameOver(true, 1f);
    }

    //PlayerState
    public IState SaveState()
    {
        EntityState state = new EntityState();

        state.playerType = playerType;
        state.player = player.SaveState();
        state.health = health;

        state.items = new Item.SerializableItem[items.Length];

        if (player is IEquipable equiPlayer)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null || items[i].id == 0) //ID == 0 => Item is not valid!
                    state.items[i] = null; 
                else
                    state.items[i] = items[i].Serialize(equiPlayer.IsEquipped(items[i].id));
            }
        }
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                state.items[i] = (items[i] == null || items[i].id == 0)? null : items[i].Serialize(false);
            }
        }

        return state;
    }

    public void LoadState(IState state)
    {
        EntityState entityState = state as EntityState;

        health = entityState.health;

        if (health <= 0) //Safety check (1)... Could this happen?
        {
            LevelManager.Instance.GameOver(false, 0f);
            return;
        }

        playerType = entityState.playerType;

        switch (playerType)
        {
            case Player.PlayerType.BOT:
                player = Instantiate(playerBot);
                break;

            case Player.PlayerType.BOX:
                player = Instantiate(playerBox);
                break;
        }

        player.LoadState(entityState.player);

        IEquipable equipPlayer = player as IEquipable;
        //Loads the looted item
        for(int i=0; i<items.Length; i++)
        {
            if (entityState.items[i] == null || entityState.items[i].id == 0) //The item slot is empty!
                items[i] = null;
            else
            {   
                //Loads the item into the slot
                items[i] = entityState.items[i].Deserialize();

                //Update UI
                UIManager.Instance.GetUIGameplay().GotItem(i, items[i].sprite, items[i].numUse, items[i].isEquipable);
                
                if (entityState.items[i].equipped) //If true, the item is equippable
                {
                    UIManager.Instance.GetUIGameplay().EquippedItem(i, true);
                    //Notifies the player to equip the item
                    equipPlayer?.WearEquip(items[i].id, items[i].prefabEquip);
                }
            }
        }
    }

    [System.Serializable]
    private class EntityState : IState
    {
        [SerializeField] public Player.PlayerType playerType;
        [SerializeReference] public IState player;
        [SerializeField] public int health;
        [SerializeField] public Item.SerializableItem[] items;
    }
}

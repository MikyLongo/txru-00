/*
 * Script that manages the UI for submenus dedicated to rebinding supported devices.
 * Currently, there is one submenu for keyboard rebinding and another for gamepad rebinding. 
 * Both use the same script by setting a different value for the "deviceType" field.
 * 
 * Features:
 * - Provides a button to reset to default settings.
 * - If the user makes changes and attempts to exit the menu without saving, prompts the user to confirm 
 *   whether to save the changes before closing the menu.
 *
 * - If the user is rebinding the gamepad and it gets disconnected, a message box with a timer will appear
 *   to notify them of the issue. If the user fails to reconnect a gamepad within the time limit, all changes
 *   will be discarded, and the game will return to the settings home.
 *
 * Note: Refer to the GameInput class for detailed information on how the rebinding system works.
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GameData;
using System.Collections.Generic;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.InputSystem;

public class UISettingsBindings : MonoBehaviour, IPageUI
{
    [SerializeField] private UISettings parent;
    [SerializeField] private Button bForward;
    [SerializeField] private Button bBackward;
    [SerializeField] private Button bLeft;
    [SerializeField] private Button bRight;
    [SerializeField] private Button bJump;
    [SerializeField] private Button bUse1;
    [SerializeField] private Button bUse2;
    [SerializeField] private Button bUse3;
    [SerializeField] private Button bPause;
    [SerializeField] private Button bSubmitUI;
    [SerializeField] private Button bSave;
    [SerializeField] private Button bReset;
    [SerializeField] private Button bExit;
    [SerializeField] private UICustomScrollView scrollView;

    //Essential for rebinding
    [SerializeField] private GameInput.DeviceType deviceType;
    private Button bClicked;
    private UIMessageBox mBox = null;

    private bool initialized = false;
    private bool modified = false;
    private bool isRebinding = false;
    private bool waitReconnect = false;

    public static readonly string LK_GAMEPAD_DISCONNECTED = "RB_GAMEPAD_DISCONNECTED";
    public static readonly string LK_GAMEPAD_ASK_RECONNECT = "RB_GAMEPAD_ASK_RECONNECT";

    private void OnEnable()
    {
        UIManager.Instance.OpenedPage = this;
        //Avoid redundant Init() calls if the UI has already been initialized and remains unmodified
        if (!initialized || (initialized && modified))
            Init();

        isRebinding = false;
        waitReconnect = false;

        bForward.onClick.AddListener(OnForwardClick);
        bForward.GetComponent<UICustomElement>().onMove.AddListener(Move);

        bBackward.onClick.AddListener(OnBackwardClick);
        bBackward.GetComponent<UICustomElement>().onMove.AddListener(Move);

        bLeft.onClick.AddListener(OnLeftClick);
        bLeft.GetComponent<UICustomElement>().onMove.AddListener(Move);

        bRight.onClick.AddListener(OnRightClick);
        bRight.GetComponent<UICustomElement>().onMove.AddListener(Move);

        bJump.onClick.AddListener(OnJumpClick);
        bJump.GetComponent<UICustomElement>().onMove.AddListener(Move);

        bUse1.onClick.AddListener(OnUse1Click);
        bUse1.GetComponent<UICustomElement>().onMove.AddListener(Move);

        bUse2.onClick.AddListener(OnUse2Click);
        bUse2.GetComponent<UICustomElement>().onMove.AddListener(Move);

        bUse3.onClick.AddListener(OnUse3Click);
        bUse3.GetComponent<UICustomElement>().onMove.AddListener(Move);

        bPause.onClick.AddListener(OnPauseClick);
        bPause.GetComponent<UICustomElement>().onMove.AddListener(Move);

        bSubmitUI.onClick.AddListener(OnSubmitUIClick);
        bSubmitUI.GetComponent<UICustomElement>().onMove.AddListener(Move);

        bSave.onClick.AddListener(OnSaveClick);
        bSave.GetComponent<UICustomElement>().onMove.AddListener(Move);

        bReset.onClick.AddListener(OnResetClick);
        bReset.GetComponent<UICustomElement>().onMove.AddListener(Move);

        bExit.onClick.AddListener(OnExitClick);
        bExit.GetComponent<UICustomElement>().onMove.AddListener(Move);

        if(deviceType == GameInput.DeviceType.Gamepad)
            InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        bForward.onClick.RemoveAllListeners();
        bForward.GetComponent<UICustomElement>().onMove.RemoveAllListeners();

        bBackward.onClick.RemoveAllListeners();
        bBackward.GetComponent<UICustomElement>().onMove.RemoveAllListeners();

        bLeft.onClick.RemoveAllListeners();
        bLeft.GetComponent<UICustomElement>().onMove.RemoveAllListeners();

        bRight.onClick.RemoveAllListeners();
        bRight.GetComponent<UICustomElement>().onMove.RemoveAllListeners();

        bJump.onClick.RemoveAllListeners();
        bJump.GetComponent<UICustomElement>().onMove.RemoveAllListeners();

        bUse1.onClick.RemoveAllListeners();
        bUse1.GetComponent<UICustomElement>().onMove.RemoveAllListeners();

        bUse2.onClick.RemoveAllListeners();
        bUse2.GetComponent<UICustomElement>().onMove.RemoveAllListeners();

        bUse3.onClick.RemoveAllListeners();
        bUse3.GetComponent<UICustomElement>().onMove.RemoveAllListeners();

        bPause.onClick.RemoveAllListeners();
        bPause.GetComponent<UICustomElement>().onMove.RemoveAllListeners();

        bSubmitUI.onClick.RemoveAllListeners();
        bSubmitUI.GetComponent<UICustomElement>().onMove.RemoveAllListeners();

        bSave.onClick.RemoveAllListeners();
        bSave.GetComponent<UICustomElement>().onMove.RemoveAllListeners();

        bReset.onClick.RemoveAllListeners();
        bReset.GetComponent<UICustomElement>().onMove.RemoveAllListeners();

        bExit.onClick.RemoveAllListeners();
        bExit.GetComponent<UICustomElement>().onMove.RemoveAllListeners();

        if(deviceType == GameInput.DeviceType.Gamepad)
            InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void Init()
    {
        UpdateUI();

        initialized = true;
        modified = false;
        SetSaveInteractable(false);
    }

    private void UpdateUI()
    {
        //Move
        bForward.SetText(GameInput.GetHumanReadableString(GameInput.GetForwardBinding(deviceType)));
        bBackward.SetText(GameInput.GetHumanReadableString(GameInput.GetBackwardBinding(deviceType)));
        bLeft.SetText(GameInput.GetHumanReadableString(GameInput.GetLeftBinding(deviceType)));
        bRight.SetText(GameInput.GetHumanReadableString(GameInput.GetRightBinding(deviceType)));

        //Jump
        bJump.SetText(GameInput.GetHumanReadableString(GameInput.GetJumpBinding(deviceType)));

        //Use1
        bUse1.SetText(GameInput.GetHumanReadableString(GameInput.GetUse1Binding(deviceType)));

        //Use2
        bUse2.SetText(GameInput.GetHumanReadableString(GameInput.GetUse2Binding(deviceType)));

        //Use3
        bUse3.SetText(GameInput.GetHumanReadableString(GameInput.GetUse3Binding(deviceType)));

        //Pause
        bPause.SetText(GameInput.GetHumanReadableString(GameInput.GetPauseBinding(deviceType)));

        //SubmitUI
        bSubmitUI.SetText(GameInput.GetHumanReadableString(GameInput.GetSubmitUIBinding(deviceType)));
    }

    private void SetModified()
    {
        if(initialized)
        {
            modified = true;
            if (!bSave.interactable)
            {
                SetSaveInteractable(true);
            }
        }
    }

    //The save button should be interactable only when changes have been made
    private void SetSaveInteractable(bool interactable)
    {
        bSave.interactable = interactable;
        Navigation navigation = bReset.navigation;
        navigation.selectOnLeft = interactable ? bSave : null;
        bReset.navigation = navigation;
    }

    private void ShowBindingMessage()
    {
        isRebinding = true;
        if(mBox != null)
        {
            mBox.Close();
        }
        else
            mBox = UIManager.Instance.GetUIMessageBox();

        mBox.Open(
            "",
            LocalizationHelper.GetSmartString(LocalizationHelper.UI_SMART, UIMessageBox.LK_ASK_TO_PRESS_KEY,
                new Dictionary<string, IVariable>()
                {
                    { 
                        "keyString", 
                        new StringVariable() 
                        { Value = GameInput.GetHumanReadableString(GameInput.GetCancelRebindKey(deviceType)) } 
                    } 
                }
            ),
            null,
            null,
            false,
            null, 
            0,
            UIMessageBox.MsgType.Generic,
            true,
            0
        );
    }
    
    private void OnComplete(bool foundDuplicate, string duplicateBinding,string effectiveBinding)
    {
        isRebinding = false;
        if(foundDuplicate) 
        {
            if(mBox != null)
            {
                mBox.Close();
            }
            else
                mBox = UIManager.Instance.GetUIMessageBox();

            mBox.Open(
                LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, UIMessageBox.LK_KEYBIND_ALREADY_USED_T),
                LocalizationHelper.GetSmartString(LocalizationHelper.UI_SMART, UIMessageBox.LK_KEYBIND_ALREADY_USED_TEXT,
                    new Dictionary<string, IVariable>()
                    {
                        {
                            "keyString",
                            new StringVariable() { Value = duplicateBinding }
                        }
                    }
                ),
                null,
                null,
                true,
                null,
                3f,
                UIMessageBox.MsgType.Warning,
                false
            );
            bClicked.SetText(effectiveBinding);
        }
        else
        {
            if(mBox != null)
                mBox.Close();

            SetModified();
            bClicked.SetText(effectiveBinding);
        }
    }

    private void OnCancel()
    {
        isRebinding = false;
        if (mBox != null)
            mBox.Close();
    }

    private void OnForwardClick()   
    {
        bClicked = bForward;          
        GameInput.RebindMoveForward(deviceType, OnComplete, OnCancel);
        ShowBindingMessage();
    }

    private void OnBackwardClick()   
    {
        bClicked = bBackward;          
        GameInput.RebindMoveBackward(deviceType, OnComplete, OnCancel);
        ShowBindingMessage();
    }

    private void OnLeftClick()       
    {
        bClicked = bLeft;
        GameInput.RebindMoveLeft(deviceType, OnComplete, OnCancel);
        ShowBindingMessage();
    }

    private void OnRightClick()     
    {
        bClicked = bRight;
        GameInput.RebindMoveRight(deviceType, OnComplete, OnCancel);
        ShowBindingMessage();
    }

    private void OnJumpClick()
    {
        bClicked = bJump;
        GameInput.RebindJump(deviceType, OnComplete, OnCancel);
        ShowBindingMessage();
    }

    private void OnPauseClick()
    {
        bClicked = bPause;
        GameInput.RebindPause(deviceType, OnComplete, OnCancel);
        ShowBindingMessage();
    }

    private void OnUse1Click()
    {
        bClicked = bUse1;
        GameInput.RebindUse1(deviceType, OnComplete, OnCancel);
        ShowBindingMessage();
    }

    private void OnUse2Click()
    {
        bClicked = bUse2;
        GameInput.RebindUse2(deviceType, OnComplete, OnCancel);
        ShowBindingMessage();
    }

    private void OnUse3Click()
    {
        bClicked = bUse3;
        GameInput.RebindUse3(deviceType, OnComplete, OnCancel);
        ShowBindingMessage();
    }

    private void OnSubmitUIClick()
    {
        bClicked = bSubmitUI;
        GameInput.RebindSubmitUI(deviceType, OnComplete, OnCancel);
        ShowBindingMessage();
    }

    private void OnExitClick()
    {
        if (modified) //If there are unsaved changes, prompt the player with a message box asking 
        {             //whether to save
            if (mBox != null)
            {
                mBox.Close();
            }
            else
                mBox = UIManager.Instance.GetUIMessageBox();

            mBox.Open(
                LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, UIMessageBox.LK_CHANGE_N_SAVED_T),
                LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, UIMessageBox.LK_CHANGE_N_SAVED_AC),
                LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, LocalizationHelper.LK_SAVE),
                () => { Close(); Save(); },
                true,
                () => { Close(); GameManager.Instance.LoadInputSettings(true); }, //Reverts any changes made
                0,
                UIMessageBox.MsgType.Warning,
                true
            );
        }
        else
            Close();
        /*
         *  If there are any changes, it indicates the presence of new binding overrides. However, 
         *  the loaded input settings are also overrides.
         *  If the new overrides are not saved, the old overrides must be reloaded using LoadInputSettings(true).
         */
    }

    private void OnResetClick()
    {
        GameInput.ResetBindings(deviceType);
        SetModified();
        UpdateUI();
    }

    private void Save()
    {
        GameManager.Instance.SaveInputSettings();

        modified = false; //Prevents redundant calls to Init()
        SetSaveInteractable(false);

        if (mBox != null)
        {
            mBox.Close();
        }
        else
            mBox = UIManager.Instance.GetUIMessageBox();

        mBox.Open(
            "",
            LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, UIMessageBox.LK_SAVE_SUCCESSFULL),
            null,
            null,
            true,
            null,
            2f,
            UIMessageBox.MsgType.Generic,
            false
        );
    }

    private void OnSaveClick()
    {
        UIManager.Instance.EventSystemHandler.UpdateSelection(bExit.gameObject);
        if (modified)
        {
            Save();
        }
    }

    private void Close()
    {
        switch (deviceType)
        {
            case GameInput.DeviceType.Keyboard:
                parent.ShowBindingsKeyboard(false);
            break;

            case GameInput.DeviceType.Gamepad:
                parent.ShowBindingsController(false);
            break;
        }
        scrollView.ScrollY(0, 220f, 220f);
    }

    //Event Handlers
    private void Move(AxisEventData eventData)
    {
        if(
            eventData.selectedObject == bSave.gameObject ||
            eventData.selectedObject == bReset.gameObject ||
            eventData.selectedObject == bExit.gameObject
        )
            return; //Does nothing!

        //Since the button is within a horizontal layout, we use the horizontal layout's position 
        //to determine the position in the content GameObject.
        float y = eventData.selectedObject.transform.parent.GetComponent<RectTransform>().anchoredPosition.y;
        scrollView.ScrollY(y, 220f, 220f);
    }

    /*
     * Used exclusively during the rebinding process for the gamepad!
     * If the user is rebinding the gamepad and it gets disconnected, a message box with a timer will appear
     * to notify them of the issue. If the user fails to reconnect a gamepad within the time limit, all changes
     * will be discarded, and the game will return to the settings home.
     */
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if(!GameInput.IsGamepadConnected()) //No Gamepad connected
        {
            if(waitReconnect) //Is waiting for reconnection
                return;

            //Begin waiting for reconnection
            waitReconnect = true;

            if (isRebinding)
            {
                GameInput.CancelRebind();
                isRebinding = false;
            }

            if (mBox != null)
            {
                mBox.Close();
            }
            else
                mBox = UIManager.Instance.GetUIMessageBox();

            mBox.Open(
                LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, LK_GAMEPAD_DISCONNECTED),
                LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, LK_GAMEPAD_ASK_RECONNECT),
                null,
                null,
                false,
                () => { Close(); GameManager.Instance.LoadInputSettings(true); },
                60f,
                UIMessageBox.MsgType.Error,
                true,
                2,
                true
            );
        }
        else
        {
            if(waitReconnect) //Was waiting for reconnection
            {
                waitReconnect = false; //Stop waiting for reconnection

                if (mBox != null)
                {
                    mBox.Close();
                }
            }
        }
    }

    //Implementation of the IPageUI interface
    public void OnPausePage()
    {
        OnExitClick();
    }

    public GameObject GetFirstSelected()
    {
        return bForward.gameObject;
    }
}

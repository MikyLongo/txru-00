/*
 * This script handles interactions in the Home of the Settings menu.
 * For more information on how the settings menu and its submenus are managed, refer to the UISettings class.
 */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UISettingsHome : MonoBehaviour, IPageUI
{
    [SerializeField] private UISettings parent;
    [SerializeField] private Button bGeneralSettings;
    [SerializeField] private Button bBindingsKeyboard;
    [SerializeField] private Button bBindingsController;
    [SerializeField] private Button bMainMenu;
    [SerializeField] private Button bExit;
    [SerializeField] private bool returnToMainMenu = false; //Enable/Disable the button

    private void OnEnable()
    {
        UIManager.Instance.OpenedPage = this;
        bGeneralSettings.onClick.AddListener(OnGeneralSettingsClick);
        bBindingsKeyboard.onClick.AddListener (OnBindingsKeyboardClick);
        bBindingsController.onClick.AddListener(OnBindingsControllerClick);
        bMainMenu.onClick.AddListener(OnMainMenuClick);
        bMainMenu.gameObject.SetActive(returnToMainMenu);
        bExit.onClick.AddListener(OnExitClick);

        //Enables or disables the button to access the submenu for gamepad rebinding
        bBindingsController.interactable = GameData.GameInput.IsGamepadConnected();
        //Registers to onDeviceChange to listen for Gamepad connection or disconnection events
        InputSystem.onDeviceChange += OnDeviceChange;

        UpdateNavigation();
    }

    private void OnDisable()
    {
        bGeneralSettings.onClick.RemoveAllListeners();
        bBindingsKeyboard.onClick.RemoveAllListeners();
        bBindingsController.onClick.RemoveAllListeners();
        bMainMenu.onClick.RemoveAllListeners();
        bExit.onClick.RemoveAllListeners();
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    public bool ReturnToMainMenu
    {
        set { returnToMainMenu = value; bMainMenu.gameObject.SetActive(value); UpdateNavigation(); }
    }

    public void OnGeneralSettingsClick()
    {
        parent.ShowGeneralSettings(true);
    }

    public void OnBindingsKeyboardClick()
    {
        parent.ShowBindingsKeyboard(true);
    }

    public void OnBindingsControllerClick()
    {
        parent.ShowBindingsController(true);
    }

    public void OnMainMenuClick()
    {
        GameManager.Instance.ReturnToMainMenu();
    }

    public void OnExitClick()
    {
        parent.ShowHome(false);
    }

    private void UpdateNavigation()
    {
        Navigation navBindKB = bBindingsKeyboard.navigation;
        Navigation navBindController = bBindingsController.navigation;
        Navigation navMainMenu = bMainMenu.navigation;
        Navigation navExit = bExit.navigation;

        if(returnToMainMenu && bBindingsController.interactable)
        {
            navBindKB.selectOnDown = bBindingsController;
            navBindController.selectOnDown = bMainMenu;
            navMainMenu.selectOnUp = bBindingsController;
            navExit.selectOnUp = bMainMenu;
        }
        else if(returnToMainMenu)
        {
            navBindKB.selectOnDown = bMainMenu;
            navMainMenu.selectOnUp = bBindingsKeyboard;
            navExit.selectOnUp = bMainMenu;
        }
        else if(bBindingsController.interactable)
        {
            navBindKB.selectOnDown = bBindingsController;
            navBindController.selectOnDown = bExit;
            navExit.selectOnUp = bBindingsController;
        }
        else
        {
            navBindKB.selectOnDown = bExit;
            navExit.selectOnUp = bBindingsKeyboard;
        }

        bBindingsKeyboard.navigation = navBindKB;
        bBindingsController.navigation = navBindController;
        bMainMenu.navigation = navMainMenu;
        bExit.navigation = navExit;
    }

    //Event Handler
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (GameData.GameInput.IsGamepadConnected())
            bBindingsController.interactable = true;
        else
        {
            bBindingsController.interactable = false;
            UIManager.Instance.EventSystemHandler.UpdateSelection(bGeneralSettings.gameObject);
        }

        UpdateNavigation();
    }

    //Implementation of the IPageUI interface
    public void OnPausePage()
    {
        OnExitClick();
    }

    public GameObject GetFirstSelected()
    {
        return bGeneralSettings.gameObject;
    }
}

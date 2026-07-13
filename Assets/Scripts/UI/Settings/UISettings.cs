/*
 * The settings menu consists of a home menu and three submenus accessible from the home.
 * This script acts as a mediator between the submenus and other components (e.g., UIManager).
 * It contains references to the Canvas of each submenu and the home menu, and provides a mechanism 
 * to navigate from one submenu to another.
 */

using UnityEngine;

public class UISettings : MonoBehaviour
{
    [SerializeField] private GameObject UIHome;
    [SerializeField] private GameObject UIGeneralSettings;
    [SerializeField] private GameObject UIBindingsKeyboard;
    [SerializeField] private GameObject UIBindingsController;
    [SerializeField] private bool returnToMainMenu = false; //Used in UIHome to enable/disable a button

    private void OnEnable()
    {
        UIHome.GetComponent<UISettingsHome>().ReturnToMainMenu = returnToMainMenu;
        UIHome.SetActive(true);
        UIGeneralSettings.SetActive(false);
        UIBindingsKeyboard.SetActive(false);
        UIBindingsController.SetActive(false);
    }

    public bool ReturnToMainMenu
    {
        set 
        { 
            returnToMainMenu = value;
            UIHome.GetComponent<UISettingsHome>().ReturnToMainMenu = value;
        }
    }

    public void ShowHome(bool show) //Opens (true) or closes (false) the settings menu
    {
        UIManager.Instance.ShowSettingsUI(show);
    }

    public void ShowGeneralSettings(bool show) 
    {
        UIHome.SetActive(!show);
        UIGeneralSettings.SetActive(show);
    }

    public void ShowBindingsKeyboard(bool show) 
    {
        UIHome.SetActive(!show);
        UIBindingsKeyboard.SetActive(show);
    }

    public void ShowBindingsController(bool show)
    {
        UIHome.SetActive(!show);
        UIBindingsController.SetActive(show);
    }
}

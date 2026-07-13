//Handles interaction with the Main Menu UI
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour, IPageUI
{
    [SerializeField] private Button bNewGame;
    [SerializeField] private Button bLoadGame;
    [SerializeField] private Button bSettings;
    [SerializeField] private Button bCredits;
    [SerializeField] private Button bExit;

    private bool initialized = false;
    private bool hasSavedMemory = false; //Used to enable or disable the Load Game button

    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        ManageListeners(false);
    }

    private void Init()
    {
        if (!initialized)
        {
            hasSavedMemory = GameManager.Instance.HasSavedMemory;
            initialized = true;
        }

        ManageListeners(true);
        UIManager.Instance.OpenedPage = this; //Notifies the UIManager that an IPageUI has been opened
    }

    private void ManageListeners(bool add)
    {
        if(add)
        {
            bNewGame.onClick.AddListener(OnNewGameClick);
            if (hasSavedMemory)
            {
                bLoadGame.interactable = true;
                bLoadGame.onClick.AddListener(OnLoadGameClick);
            }
            else //Disables the Load Game button if no save data is available
            {
                bLoadGame.interactable = false;
            }
            bSettings.onClick.AddListener(OnSettingsClick);
            bCredits.onClick.AddListener(OnCreditsClick);
            bExit.onClick.AddListener(OnExitClick);
        }
        else
        {
            bNewGame.onClick.RemoveAllListeners();
            bLoadGame.onClick.RemoveAllListeners();
            bSettings.onClick.RemoveAllListeners();
            bCredits.onClick.RemoveAllListeners();
            bExit.onClick.RemoveAllListeners();
        }
    }

    private void OnNewGameClick()
    {
        UIManager.Instance.ShowHandleSaveFileUI(true, false);
    }

    private void OnLoadGameClick() 
    {
        UIManager.Instance.ShowHandleSaveFileUI(true, true);
    }

    private void OnSettingsClick()
    {
        UIManager.Instance.ShowSettingsUI(true, false);
    }

    private void OnCreditsClick()
    {
        UIManager.Instance.ShowCreditsUI(true);
    }

    public void OnExitClick()
    {
        /*
         * Use a Message Box to ask for confirmation before closing the game.
         * Since the second button is the "Close" button, to avoid misunderstanding, 
         * the second button will close the game, while the first button will serve 
         * as the "Cancel" option and do nothing (other than closing the message window).
         */
        UIManager.Instance.GetUIMessageBox().Open(
                LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, UIMessageBox.LK_CLOSING_GAME_T),
                LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, UIMessageBox.LK_CLOSING_GAME_AC),
                LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, LocalizationHelper.LK_CANCEL),
                () => { },
                true,
                () => { GameManager.Instance.CloseGameApp(); },
                0,
                UIMessageBox.MsgType.Warning,
                true,
                1
            );
    }

    //Implementation of the IPageUI interface
    public void OnPausePage()
    {
        OnExitClick();
    }

    public GameObject GetFirstSelected()
    {
        return bNewGame.gameObject;
    }
}

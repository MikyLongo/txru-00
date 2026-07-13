/*
 * This manager handles all the UI screens (called "UI containers" because they contain UI elements) 
 * and manages interaction with the UI. For other scripts, this manager acts as the intermediary for 
 * interacting with the UI.
 * 
 * The manager determines which UI container should be displayed in each scene, allows enabling/disabling 
 * user input related to the UI, and provides methods for requesting the opening/closing of a UI container.
 * 
 * UI containers can include menu-like screens, overlays, gameplay UIs, loading screens, and modal 
 * windows/screens. 
 * Currently, only one UI container can be open at a time and only one modal window at a time. Opening a new UI 
 * container requires closing the existing one, and the same applies to modal windows. 
 * However, a UI container and a modal window can coexist simultaneously.
 * 
 * Since input is handled with the InputSystem package, the scene must include an Event System and an 
 * Input System UI Input Module. The Event System is necessary to manage events, and the Input Module assigns 
 * actions from the InputSystem to each required UI interaction. 
 * 
 * For game design purposes, mouse input is not supported, so we need a way to inform the Event System which UI 
 * element is currently selected or focused.
 * 
 * The Event System is managed through an instance of the custom class UIEventSystemHandler (see for more info). 
 * Together with the interface system implemented via IPageUI and IModalUI, this setup enables handling UI 
 * containers and interactions with UI elements (refer to IPageUI and IModalUI interfaces for more information).
 * 
 * In short, IPageUI represents a container UI, while IModalUI represents a modal window. Not all container UIs 
 * are IPageUI—for example, overlays that do not require interaction.
 * 
 * For the UI system to function properly, the opening of an IPageUI or an IModalUI must be reported to the 
 * UIManager using the OpenedPage and OpenedModal properties. It must also be notified to UIEventSystemHandler 
 * by accessing it through the methods provided by the manager. (For IPageUI, this is done by the UIManager, 
 * while IModalUI handles this notification on its own.)
 * 
 * A special interaction is triggered by pressing the "Pause/Menu" button. This launches the OnPause method, 
 * which notifies the appropriate UI container or modal window of the button press, enabling them to handle 
 * the request (see below for more details).
 */

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;

    //UI Prefabs
    [SerializeField] private UIMainMenu prefabMainMenu;
    [SerializeField] private UISettings prefabSettings;
    [SerializeField] private UICredits  prefabCredits;
    [SerializeField] private UILevelHub prefabLevelHub;
    [SerializeField] private UIGameplay prefabGameplay;
    [SerializeField] private UIPause    prefabPause;
    [SerializeField] private UIGameOver prefabGameOver;
    [SerializeField] private UIGameWin  prefabGameWin;
    [SerializeField] private UIGameHelp prefabGameHelp;
    [SerializeField] private UIDemoRankLevel prefabDemoRankLevel;
    [SerializeField] private UIOverlay  prefabOverlay;
    [SerializeField] private UIHandleSaveFile prefabHandleSaveFile;

    [SerializeField] private UIMessageBox prefabMessageBox;
    [SerializeField] private UILoadingScreen prefabLoadingScreen;
    [SerializeField] private UILevelInfo prefabLevelInfo;

    [SerializeField] private EventSystem prefabEventSystem;

    //UI Elements: 
    [SerializeField] private UIMainMenu mainMenuUI = null;
    [SerializeField] private UISettings settingsUI = null;
    [SerializeField] private UICredits  creditsUI = null;
    [SerializeField] private UIHandleSaveFile handleSaveFileUI = null;
    [SerializeField] private UILevelHub levelHubUI = null;
    [SerializeField] private UIGameplay gameplayUI = null;
    [SerializeField] private UIPause    pauseUI = null;
    [SerializeField] private UIGameOver gameOverUI = null;
    [SerializeField] private UIGameWin  gameWinUI = null;
    [SerializeField] private UIGameHelp gameHelpUI = null;
    [SerializeField] private UIDemoRankLevel demoRankLevelUI = null;
    [SerializeField] private UIOverlay  overlayUI = null;
    [SerializeField] private UILoadingScreen loadingScreenUI = null;

    //UI Elements: Modals
    [SerializeField] private UIMessageBox messageBoxUI = null;
    [SerializeField] private UILevelInfo levelInfoUI = null;

    [SerializeField] private IPageUI openedPage = null;
    [SerializeField] private IModalUI openedModal = null;

    //EventSystem
    [SerializeField] private EventSystem eventSystem = null;
    [SerializeField] private UIEventSystemHandler eventSystemHandler = null;

    //Input
    private InputAction pauseAction = null;
    private bool pausePressed = false;

    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<UIManager>(true);
                if (_instance == null)
                {
                    Debug.LogWarning("No UIManager available in the scene!");
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

    private void OnEnable()
    {
        Init();
        ManageEventSubscription(true);
    }

    private void OnDisable()
    {
        ManageEventSubscription(false);
    }

    //Functions
    //Functions: UI Elements
    //Generic function to retrieve UI elements (EventSystem, MainMenu, Settings, etc.).
    private void GetUIObject<T>(ref T uiObj, T prefabUI) where T : UnityEngine.Object
    {
        if (uiObj == null)
        {
            if ((uiObj = GameObject.FindObjectOfType<T>(true)) == null)
            {
                uiObj = Instantiate(prefabUI, Vector3.zero, Quaternion.identity);
            }
        }
    }

    private EventSystem GetEventSystem()
    {
        GetUIObject<EventSystem>(ref eventSystem, prefabEventSystem);
        if (!eventSystem.isActiveAndEnabled)
        {
            eventSystem.gameObject.SetActive(true);
            eventSystem.enabled = true;
        }
        return eventSystem;
    }

    private UILoadingScreen GetUILoadingScreen()
    {
        GetUIObject<UILoadingScreen>(ref loadingScreenUI, prefabLoadingScreen);
        return loadingScreenUI;
    }

    public UIMessageBox GetUIMessageBox()
    {
        GetUIObject<UIMessageBox>(ref messageBoxUI, prefabMessageBox);
        return messageBoxUI;
    }

    private UIMainMenu GetUIMainMenu()
    {
        GetUIObject<UIMainMenu>(ref mainMenuUI, prefabMainMenu);
        return mainMenuUI;
    }

    private UISettings GetUISettings()
    {
        GetUIObject<UISettings>(ref settingsUI, prefabSettings);
        return settingsUI;
    }

    private UICredits GetUICredits()
    {
        GetUIObject<UICredits>(ref creditsUI, prefabCredits);
        return creditsUI;
    }

    private UIHandleSaveFile GetUIHandleSaveFile()
    {
        GetUIObject<UIHandleSaveFile>(ref handleSaveFileUI, prefabHandleSaveFile);
        return handleSaveFileUI;
    }

    private UILevelHub GetUILevelHub()
    {
        GetUIObject<UILevelHub>(ref levelHubUI, prefabLevelHub);
        return levelHubUI;
    }

    public UIGameplay GetUIGameplay()
    {
        GetUIObject<UIGameplay>(ref gameplayUI, prefabGameplay);
        return gameplayUI;
    }

    private UIPause GetUIPause()
    {
        GetUIObject<UIPause>(ref pauseUI, prefabPause);
        return pauseUI;
    }

    private UIGameOver GetUIGameOver()
    {
        GetUIObject<UIGameOver>(ref gameOverUI, prefabGameOver);
        return gameOverUI;
    }

    private UIGameWin GetUIGameWin()
    {
        GetUIObject<UIGameWin>(ref gameWinUI, prefabGameWin);
        return gameWinUI;
    }

    private UIGameHelp GetUIGameHelp()
    {
        GetUIObject<UIGameHelp>(ref gameHelpUI, prefabGameHelp);
        return gameHelpUI;
    }
    
    public UILevelInfo GetUILevelInfo()
    {
        GetUIObject<UILevelInfo>(ref levelInfoUI, prefabLevelInfo);
        return levelInfoUI;
    }

    private UIOverlay GetUIOverlay()
    {
        GetUIObject<UIOverlay>(ref overlayUI, prefabOverlay);
        return overlayUI;
    }

    private UIDemoRankLevel GetUIDemoRankLevel()
    {
        GetUIObject<UIDemoRankLevel>(ref demoRankLevelUI, prefabDemoRankLevel);
        return demoRankLevelUI;
    }

    //Functions: Manage UI Elements
    private GameObject GetCurrentLevelUI()
    {
        switch (GameManager.Instance.GetCurrentLevelType())
        {
            case GameData.LevelSO.LevelType.MainMenu:
                return GetUIMainMenu().gameObject;

            case GameData.LevelSO.LevelType.LevelHub:
                return GetUILevelHub().gameObject;

            case GameData.LevelSO.LevelType.LevelGameplay: //Has two associated UI containers.
                if(pausePressed) //When the pause is active, the container is the pauseUI.
                    return GetUIPause().gameObject;
                
                return GetUIGameplay().gameObject; //Otherwise, the container is the gameplayUI.

            case GameData.LevelSO.LevelType.DemoRankLevel:
                return GetUIDemoRankLevel().gameObject;
        }

        return null;
    }

    public void ShowSettingsUI(bool show, bool toMainMenu = true)
    {
        GetCurrentLevelUI().SetActive(!show);
        GetUISettings();

        if(show)
            settingsUI.ReturnToMainMenu = toMainMenu;

        settingsUI.gameObject.SetActive(show);
    }

    public void ShowCreditsUI(bool show)
    {
        GetCurrentLevelUI().SetActive(!show);
        GetUICredits();
        creditsUI.gameObject.SetActive(show);
    }

    public void ShowHandleSaveFileUI(bool show, bool load = false)
    {
        GetCurrentLevelUI().SetActive(!show);
        GetUIHandleSaveFile();

        handleSaveFileUI.Show(show, load);
    }

    public void ShowPauseUI(bool show)
    {
        //Only used for scenes of type Gameplay, where we have two mutually exclusive UIs.
        GetUIPause().gameObject.SetActive(show);
        GetUIGameplay().gameObject.SetActive(!show);
        pausePressed = show; //Ensure that GetCurrentLevelUI() works correctly!
    }

    public void ShowGameOverUI(bool show, bool withContinue)
    {
        GetCurrentLevelUI().SetActive(!show);
        GetUIGameOver();

        if(show)
            gameOverUI.HasContinue = withContinue;

        gameOverUI.gameObject.SetActive(show);
    }

    public UIGameWin ShowGameWinUI(bool show)
    {
        GetCurrentLevelUI().SetActive(!show);
        GetUIGameWin().gameObject.SetActive(show);

        if(show)
            return gameWinUI;

        return null;
    }

    public void ShowGameHelpUI(bool show)
    {
        GetCurrentLevelUI().SetActive(!show);
        GetUIGameHelp().gameObject.SetActive(show);
    }

    public UILoadingScreen ShowLoadingScreenUI(bool show)
    {
        GetCurrentLevelUI().SetActive(!show);
        GetUILoadingScreen().gameObject.SetActive(show);

        if(show)
            return loadingScreenUI;

        return null;
    }

    //Functions: 
    private void Init()
    {
        //Initialize references
        GetEventSystem();
        eventSystemHandler = new UIEventSystemHandler(eventSystem);

        pausePressed = false;

        OpenedModal = null;
        OpenedPage = null;

        mainMenuUI = null;
        settingsUI = null;
        creditsUI = null;
        handleSaveFileUI = null;
        levelHubUI = null;
        pauseUI = null;
        gameplayUI = null;
        gameOverUI = null;
        gameWinUI = null;
        gameHelpUI = null;
        demoRankLevelUI = null;
        HandleOverlay();

        messageBoxUI = null;
        loadingScreenUI = null;
        levelInfoUI = null;

        //Establish which UI should be displayed.
        switch (GameManager.Instance.GetCurrentLevelType())
        {
            case GameData.LevelSO.LevelType.MainMenu:
                GetUIMainMenu().gameObject.SetActive(true);
                OpenedPage = mainMenuUI;
                HandleActions(true);
                break;

            case GameData.LevelSO.LevelType.LevelHub:
                GetUILevelHub().gameObject.SetActive(true);
                OpenedPage = levelHubUI;
                HandleActions(true);
                break;

            case GameData.LevelSO.LevelType.LevelGameplay:
                GetUIGameplay().gameObject.SetActive(true);
                HandleActions(false);
                break;

            case GameData.LevelSO.LevelType.DemoRankLevel:
                GetUIDemoRankLevel().gameObject.SetActive(true);
                OpenedPage = demoRankLevelUI;
                HandleActions(true);
                break;
        }
    }

    public UIEventSystemHandler EventSystemHandler
    {
        get
        {
            if (eventSystemHandler == null)
            {
                eventSystemHandler = new UIEventSystemHandler(GetEventSystem());
            }

            return eventSystemHandler;
        }
    }

    /*
     * Called by the UI page to notify that a UI container has been opened! 
     * It is recommended to call this after calculating the first selected GameObject (if needed).
     */
    public IPageUI OpenedPage
    {
        private get { return openedPage; }
        set 
        { 
            openedPage = value;

            if (openedPage != null) 
                eventSystemHandler.PageOpened(openedPage.GetFirstSelected());
        }
    }

    public IModalUI OpenedModal
    {
        private get { return openedModal; }
        set { openedModal = value; }
    }

    private void ManageEventSubscription(bool subscribe)
    {
        if (subscribe)
        {
            GameManager.OnSceneUpdated += OnSceneChanged;
            GameManager.OnGeneralSettingsUpdate += OnGeneralSettingsUpdate;
            pauseAction.performed += OnPause;
        }
        else
        {
            GameManager.OnSceneUpdated -= OnSceneChanged;
            GameManager.OnGeneralSettingsUpdate -= OnGeneralSettingsUpdate;
            pauseAction.performed -= OnPause;
        }
    }

    private void HandleOverlay()
    {
        GetUIOverlay().gameObject.SetActive(true);
        overlayUI.ShowFPS(GameManager.Instance.Settings.ShowFPS);
    }

    private void HandleActions(bool enable)
    {
        if (pauseAction == null)
            pauseAction = GameData.GameInput.GetPauseUIAction();

        if (enable)
        {
            GameData.GameInput.EnableUIActionMap();
        }
        else
        {
            GameData.GameInput.DisableUIActionMap();
        }
    }

    public void RequestInteraction(bool interact)
    {
        HandleActions(interact);
    }

    //Event handlers
    //Event handlers: GameManager
    private void OnSceneChanged()
    {
        Init();
    }

    private void OnGeneralSettingsUpdate()
    {
        HandleOverlay();
    }

    //Event handlers: Input
    private void OnPause(InputAction.CallbackContext context)
    {
        //Modal windows have priority (they overlap the UI container and gain focus).
        if (OpenedModal != null) 
        {
            OpenedModal.OnPauseModal();
        }
        else if (OpenedPage != null)
        {
            OpenedPage.OnPausePage();
        }
    }
}
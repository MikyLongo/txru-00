/*
 * The most important manager of the game:
 * - Holds references to essential resources required for the game to function (and is the only one).
 * - Manages all settings, game data, and game saves.
 * - Handles scenes and transitions between them.
 * - Manages I/O operations related to memory (e.g., New Game, Load Game, Auto Save, Save/Load settings).
 * - Controls game pausing and toggles user interactions (UI and gameplay).
 * - Provides methods for other managers/scripts to access essential resources or make requests for scene,
 *   level, memory, and settings handling.
 * - Persists across scene transitions, along with its child GameObjects (UIManager, SoundManager,
 *   and EventSystem).
 * 
 * PACKAGES:
 * - Localization Package:
 *   Used for game localization, with helper methods provided by the static LocalizationHelper class
 *   (see "Assets/Scripts/Localization/LocalizationHelper.cs").
 * - InputSystem Package:
 *   Handles input configuration and device management with helper methods provided by the static GameInput 
 *   class (see "Assets/Scripts/GameData/GameInput.cs").
 * 
 * SETTINGS:
 * - Settings are categorized into general and input settings, loaded/saved separately.
 * - Input settings rely on the InputSystem package and require the InputActionAsset resource.
 * 
 * ESSENTIAL RESOURCES:
 * - InputActionAsset: Required to work with the InputSystem package.
 * 
 * - GameData.LevelListSO: A ScriptableObject that contains a list of LevelSO objects, which represent 
 *   scene/level configurations. Each LevelSO includes default gameplay data and defines how the associated 
 *   scene will function. The index of each LevelSO in the list corresponds to the scene index in the Build 
 *   Settings. Specifically, an index match (index in Build Settings == index in the list) ensures they 
 *   represent the same scene.
 *   The order in the Build Settings follows specific rules to ensure proper behavior for scene handling 
 *   by the GameManager.
 *   LevelSO also contains default data used during gameplay. One example is the default record to beat in 
 *   the level associated with the scene. While this default data can change over time, the updated version 
 *   is stored using the GameLevel struct. The GameLevel struct acts as a wrapper for the mutable data of 
 *   LevelSO and contains additional information. It is part of the game data that gets saved. 
 *   For further details, refer to LevelSO.cs, LevelListSO.cs, GameLevel.cs, and LevelState.cs in 
 *   "Assets/Scripts/GameData/GameLevel".
 * 
 * - ItemListID: A ScriptableObject consisting of a list where each type of item in the game is assigned a 
 *   unique ID. For more information, refer to Item.cs and ItemListID.cs in "Assets/Scripts/Gameplay/Item".
 * 
 * - GameData.GameSetting: A struct containing the configuration of general settings for the game. 
 *   These settings can be modified and applied by the GameManager. 
 *   For additional information, see GameSetting.cs in "Assets/Scripts/GameData".
 * 
 * - GameData.GameMemory: A struct that holds data for the game file currently loaded.
 *   For additional information, see GameMemory.cs in "Assets/Scripts/GameData".
 *
 * GAME DATA (Game Saves):
 * - Supports up to 10 game save profiles, each with auto-save functionality (manual saves are disabled).
 * - Each game profile supports checkpoints to save and load the state of a level in progress.
 * - Each game profile is associated with a specific slot number.
 * 
 * I/O OPERATIONS:
 * - All scripts requiring memory I/O operations rely on the GameManager, which uses methods from the
 *   static GameSaver class (see "Assets/Scripts/Static/GameSaver.cs").
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    //Settings
    [SerializeField] private GameData.GameSetting settings;  //General settings
    [SerializeField] private InputActionAsset inputSettings; //Input settings (InputSystem Package)

    //GameMemory
    [SerializeField] private GameData.GameMemory gameMemory; //The currently loaded game profile.
    private int memorySlot = 0; //The slot number of the currently loaded game profile.

    //Scene && Level management
    private Coroutine loadSceneCoroutine = null;  
    [SerializeField] private int currentScene = 0; //0 = MainMenu, 1 = Level Hub, 2,3,4.. = GameLevels
    [SerializeField] private GameData.LevelListSO levelListSO = null; //Default settings for every scene/level
    private bool continueLevel = false; 
    //Defines whether the level starts from zero (false) or from a checkpoint (true)

    //Gameplay
    [SerializeField] private ItemsListID itemsIDs = null;

    //My Events
    public static event Action OnGeneralSettingsUpdate;//Event invoked to notify changes in the general settings.
    public static event Action OnSceneUpdated;
    /*
     *  The GameManager needs to update some data (e.g., currentScene) when the scene changes.
     *  This event is invoked after the GameManager performs these updates.
     *  Other scripts need to listen to this event instead of the one provided by the SceneManager
     *  due to a synchronization issue.
     */

    public static GameManager Instance
    {
        get 
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameManager>(true);
                if( _instance == null ) 
                {
                    Debug.LogError("No GameManager available in the scene!");
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
            GameObject.DontDestroyOnLoad(this.gameObject);
        }
        else
            GameObject.Destroy(this.gameObject);
    }

    private void OnEnable()
    {
        Cursor.visible = false;
        LoadGeneralSettings();
        LoadInputSettings(true);
        GetCurrentScene();
        ManageEventSubscription(true);
    }

    private void OnDisable()
    {
        ManageEventSubscription(false);
        LoadSceneCoroutineDispose();
    }

    //General Settings
    public GameData.GameSetting Settings //Getter for the general settings
    { 
        get { return settings; } 
    }

    private void LoadGeneralSettings()
    {
        settings = GameSaver.LoadSettings();
        ApplyGeneralSettings();
    }

    public void SaveGeneralSettings(GameData.GameSetting settings)
    {
        this.settings = settings;
        ApplyGeneralSettings();
        GameSaver.SaveSettings(settings);
    }

    private void ApplyGeneralSettings()
    {
        LocalizationHelper.SetCurrentLanguage(settings.LocalizationID);

        //Screen.SetResolution(settings.Resolution.Width, settings.Resolution.Height, settings.FullScreen);
        RefreshRate hz = new RefreshRate
        {
            numerator = settings.Resolution.HzNum,
            denominator = settings.Resolution.HzDen
        };

        Screen.SetResolution(
            settings.Resolution.Width,
            settings.Resolution.Height,
            (settings.FullScreen)? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed,
            hz
        );

        QualitySettings.vSyncCount = settings.VSync ? 1 : 0;

        //Other settings are handled by other scripts registered to this event!
        OnGeneralSettingsUpdate?.Invoke();
    }

    //Input Settings
    public InputActionAsset LoadInputSettings(bool reload)
    {
        if(reload)
            GameSaver.LoadInputSettings(inputSettings);

        return inputSettings;
    }

    public void SaveInputSettings()
    {
        GameSaver.SaveInputSettings(inputSettings);
    }

    //GameMemory
    public GameData.GameMemory Memory
    {
        get { return gameMemory; }
    }

    public bool HasSavedMemory { get { return GameSaver.HasSavedMemory(); } }

    public List<GameData.GameMemory.PartialMemory> LoadMemoryInfos()
    {
        return GameSaver.LoadDataInfos();
    }

    private bool LoadMemory(int numSlot)
    {
        GameData.GameMemory? tempMem = GameSaver.LoadGame(numSlot);

        if (tempMem == null) //No saved data
        {
            Debug.LogError($"No memory data found in slot {numSlot}");
            return false;
        }
        else
        {
            memorySlot = numSlot;
            gameMemory = tempMem.Value;
            return true;
        }
    }

    private void SaveMemory()
    {
        gameMemory.LastSave = DateTime.Now.ToString("o");
        //The "o" format ensures that the DateTime string representation can be converted back 
        //to the exact same DateTime value without losing any precision.
        GameSaver.SaveGame(gameMemory, memorySlot);
    }

    public void SaveState(List<IState> states) //Checkpoint save state
    {
        GameSaver.SaveLevelState(states, memorySlot);
    }

    public List<IState> LoadState()
    {
        return GameSaver.LoadLevelState(memorySlot); //Checkpoint load state
    }

    //Scene && Level management
    private void GetCurrentScene()
    {
        currentScene = SceneManager.GetActiveScene().buildIndex;
    }

    public GameData.GameLevel GetCurrentLevel()
    {
        return gameMemory.Levels[GetCurrentLevelIndex()];
    }

    public GameData.GameLevel GetLevel(int level)
    {
        return gameMemory.Levels[level - 1];
    }

    public GameData.LevelSO GetCurrentRawLevel()
    {
        return levelListSO.list[currentScene];
    }

    public GameData.LevelSO GetRawLevel(int level) //level > 0
    {
        return levelListSO.list[GetRawLevelIndex(level)];
    }

    public int GetRawLevelIndex(int level)
    {
        return levelListSO.levelStartingIndex + level - 1;
    }

    public List<AudioClip> GetCurrentLevelBGMs()
    {
        return levelListSO.list[currentScene].BGMs;
    }

    public GameData.LevelSO.LevelType GetCurrentLevelType()
    {
        return levelListSO.list[currentScene].levelType;
    }

    public int GetCurrentLevelIndex()
    {
        //The list contains only Gameplay-type scenes (MainMenu & LevelHub are excluded as they are "non-level").
        return (currentScene - levelListSO.levelStartingIndex); 
    }

    public int GetTotalLevels()
    {
        //Excluding "non-level" scenes from the count.
        return levelListSO.list.Count - levelListSO.levelStartingIndex;  
    }

    //Gameplay
    public ItemsListID ItemsIDs { get { return itemsIDs; } }

    //Actions
    //Actions: Main Menu
    public void NewGame(int numSlot)
    {
        memorySlot = numSlot;
        gameMemory = GameData.GameMemory.GetDefaultMemory();
        gameMemory.Levels = new List<GameData.GameLevel>
            {
                GameData.GameLevel.ToGameLevel(levelListSO.list[levelListSO.levelStartingIndex])
            };

        //LevelHub will always appear before Level 1.
        //loadSceneCoroutine = StartCoroutine(LoadScene(levelListSO.levelStartingIndex - 1));
        LaunchLoadSceneCoroutine(levelListSO.levelStartingIndex - 1);
        //Note: The new game file is not saved until the player accesses the first level.
    }

    public void LoadGame(int numSlot)
    {
        if (LoadMemory(numSlot))
        {
            //LevelHub will always appear before Level 1.
            //loadSceneCoroutine = StartCoroutine(LoadScene(levelListSO.levelStartingIndex - 1));
            LaunchLoadSceneCoroutine(levelListSO.levelStartingIndex - 1);
        } //else error
    }

    public void CloseGameApp()
    {
        Debug.Log("Gioco chiuso!");
        Application.Quit();
    }

    //Actions: LevelHub & Gameplay
    public void StartLevel(int level)
    {
        continueLevel = false;
        Time.timeScale = 0f;
        //loadSceneCoroutine = StartCoroutine(LoadScene(GetRawLevelIndex(level)));
        LaunchLoadSceneCoroutine(GetRawLevelIndex(level));
    }

    public void StartNextLevel()
    {
        int level = GetCurrentLevel().Level;
        if (level < GetTotalLevels())
        {
            StartLevel(level + 1);
        }
    }

    public void RestartLevel()
    {
        continueLevel = false;
        Time.timeScale = 0f;
        SceneManager.LoadScene(currentScene);
    }

    public void ContinueLevel(int level)
    {
        continueLevel = true;
        Time.timeScale = 0f;
        //loadSceneCoroutine = StartCoroutine(LoadScene(GetRawLevelIndex(level)));
        LaunchLoadSceneCoroutine(GetRawLevelIndex(level));
    }

    public bool IsContinueLevel() { return continueLevel; }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        //loadSceneCoroutine = StartCoroutine(LoadScene(0)); //Main menu = scene 0
        LaunchLoadSceneCoroutine(0); //Main menu = scene 0
    }

    public void ReturnToHub()
    {
        Time.timeScale = 1f;
        //LevelHub will always appear before Level 1.
        //loadSceneCoroutine = StartCoroutine(LoadScene(levelListSO.levelStartingIndex - 1));
        LaunchLoadSceneCoroutine(levelListSO.levelStartingIndex - 1);
    }

    public void SetPause(bool pause, bool fromAction = true) //Works only if a Gameplay scene is open!
    {
        if (pause)
        {
            if(fromAction //In gameplay mode, the user pressed the pause button
               && Time.timeScale > 0f //Prevents a bug when device gets disconnected in the display scene while
            )                         //in pause sub-menus or during gameplay freeze moments (UI interaction).
                UIManager.Instance.ShowPauseUI(true);

            Time.timeScale = 0f;
            UIManager.Instance.RequestInteraction(true);
            PlayerManager.Instance.RequestInteraction(false);
        }
        else
        {
            if(fromAction) //In gameplay mode, the user pressed the pause button
                UIManager.Instance.ShowPauseUI(false);

            Time.timeScale = 1f;
            UIManager.Instance.RequestInteraction(false);
            PlayerManager.Instance.RequestInteraction(true);
        }
    }

    public void GameStarted(GameData.GameLevel level)
    {
        gameMemory.Levels[level.Level - 1] = level;

        if(gameMemory.HasContinueSave && gameMemory.ContinueIndex == level.Level - 1)
        {
            GameResetCheckPoint();
        }

        gameMemory.TotalAttempts++;

        SaveMemory(); //Auto-save process
    }

    public void GameContinued(GameData.GameLevel level, float time)
    {
        gameMemory.Levels[level.Level - 1] = level;
        gameMemory.ContinueTime = time;
        gameMemory.TotalAttempts++;

        SaveMemory(); //Auto-save process
    }

    public void GameCheckPointReached(int level, float time)
    {
        gameMemory.ContinueIndex = level-1;
        gameMemory.ContinueTime = time;
        gameMemory.HasContinueSave = true;

        SaveMemory(); //Auto-save process
    }

    public void GameResetCheckPoint()
    {
        gameMemory.HasContinueSave = false;
        gameMemory.ContinueIndex = -1;
        gameMemory.ContinueTime = 0f;
        /*
         * Note: There is no auto-save process here because every call to this function is followed by
         * a call to a function that causes the auto-save.
         */
    }

    public void GameLevelCompleted(GameData.GameLevel level)
    {
        gameMemory.Levels[level.Level - 1] = level;

        if(gameMemory.UnlockedLevel == level.Level && level.Level<GetTotalLevels())
        {
            gameMemory.UnlockedLevel++;
            gameMemory.Levels.Add(GameData.GameLevel.ToGameLevel(GetRawLevel(gameMemory.UnlockedLevel)));
        }

        if(gameMemory.HasContinueSave && gameMemory.ContinueIndex == level.Level-1)
        {
            GameResetCheckPoint();
        }
        gameMemory.TotalWins++;

        SaveMemory(); //Auto-save process
    }

    public void GameOver(GameData.GameLevel level)
    {
        level.GameOver();
        gameMemory.Levels[level.Level - 1] = level;
        SaveMemory(); //Auto-save process
    }

    //Functions
    private void ManageEventSubscription(bool subscribe)
    {
        if(subscribe)
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
        }
        else
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }

    //Event handlers
    //Event handlers: SceneManager (Unity)
    private void OnSceneChanged(Scene current, Scene next)
    {
        GetCurrentScene();
        OnSceneUpdated?.Invoke();
    }

    //Coroutines
    private IEnumerator LoadScene(int index)
    {
        UIProgressBar progressBar = UIManager.Instance.ShowLoadingScreenUI(true).GetProgressBar();

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(index);
        
        while (!asyncOperation.isDone)
        {
            progressBar.UpdateFill(asyncOperation.progress);
            yield return null;
        }
        
        loadSceneCoroutine = null;
    }

    private void LoadSceneCoroutineDispose()
    {
        if (loadSceneCoroutine != null)
        {
            StopCoroutine(loadSceneCoroutine);
            loadSceneCoroutine = null;
        }
    }

    private void LaunchLoadSceneCoroutine(int index)
    {
        LoadSceneCoroutineDispose();
        loadSceneCoroutine = StartCoroutine(LoadScene(index));
    }
}
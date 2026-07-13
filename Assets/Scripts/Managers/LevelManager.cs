/*
 * Manages the lifecycle of the Level:
 * - Starting the level
 * - Handling the timer
 * - Checking objectives
 * - Enabling/disabling destinations
 * - Managing the load/save state of the level (checkpoints)
 * - Providing access to essential resources for gameplay scenes
 * 
 * Note: 
 * The load/save state of the level is not fully managed by this script.
 * The manager only initiates the load/save process by making requests to the LevelState script, 
 * which is attached to the same GameObject as the manager.
 * The LevelState script references all GameObjects with a state and handles the load/save process. 
 * For more details, see LevelState.cs in the folder "Assets/Scripts/GameData/GameLevel".
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour, IStateSaveable
{
    private static LevelManager _instance;

    //Level Configuration
    [SerializeField] private List<LevelObjective> objectives;
    [SerializeField] private Transform spawnpoint;
    [SerializeField] private CheckPoint checkpoint;
    [SerializeField] private DestinationPoint destination;
    [SerializeField] private Transform topLeft;      //Related to the minimap
    [SerializeField] private Transform bottomRight;  //Related to the minimap
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraController camController; //Related to MainCamera
    [SerializeField] private QuestItem[] itemToCollect; //Related to the Objective: CollectItem

    //Level Data (See LevelSO, LevelListSO for more information)
    [SerializeField] private GameData.GameLevel level;  //Current part of save data associated with the level
    [SerializeField] private GameData.LevelSO rawLevel; //Raw level data (e.g., end time, default records, etc.)
    private bool hasContinue = false; //Related to the checkpoint
    private float continueTime = -1f; //Related to the checkpoint
    private float timer = 0f;
    private Coroutine timerCoroutine = null;
    private Coroutine gameOverCoroutine = null;
    private bool gameOver = false;

    private UIGameplay gameplayUI; //UI for gameplay (e.g., timer, skill bar, quest bar, minimap, etc.)

    [SerializeField] private AudioSource audioSource; //AudioSource used to play the timer-ending noise

    public enum LevelObjective
    {
        ReachEnd,
        CollectItem
    }

    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<LevelManager>(true);
                if (_instance == null)
                {
                    Debug.LogWarning("No LevelManager available in the scene!");
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
            if(audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    public Camera MainCamera { get { return mainCamera; } }
    public CameraController CamController { get  { return camController; } }

    public Vector2 TopLeft { get { return topLeft == null? Vector2.zero : new Vector2(topLeft.position.x,topLeft.position.z); } }
    public Vector2 BottomRight { get { return bottomRight == null ? Vector2.zero : new Vector2(bottomRight.position.x, bottomRight.position.z); } }

    private void Start()
    {
        level = GameManager.Instance.GetCurrentLevel();
        rawLevel = GameManager.Instance.GetCurrentRawLevel();

        //if(rawLevel.levelType == GameData.LevelSO.LevelType.LevelGameplay)
        //{
        gameplayUI = UIManager.Instance.GetUIGameplay();

        if (GameManager.Instance.IsContinueLevel())
        {
            continueTime = GameManager.Instance.Memory.ContinueTime;
            ContinueLevel();
        }
        else
            StartLevel();
        //}
    }

    /*
     *  DEFAULT LEVEL TYPE (LevelGameplay)
     */

    private void Init(bool fromContinue)
    {
        if(fromContinue)
            gameObject.transform.GetComponent<GameData.LevelState>().LoadState();
        else
            PlayerManager.Instance.Spawn(spawnpoint);

        if (checkpoint != null)
            checkpoint.gameObject.SetActive(!fromContinue);

        gameplayUI.UpdateQuestItemState(itemToCollect);
        destination.EnableDestination(CheckObjectives());

        hasContinue = fromContinue;
        continueTime = fromContinue ? continueTime + 1f : 0f;
        //Each time we continue from the checkpoint, 1 second is added to the elapsed time as a penalty.
        //For more details, see the CheckPointReached method.
        gameplayUI.Timer.text = continueTime.ToTimeString();

        level.Attemps++;
    }

    private void StartLevel()
    {
        Init(false);
        GameManager.Instance.GameStarted(level);
        OpenLevelInfo();
    }

    public void ContinueLevel()
    {
        Init(true);
        GameManager.Instance.GameContinued(level, continueTime);
        OpenLevelInfo();
    }

    public void RestartLevelWithContinue()
    {
        GameManager.Instance.ContinueLevel(level.Level);
    }

    private void OpenLevelInfo()
    {
        //If the "Safety Check (1)" in PlayerManager.LoadState occurs, this prevents bug generation!
        if (gameOver) 
            return;
        
        UILevelInfo levelInfo = UIManager.Instance.GetUILevelInfo();
        levelInfo.Show(true,level.Level,rawLevel.locKey,rawLevel.GetLocalizationParams());
        GameManager.Instance.SetPause(true, false);
    }

    public void LevelInfoClosed()
    {
        //If the "Safety Check (1)" in PlayerManager.LoadState occurs after OpenLevelInfo, this prevents
        //bug generation
        if (gameOver)
            return;

        gameplayUI.ShowMinimap(true);

        TimerCoroutineDispose();
        timerCoroutine = StartCoroutine(TimerCoroutine(continueTime));
    }

    public void CheckPointReached()
    {
        hasContinue = true;
        /*
         * At the start of each level from a checkpoint, 1 second is added to the timer as a penalty, 
         * except for the first instance, where the timer remains the same as when the player reached the 
         * checkpoint.
         * 
         * To account for this, we save `timer - 1f` because, in the Init method (which handles the starting 
         * point of the timer, `continueTime`), the penalty will be applied.
         */
        continueTime = timer - 1f;

        gameObject.transform.GetComponent<GameData.LevelState>().SaveState();
        GameManager.Instance.GameCheckPointReached(level.Level, continueTime);
    }

    private void SetGameOver()
    {
        GameManager.Instance.SetPause(true, false);
        UIManager.Instance.ShowGameOverUI(true, hasContinue);
        SoundManager.Instance.PlayGameOver();
    }

    public void GameOver(bool withDelay, float gameOverDelay = 2f)
    {
        //Prevents this function from being executed multiple times if the delayed GameOver detects
        //GameOver multiple times
        if (gameOver) 
            return;
        
        gameOver = true;
        
        SoundManager.Instance.Stop();
        TimerCoroutineDispose(); //Stops the timer

        destination.EnableDestination(false); 
        PlayerManager.Instance.RequestInteraction(false); //Blocks player gameplay input

        //If continueTime + penalty exceeds the time limit, the player cannot use the "Continue" option
        if (hasContinue && continueTime+1 >= rawLevel.endTime)
        {
            hasContinue = false;
            continueTime = 0f;
            GameManager.Instance.GameResetCheckPoint();
        }

        GameManager.Instance.GameOver(level);

        if (withDelay)
        {
            GameOverCoroutineDispose();
            gameOverCoroutine = StartCoroutine(GameOverCoroutine(gameOverDelay));
        }
        else
            SetGameOver();
    }

    public void QuestItemCollected(Item item)
    {
        foreach (QuestItem qitem in itemToCollect) 
        {
            if(qitem.item.Equals(item))
            {
                qitem.obtained = true;
                if(CheckObjectives())
                {
                    destination.EnableDestination(true);
                }
                gameplayUI.UpdateQuestItemState(itemToCollect);
                break;
            }
        }
    }

    private bool CheckObjectives() //Except for ReachEnd
    {
        foreach(LevelObjective obj in objectives)
        {
            if(obj == LevelObjective.CollectItem)
            {
                foreach(QuestItem qitem in itemToCollect)
                {
                    if (!qitem.obtained)
                        return false;
                }
            }
        }

        return true;
    }

    public void EndReached() //GameWin
    {
        if (timer < rawLevel.endTime && CheckObjectives())
        {
            SoundManager.Instance.Stop();
            TimerCoroutineDispose(); //Stops the timer
            GameManager.Instance.SetPause(true, false);

            level.LevelCompleted(timer);

            GameManager.Instance.GameLevelCompleted(level);
            bool nextLevel = level.Level < GameManager.Instance.GetTotalLevels();

            UIGameWin gameWinUI = UIManager.Instance.ShowGameWinUI(true);
            gameWinUI.UpdateUI(level.Records, rawLevel.records, level.BestRecordIndex, timer, nextLevel, true);
            SoundManager.Instance.PlayGameWin();
        }
    }

    //Coroutine
    private IEnumerator TimerCoroutine(float startTime)
    {
        timer = startTime;

        //Time elapsed since the last sound was emitted to signal the timer is ending (final 10 seconds)
        float lastSound = 0f;
        //Determines if the timer is far from the last 10 seconds and if the first alarm sound has been emitted
        bool farFromEnding = true; 

        GameManager.Instance.SetPause(false, false);

        while (true)
        {
            if (timer < rawLevel.endTime)
            {
                //Update timer UI
                gameplayUI.Timer.text = timer.ToTimeString();//Using an ExtensionMethod (see MyExtension class)

                if (rawLevel.endTime-timer < 10f)
                {
                    if(farFromEnding) //true: Triggers the first alarm sound
                    {
                        gameplayUI.SetEndingTimer(); //Changes the color of the timer UI
                        farFromEnding = false;
                        audioSource.volume = SoundManager.Instance.GetSoundEffectsVolume();
                        audioSource.Play(); //Emits a sound to indicate that the timer is nearing its end
                    }

                    //Emits a sound every second to indicate that the timer is nearing its end
                    if (lastSound >= 1f) 
                    {
                        audioSource.volume = SoundManager.Instance.GetSoundEffectsVolume();
                        audioSource.Play();
                        lastSound = 0f;
                    }

                    lastSound += Time.deltaTime;
                }
            }
            else
            {
                break;
            }

            yield return 0;

            timer += Time.deltaTime;
        }

        timerCoroutine = null;
        GameOver(false); //When the timer ends, trigger an instant GameOver
    }

    private IEnumerator GameOverCoroutine(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        SetGameOver();
        gameOverCoroutine = null;
    }

    private void TimerCoroutineDispose()
    {
        if(timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    private void GameOverCoroutineDispose()
    {
        if(gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
            gameOverCoroutine = null;
        }
    }

    //PlayerState
    public IState SaveState()
    {
        EntityState state = new EntityState();

        state.itemsObtained = new bool[itemToCollect.Length];

        for (int i = 0; i < itemToCollect.Length; i++)
            state.itemsObtained[i] = itemToCollect[i].obtained;

        return state;
    }

    public void LoadState(IState state)
    {
        EntityState entityState = state as EntityState;
        
        for(int i = 0; i < entityState.itemsObtained.Length; i++)
        {
            itemToCollect[i].obtained = entityState.itemsObtained[i];
        }

        gameplayUI.UpdateQuestItemState(itemToCollect);
        if (CheckObjectives())
        {
            destination.EnableDestination(true);
        }
    }

    [System.Serializable]
    protected class EntityState : IState
    {
        [SerializeField] public bool[] itemsObtained;
    }
}

/*
 * Manages the BGM for scenes and provides methods to retrieve the appropriate volume value.
 * Also allows the instantiation of temporary SFX.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    //Sound
    private AudioSource audioSource = null; //2D AudioSource for BGM tracks
    [SerializeField] private List<AudioClip> listBGM = null; //Playlist BGM tracks
    [SerializeField] private AudioClip gameOverBGM = null;
    [SerializeField] private AudioClip gameWinBGM = null;
    //Currently null in the prefab too, as I couldn't find a suitable BGM for the level victory.
    private int indexBGM = 0;

    private Coroutine waitForEndBGM = null;

    //Settings
    private GameData.GameSound soundSettings; //A copy of the settings provided by the GameManager

    //Events
    private bool subUIGSSounds = false;
    /*
     * Used when the user accesses the General Settings.
     * This boolean informs the manager whether it is subscribed to the UISettingsGeneral.OnSoundSliderChange 
     * event.
     * With this subscription, the manager can provide feedback by temporarily adjusting the Master and 
     * Music sound levels, allowing the user to notice the changes when using the slider for the three types of 
     * volume.
     * The sound settings will be updated if the user saves; otherwise, they will revert to their previous state.
     * To detect whether the user has opened or closed the General Settings, the manager subscribes to:
     * UISettingsGeneral.OnOpenGeneralSettings and UISettingsGeneral.OnCloseGeneralSettings.
     */


    //Prefabs
    [SerializeField] private TemporarySFX temporarySFXPrefab = null;

    public static SoundManager Instance
    {
        get 
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<SoundManager>(true);
                if (_instance == null)
                {
                    Debug.LogWarning("No SoundManager available in the scene!");
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
        if(audioSource == null)
            audioSource = GetComponent<AudioSource>();

        UpdateSettings();
        ManageEventSubscription(true);
        ManageBGM();
    }

    private void OnDisable()
    {
        WaitForEndBGMDispose();
        ManageEventSubscription(false);
    }

    //Settings
    private void UpdateSettings()
    {
        soundSettings = GameManager.Instance.Settings.Sound;
    }

    //Functions
    private void ManageEventSubscription(bool subscribe)
    {
        if (subscribe)
        {
            GameManager.OnGeneralSettingsUpdate += OnUpdateSettings;
            GameManager.OnSceneUpdated += OnSceneChanged;
            UISettingsGeneral.OnOpenGeneralSettings += OnStartListenSettingsUI;
            UISettingsGeneral.OnCloseGeneralSettings += OnStopListenSettingsUI;
        }
        else
        {
            GameManager.OnGeneralSettingsUpdate -= OnUpdateSettings;
            GameManager.OnSceneUpdated -= OnSceneChanged;
            UISettingsGeneral.OnOpenGeneralSettings -= OnStartListenSettingsUI;
            UISettingsGeneral.OnCloseGeneralSettings -= OnStopListenSettingsUI;
            if (subUIGSSounds)
            {
                UISettingsGeneral.OnSoundSliderChange -= OnUpdateTempSounds;
                subUIGSSounds = false;
            }
        }
    }

    public float GetMusicVolume()
    {
        return soundSettings.MasterSound * soundSettings.Music;
    }

    public float GetSoundEffectsVolume()
    {
        return soundSettings.MasterSound * soundSettings.SoundEffects;
    }

    private void UpdateSoundVolume(GameData.GameSound? sounds)
    {
        if (sounds != null) //Used for temporary changes (e.g., UISettingsGeneral)
        {
            audioSource.volume = sounds.Value.MasterSound * sounds.Value.Music;
        }
        else //Use sound settings provided by the GameManager
        {
            audioSource.volume = soundSettings.MasterSound * soundSettings.Music;
        }
    }

    public void GenerateTempSFX(Vector3 position, AudioClip clip, bool _3d = true, float minDistance = 1f, 
        float maxDistance = 10f, AudioRolloffMode rolloffMode = AudioRolloffMode.Linear
    )
    {
        TemporarySFX sfx = Instantiate<TemporarySFX>(temporarySFXPrefab, position, Quaternion.identity);
        sfx.PlaySFX(clip,_3d,minDistance,maxDistance,rolloffMode);
    }

    private void ManageBGM()
    {
        WaitForEndBGMDispose();

        //Retrieve the BGM track list for the current scene
        //(refer to GameManager, LevelSO, and LevelListSO for details)
        listBGM = GameManager.Instance.GetCurrentLevelBGMs();

        if (listBGM == null || listBGM.Count == 0)
        {
            return;
        }
        else if (listBGM.Count == 1) //Play a single track in a continuous loop
        {
            StartBGM(0, true, false);
        }
        else //Play the entire playlist on a continuous loop
        {
            StartBGM(indexBGM, false, true);
        }
    }


    /*
     * Plays a BGM track from the list.
     * Supports three types of play modes:
     * 1) Plays a single BGM from the list, determined by the index, in loop mode.
     *    Params: BGM's index, loop = true, multiclip is ignored.
     * 2) Plays a single BGM from the list without loop mode (no other clips will be played after it ends).
     *    Params: BGM's index, loop = false, multiclip = false.
     * 3) Plays the entire playlist in loop mode, starting from the clip specified by the index.
     *    After the last BGM in the list finishes, the first BGM in the list will be played.
     *    Params: BGM's index, loop = false, multiclip = true.
     *    Note: The start of the next BGM is handled by another method, which calls this method again!
     */

    private void StartBGM(int index, bool loop, bool multiClip) 
    {
        if (index < 0 || index >= listBGM.Count) //Error
        {
            Debug.LogError("SoundManager.StartBGM: Index out of bounds | Index: " + index);
            return;
        }

        audioSource.Stop(); //Stop the previous BGM
        audioSource.clip = listBGM[index];
        audioSource.volume = GetMusicVolume();

        if (loop)
        {
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (multiClip) //Play playlist
        {
            indexBGM = index;
            audioSource.loop = false;
            audioSource.Play();
            WaitForEndBGMDispose();
            waitForEndBGM = StartCoroutine(WaitForEndBGM(audioSource.clip.length));
        }
        else //No looping and no additional clip
        {
            audioSource.loop = false;
            audioSource.Play();
        }
    }

    //Overload
    //Play a generic audio clip as BGM, with the option to enable looping
    private void StartBGM(AudioClip clip, bool loop) 
    {
        if (clip == null)
            return;

        Stop();
        audioSource.volume = GetMusicVolume();
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();
    }

    public void Stop()
    {
        WaitForEndBGMDispose();
        audioSource.Stop();
    }

    public void PlayPlaylist(bool restart)
    {
        if(restart)
            indexBGM = 0;

        ManageBGM();
    }

    public void PlayGameOver()
    {
        StartBGM(gameOverBGM, false);
    }

    public void PlayGameWin()
    {
        StartBGM(gameWinBGM, false);
    }

    //Event Handlers
    //Event Handlers: SoundManager
    private void OnBGMEnded() //Refers to the playlist
    {
        if (indexBGM == listBGM.Count - 1)
            StartBGM(0, false, true);
        else
            StartBGM(indexBGM + 1, false, true);
    }

    //Event Handlers: GameManager
    private void OnUpdateSettings()
    {
        UpdateSettings();
        UpdateSoundVolume(null); //Use sounds settings
    }

    private void OnSceneChanged()
    {
        indexBGM = 0;
        ManageBGM();
    }

    //Event Handlers: UISettingsGeneral
    private void OnStartListenSettingsUI()
    {
        UISettingsGeneral.OnSoundSliderChange += OnUpdateTempSounds;
        subUIGSSounds = true;
    }

    //Necessary to restore previous settings if the user closes without saving
    private void OnStopListenSettingsUI()
    {
        UISettingsGeneral.OnSoundSliderChange -= OnUpdateTempSounds;
        subUIGSSounds = false;
        UpdateSoundVolume(null); //Use sound settings
    }

    private void OnUpdateTempSounds(GameData.GameSound sounds)
    {
        UpdateSoundVolume(sounds); //Use temporary sounds settings from UISettingsGeneral
    }

    //Coroutines
    /*
     * This coroutine invokes the OnBGMEnd event after waiting for "time" seconds, 
     * where "time" represents the duration of the BGM.
     * 
     * This solution works but has some limitations:
     * - The timer must start at the beginning of the audio clip. If the audio clip's start time is delayed, 
     *   this method will stop the BGM before it ends.
     * - If any changes are made to the BGM that alter the duration of the audio clip, 
     *   the correct updated duration must be provided as "time". 
     *   In this case, audioSource.clip.length cannot be used as a parameter because it will only provide 
     *   the base duration without accounting for changes!
     */

    private IEnumerator WaitForEndBGM(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        waitForEndBGM = null;
        OnBGMEnded();
    }

    private void WaitForEndBGMDispose()
    {
        if(waitForEndBGM != null)
        {
            StopCoroutine(waitForEndBGM);
            waitForEndBGM = null;
        }
    }
}

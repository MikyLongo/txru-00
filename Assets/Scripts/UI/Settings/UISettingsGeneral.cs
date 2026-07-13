/*
 * Script responsible for managing the UI of the submenu dedicated to general settings.
 * 
 * Features:
 * - Includes a button to restore default settings.
 * - If the user modifies settings and tries to leave the menu without saving, displays a prompt to confirm
 *   whether to save the changes before exiting.
 */
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISettingsGeneral : MonoBehaviour, IPageUI
{
    [SerializeField] private UISettings parent;
    [SerializeField] private TMP_Dropdown ddLanguage;
    [SerializeField] private TMP_Dropdown ddResolution;
    [SerializeField] private Toggle tgFullscreen;
    [SerializeField] private Toggle tgShowFPS;
    [SerializeField] private Toggle tgVSync;
    [SerializeField] private Slider sMasterSound;
    [SerializeField] private TMP_Text tMasterSound;
    [SerializeField] private Slider sSoundEffects;
    [SerializeField] private TMP_Text tSoundEffects;
    [SerializeField] private Slider sMusic;
    [SerializeField] private TMP_Text tMusic;
    [SerializeField] private Button bSave;
    [SerializeField] private Button bReset;
    [SerializeField] private Button bExit;

    private bool initialized = false;
    private bool modified = false;

    private GameData.GameSound sounds;

    //Events
    public static event Action OnOpenGeneralSettings;   
    public static event Action OnCloseGeneralSettings;
    public static event Action<GameData.GameSound> OnSoundSliderChange;
    /*
     * Triggers events to notify the opening and closing of the general settings menu, as well as changes 
     * to the sound settings.
     * The SoundManager listens to these events to provide feedback to the user when sound settings are modified.
     */

    private void OnEnable()
    {
        UIManager.Instance.OpenedPage = this;

        //Avoid redundant Init() calls if the UI has already been initialized and remains unmodified
        if ((!initialized) || (initialized && modified))
            Init();
        ddLanguage.onValueChanged.AddListener((x) => OnLangaugeDDValueChanged());
        ddResolution.onValueChanged.AddListener((x) => OnResolutionDDValueChanged());
        tgFullscreen.onValueChanged.AddListener((x) => OnFullScreenSwitch());
        tgShowFPS.onValueChanged.AddListener((x) => OnShowFPSSwitch());
        tgVSync.onValueChanged.AddListener((x) => OnVSyncSwitch());
        sMasterSound.onValueChanged.AddListener((x) => OnMasterSoundChange());
        sSoundEffects.onValueChanged.AddListener((x) => OnSoundEffectsChange());
        sMusic.onValueChanged.AddListener((x) => OnMusicChange());
        bSave.onClick.AddListener(OnSave);
        bReset.onClick.AddListener(OnReset);
        bExit.onClick.AddListener(OnExit);
        OnOpenGeneralSettings?.Invoke();
    }

    private void OnDisable()
    {
        ddLanguage.onValueChanged.RemoveAllListeners();
        ddResolution.onValueChanged.RemoveAllListeners();
        tgFullscreen.onValueChanged.RemoveAllListeners();
        tgShowFPS.onValueChanged.RemoveAllListeners();
        tgVSync.onValueChanged.RemoveAllListeners();
        sMasterSound.onValueChanged.RemoveAllListeners();
        sSoundEffects.onValueChanged.RemoveAllListeners();
        sMusic.onValueChanged.RemoveAllListeners();
        bSave.onClick.RemoveAllListeners();
        bReset.onClick.RemoveAllListeners();
        bExit.onClick.RemoveAllListeners();
        OnCloseGeneralSettings?.Invoke();
    }

    private void Init()
    {
        //Languages
        if (ddLanguage.options.Count == 0) //If not filled
        {
            List<string> languages = new List<string>();
            int selectedLang = LocalizationHelper.GetLocalizationLanguages(ref languages);

            ddLanguage.AddOptions(languages);
            ddLanguage.value = selectedLang;
        }
        else //If filled
        {
            int langIndex = LocalizationHelper.GetCurrentLanguageIndex();
            if (langIndex < 0)
            {
                Debug.LogError("UISettingsGeneral-Error (Init): Failed to retrieve the index of the current language!");
                ddLanguage.value = 0;
            }
            else
                ddLanguage.value = langIndex;
        }

        //Resolutions
        if(ddResolution.options.Count == 0) //If not filled
        {
            ddResolution.AddOptions(GameData.GameResolution.GetAvailableResolutionsAsStrings());
        }

        int resIndex = GameData.GameResolution.GetResolutionIndex(GameManager.Instance.Settings.Resolution);
        if (resIndex < 0)
        {
            Debug.LogError("UISettingsGeneral-Error (Init): The current resolution is not listed among the available resolutions!");
            ddResolution.value = 0;
        }
        else
            ddResolution.value = resIndex;

        //Sound
        sounds = GameManager.Instance.Settings.Sound;

        tMasterSound.text = ConvertVolume(sounds.MasterSound, false).ToString();
        sMasterSound.value = ConvertVolume(sounds.MasterSound, false);
        tSoundEffects.text = ConvertVolume(sounds.SoundEffects, false).ToString();
        sSoundEffects.value = ConvertVolume(sounds.SoundEffects, false);
        tMusic.text = ConvertVolume(sounds.Music, false).ToString();
        sMusic.value = ConvertVolume(sounds.Music, false);
        //CheckBox
        tgFullscreen.isOn = GameManager.Instance.Settings.FullScreen;
        tgShowFPS.isOn = GameManager.Instance.Settings.ShowFPS;
        tgVSync.isOn = GameManager.Instance.Settings.VSync;

        initialized = true;
        modified = false;
        SetSaveInteractable(false);
    }

    /*
     * Adjusts the scale of the float value for sound management.
     * Parameters:
     * - toSound:
     *   - true:  Converts the value to the range [0,1], suitable for AudioSource.
     *   - false: Converts the value to the range [0,100], suitable for the user interface.
     */
    private float ConvertVolume(float value, bool toSound)
    {
        if (toSound) //[0,1]
            return value / 100;
        else //[0-100]
            return value * 100;
    }

    public void OnLangaugeDDValueChanged()
    {
        SetModified();
    }

    public void OnResolutionDDValueChanged()
    {
        SetModified();
    }

    public void OnFullScreenSwitch()
    {
        SetModified();
    }

    public void OnShowFPSSwitch()
    {
        SetModified();
    }

    public void OnVSyncSwitch()
    {
        SetModified();
    }

    public void OnMasterSoundChange()
    {
        SetModified();
        tMasterSound.text = sMasterSound.value.ToString();
        sounds.MasterSound = ConvertVolume(sMasterSound.value, true);
        OnSoundSliderChange?.Invoke(sounds);
    }
    public void OnSoundEffectsChange()
    {
        SetModified();
        tSoundEffects.text = sSoundEffects.value.ToString();
        sounds.SoundEffects = ConvertVolume(sSoundEffects.value, true);
        OnSoundSliderChange?.Invoke(sounds);
    }

    public void OnMusicChange()
    {
        SetModified();
        tMusic.text = sMusic.value.ToString();
        sounds.Music = ConvertVolume(sMusic.value, true);
        OnSoundSliderChange?.Invoke(sounds);
    }

    public void OnReset()
    {
        //modified = true ---> Redundant (executed by other listeners when changes occur)
        GameData.GameSetting settings = GameData.GameSetting.GetDefaultSettings();

        int index = LocalizationHelper.GetDefaultLanguageIndex();
        if (index < 0)
        {
            Debug.LogError("UISettingsGeneral-Error (Reset): Failed to retrieve the index of the default language!");
            ddLanguage.value = 0;
        }
        else
            ddLanguage.value = index;

        index = GameData.GameResolution.GetResolutionIndex(settings.Resolution);
        if (index < 0)
        {
            Debug.LogError("UISettingsGeneral-Error (Reset): Failed to retrieve the default resolution value!");
            ddResolution.value = 0;
        }
        else
            ddResolution.value = index;

        tMasterSound.text = ConvertVolume(settings.Sound.MasterSound, false).ToString();
        sMasterSound.value = ConvertVolume(settings.Sound.MasterSound, false);
        tSoundEffects.text = ConvertVolume(settings.Sound.SoundEffects, false).ToString();
        sSoundEffects.value = ConvertVolume(settings.Sound.SoundEffects, false);
        tMusic.text = ConvertVolume(settings.Sound.Music, false).ToString();
        sMusic.value = ConvertVolume(settings.Sound.Music, false);
        tgFullscreen.isOn = settings.FullScreen;
        tgShowFPS.isOn = settings.ShowFPS;
        tgVSync.isOn = settings.VSync;
    }

    private void SetModified()
    {
        if(initialized)
        {
            modified = true;
            if (!bSave.interactable)
                SetSaveInteractable(true);
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

    public void OnExit()
    {
        if(modified) //If there are unsaved changes, prompt the player with a message box asking 
        {            //whether to save
            UIMessageBox mBox = UIManager.Instance.GetUIMessageBox();
            mBox.Open(
                LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, UIMessageBox.LK_CHANGE_N_SAVED_T),
                LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, UIMessageBox.LK_CHANGE_N_SAVED_AC),
                LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, LocalizationHelper.LK_SAVE),
                () => { OnSave(); Close(); },
                true,
                () => { Close(); },
                0,
                UIMessageBox.MsgType.Warning,
                true
            );
        }
        else
            Close();
    }

    private void Close()
    {
        parent.ShowGeneralSettings(false);
    }

    public void OnSave()
    {   
        UIManager.Instance.EventSystemHandler.UpdateSelection(bExit.gameObject);
        if (modified)
        {
            UIMessageBox mBox = UIManager.Instance.GetUIMessageBox();

            GameData.GameResolution resolution = GameData.GameResolution.GetAvailableResolutionAt(ddResolution.value);
            UnityEngine.Localization.Locale locCode = LocalizationHelper.GetLanguageByIndex(ddLanguage.value);

            if (resolution.IsUndefined() || locCode == null)
            {
                mBox.Open(
                    LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, LocalizationHelper.LK_ERROR),
                    LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, UIMessageBox.LK_ERROR_OCCURRED),
                    null,
                    null,
                    true,
                    null,
                    3f,
                    UIMessageBox.MsgType.Error,
                    false
                );
                Close();
                return;
            }

            GameData.GameSetting settings = new GameData.GameSetting(
                resolution,
                sounds,
                tgFullscreen.isOn,
                tgShowFPS.isOn,
                tgVSync.isOn,
                locCode.Identifier
            );

            GameManager.Instance.SaveGeneralSettings(settings);

            modified = false; //Avoid redundant calls to Init()
            SetSaveInteractable(false);

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
    }

    //Implementation of the IPageUI interface
    public void OnPausePage()
    {
        OnExit();
    }

    public GameObject GetFirstSelected()
    {
        return ddLanguage.gameObject;
    }
}

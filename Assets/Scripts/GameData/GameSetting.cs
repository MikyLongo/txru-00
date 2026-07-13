/*
 * Struct that defines the general setttings.
 * Note: Input is handled by the InputSystem package and the static class GameInput.
 */

using UnityEngine;
using UnityEngine.Localization;

namespace GameData
{
    [System.Serializable]
    public struct GameSetting
    {
        [SerializeField] private GameResolution resolution;
        [SerializeField] private GameSound sound;
        [SerializeField] private bool fullScreen;
        [SerializeField] private bool showFPS;
        [SerializeField] private bool vsync;
        [SerializeField] private LocaleIdentifier localizationID;

        public GameSetting(GameResolution resolution, GameSound sound, bool fullScreen, bool showFPS, bool vsync, LocaleIdentifier localizationID)
        {
            this.resolution = resolution;
            this.sound = sound;
            this.fullScreen = fullScreen;
            this.showFPS = showFPS;
            this.vsync = vsync;
            this.localizationID = localizationID;
        }

        public GameResolution Resolution { get { return resolution; } set { resolution = value; } }
        public GameSound Sound { get { return sound; } set { sound = value; } }
        public bool FullScreen { get { return fullScreen; } set { fullScreen = value; } }
        public bool ShowFPS { get { return showFPS; } set { showFPS = value; } }
        public bool VSync { get { return vsync; } set { vsync = value; } }
        public LocaleIdentifier LocalizationID { get { return localizationID; } set { localizationID = value; } }

        public static GameSetting GetDefaultSettings()
        {
            GameResolution resolution = GameResolution.GetDefaultResolution();
            GameSound sounds = GameSound.GetDefaultSounds();

            //LocalizationHelper.GetSystemLanguage().Identifier:
            //current system Locale ID if supported; otherwise the game's default Locale ID
            return new GameSetting(resolution, sounds, true, false, true, LocalizationHelper.GetSystemLanguage().Identifier);
        }
    }
}
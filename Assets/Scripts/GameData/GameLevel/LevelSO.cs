/*
 * Scriptable Object (SO) that defines the characteristics of a scene.
 * In the project, every actual scene of the game has a LevelSO associated to define the information, 
 * such as the list of BGM to play while in the scene.
 * Each scene is associated with a type of scene (and is called a level): the main distinction is between 
 * scenes designated for real gameplay (LevelGameplay) and scenes that are Menu-like (MainMenu, LevelHub).
 * This distinction is made to determine which information to extract for the scene and how to interact with it.
 * The major difference between Menu-like scenes and Gameplay scenes is the extra information for the 
 * Gameplay scenes, such as the level number (1, 2, 3...), the default records to beat, and the time limit.
 * Since, for the Gameplay scenes, there is data that needs to change (records), and every change to an SO is 
 * permanent to the file representing the SO itself, we use a wrapper called GameLevel (a struct).
 * GameLevel represents the updated information of a Gameplay scene (with extra data).
 * In the case of a Gameplay scene, LevelSO is the default information (and immutable) and is called 
 * "rawLevel," while GameLevel is the updated information and is called "level."
 * The script LevelListSO associates each LevelSO file with the correct scene index.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace GameData
{
    [CreateAssetMenu(menuName = "Levels/Level", fileName = "New Level", order = 1)]
    public class LevelSO : ScriptableObject
    {
        public int level;
        public List<float> records;
        public float endTime;
        public List<AudioClip> BGMs;
        public LevelType levelType;
        public string locKey;
        public List<DictionaryEntry> locInfoParams; 

        public enum LevelType
        {
            MainMenu,
            LevelHub,
            LevelGameplay,
            DemoRankLevel//A special Gameplay scene where there is no actual gameplay (shows the rank obtained).
        }

        /*
         *  Dictionary are not serializable for this reason we use a wrapper!
         */
        [System.Serializable]
        public struct DictionaryEntry
        {
            public string key;
            public string value;
            public ValueType valueType;
            public GameInput.BindingType bindingType;
        }

        public enum ValueType
        {
            DEFAULT = 0,
            INPUT = 1
        }

        //Convert the wrapper in the appropriate dictionary for localization
        public Dictionary<string, IVariable> GetLocalizationParams()
        {
            if(locInfoParams == null || locInfoParams.Count == 0)
                return null;

            Dictionary<string, IVariable> locParams = new Dictionary<string, IVariable>();
            string value = null;

            foreach (DictionaryEntry entry in locInfoParams)
            {
                if (string.IsNullOrEmpty(entry.key)) //Don't add entry with empty key
                    continue;

                switch(entry.valueType)
                {
                    case ValueType.INPUT:
                        value = GameInput.PrintBinding(entry.bindingType);
                    break;

                    default:
                        value = entry.value;
                    break;
                }

                locParams.Add(
                    entry.key,
                    new StringVariable() { Value = value }
                );
            }

            return locParams;
        }
    }
}

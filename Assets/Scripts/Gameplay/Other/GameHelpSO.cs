/*
 * Scriptable Object (SO) that defines a list of help/tutorial elements visible on the help screen
 * in the pause menu.
 * Each helper element is linked to a level required to unlock the help (progressive tutorial).
 * Refer to the UIGameHelp class to see how the SO is utilized.
 */
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newGameHelp", menuName = "GameplayHelpers/GameHelp")]
public class GameHelpSO : ScriptableObject
{
    public List<LevelHelpers> helpers = new List<LevelHelpers>();

    [System.Serializable]
    public struct LevelHelpers
    {
        public int unlockAtLevel;
        public HelperElement[] helperElements;
    }

    [System.Serializable]
    public struct HelperElement
    {
        public string titleKey;
        public string descriptionKey;
        public Sprite sprite;
    }
}

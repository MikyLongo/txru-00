/*
 *  Scriptable Object that associates the correct scene index to each LevelSO created.
 *  Current configuration:
 *  Scene Index | Level
 *      0       | Main Menu [Not a gameplay level]
 *      1       | Level Hub 
 *      2       | Level 1  => levelStartingIndex
 *      3       | Level 2 
 *     ...
 *  
 *  levelStartingIndex is the index of the first Gameplay scene.
 *  Rules to follow:
 *  - Main Menu must be the first scene.
 *  - Level Hub must come before the first Gameplay scene.
 *  - Gameplay scenes must be placed consecutively (starting from levelStartingIndex).
 *  - DemoRankLevel is treated as a Gameplay scene and can be placed starting from levelStartingIndex.
 *  Note: For now, DemoRankLevel is the last level (and the last scene).
 */

using System.Collections.Generic;
using UnityEngine;

namespace GameData
{
    [CreateAssetMenu(menuName = "Levels/LevelList", fileName = "New Level List", order = 2)]
    public class LevelListSO : ScriptableObject
    {
        public List<LevelSO> list;
        public int levelStartingIndex; 
    }
}

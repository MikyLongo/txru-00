/*
 * Struct that defines the game save data.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace GameData
{
    [System.Serializable]
    public struct GameMemory
    {
        [SerializeField] private int unlockedLevel;
        [SerializeField] private bool hasContinueSave;
        [SerializeField] private int continueIndex;
        [SerializeField] private float continueTime;
        [SerializeField] private int totalAttempts;
        [SerializeField] private int totalWins;
        [SerializeField] private string lastSave;
        [SerializeField] private List<GameLevel> levels;

        public GameMemory(
            int unlockedLevel, bool hasContinueSave, int continueIndex,float continueTime, 
            int totalAttempts, int totalWins, string lastSave, List<GameLevel> levels
            )
        {
            this.unlockedLevel = unlockedLevel;
            this.hasContinueSave = hasContinueSave;
            this.continueIndex = continueIndex;
            this.continueTime = continueTime;
            this.totalAttempts = totalAttempts;
            this.totalWins = totalWins;
            this.lastSave = lastSave;
            this.levels = levels;
        }

        public int UnlockedLevel { get { return unlockedLevel; } set { unlockedLevel = value; } }
        public bool HasContinueSave { get {  return hasContinueSave; } set {  hasContinueSave = value; } }
        public int ContinueIndex { get { return continueIndex; } set { continueIndex = value; } }
        public float ContinueTime { get {  return continueTime; } set {  continueTime = value; } }
        public int TotalAttempts { get { return totalAttempts; } set { totalAttempts = value; } }
        public int TotalWins { get { return totalWins; } set { totalWins = value; } }
        public string LastSave { get { return lastSave; } set { lastSave = value; } }
        public List<GameLevel> Levels { get { return levels; } set { levels = value; } }

        public static GameMemory GetDefaultMemory() //New Game Run
        {
            return new GameMemory(1,false,-1,0,0,0,System.DateTime.Now.ToString("o"),null);
        }

        public string CalcualteDemoRank()
        {
            if(Levels == null || Levels.Count == 0 || UnlockedLevel < 6)
                return "";

            float avg = 0f;
            int[] tops = new int[5] { 0, 0, 0, 0, 0 }; //1°, 2°, 3°, Win without a record, Game Over

            for(int i=0; i<5; i++)
            {
                int pos = Levels[i].FirstAttemptPos;

                avg += pos;

                tops[pos - 1]++;
            }

            avg /= 5;

            if (avg == 1f) //All 1°
                return "S";
            else
            {
                //All 1° with a maximum of one 2° and one 3°
                if (
                    (tops[1] < 2 && tops[2] < 2 && tops[3] == 0 && tops[4] == 0) || //All 1° and 1 top 2° and 1 top 3°
                    (tops[1] < 3 && tops[2] == 0 && tops[3] == 0 && tops[4] == 0)   //All 1° and 2 top 2°
                )
                {
                    return "A";
                }
                else if (avg <= 2f && tops[4] == 0) // Average <= 2 and no Game Over
                {
                    return "B";
                }
                else if (avg <= 3f)
                {
                    return "C";
                }
                else if (avg <= 4f)
                {
                    return "D";
                }
                else if (avg < 5f)
                {
                    return "E";
                }
                else
                {
                    return "F";
                }
            }
        }

        public PartialMemory GetPartialMemory()
        {
            PartialMemory partialMemory = new PartialMemory(UnlockedLevel, HasContinueSave, TotalAttempts, TotalWins, LastSave);
            partialMemory.rank = CalcualteDemoRank();
            return partialMemory;
        }

        public override string ToString()
        {
            return $"unlockedLevel = {unlockedLevel}\n" +
                   $"hasContinueSave = {hasContinueSave}\n" +
                   $"continueIndex = {continueIndex}\n" + 
                   $"continueTime = {continueTime}\n" +
                   $"totalAttempts = {totalAttempts}\n" +
                   $"totalWins = {totalWins}\n" +
                   $"lastSave = {lastSave}\n" +
                   $"levels = {((levels == null)? "null" : $"count: {levels.Count}")}";
        }

        //Struct that defines only the essential information to represent the characteristics of the game
        //save file.
        [System.Serializable]
        public struct PartialMemory
        {
            public int numSlot;
            public int unlockedLevel;
            public bool hasContinueSave;
            public int totalAttempts;
            public int totalWins;
            public string lastSave;
            public string rank;

            public PartialMemory(int unlockedLevel, bool hasContinueSave, int totalAttempts, int totalWins, string lastSave) : this()
            {
                this.unlockedLevel = unlockedLevel;
                this.hasContinueSave = hasContinueSave;
                this.totalAttempts = totalAttempts;
                this.totalWins = totalWins;
                this.lastSave = lastSave;
            }
        }
    }
}
/*
 * A wrapper to manage Gameplay scene defined by LevelSO.
 */
using System.Collections.Generic;
using UnityEngine;

namespace GameData
{
    [System.Serializable]
    public struct GameLevel
    {
        [SerializeField] private int level;
        [SerializeField] private List<float> records;
        [SerializeField] private int bestRecordIndex; 
        [SerializeField] private int firstAttemptPos; //1,2,3,4,5 => 4° = Win (no leadboard), 5° = Game Over
        [SerializeField] private int attempts;
        [SerializeField] private int beaten;

        public GameLevel(int level, List<float> records, int bestRecordIndex, int firstAttemptIndex, int attempts, int beaten)
        {
            this.level = level;
            this.records = records;
            this.bestRecordIndex = bestRecordIndex;
            this.firstAttemptPos = firstAttemptIndex; //1,2,3,4,5 => 4° = Win (no leadboard), 5° = Game Over
            this.attempts = attempts;
            this.beaten = beaten;
        }

        public int Level { get { return level; } set { level = value; } }
        public List<float> Records { get { return records; } set { records = value; } }
        public int BestRecordIndex { get { return bestRecordIndex; } set { bestRecordIndex = value; } }
        public int FirstAttemptPos { get { return firstAttemptPos; } set { firstAttemptPos = value; } }
        public int Attemps { get { return attempts; } set { attempts = value; } }
        public int Beaten { get { return beaten; } set { beaten = value; } }
        
        public void GameOver()
        {
            if (firstAttemptPos < 0) //Is first try?
            {
                firstAttemptPos = 5;
            }
        }

        public void LevelCompleted(float timer)
        {
            beaten++;

            for (int i = 0; i < 4; i++)
            {
                if (timer < records[i])
                {
                    if(firstAttemptPos < 0) //Is first try?
                    {
                        firstAttemptPos = i + 1;
                    }

                    //Calculate if is new best record (<0 == No record)
                    if (bestRecordIndex < 0 || bestRecordIndex > 0 && i < bestRecordIndex)
                        bestRecordIndex = i;

                    //Update leaderboard
                    for (int j = records.Count - 1; j > i; j--)
                    {
                        records[j] = records[j - 1];
                    }

                    records[i] = timer;
                    break;
                }
            }
        }

        public static GameLevel ToGameLevel(LevelSO levelSO)
        {
            List<float> levelRecords = new List<float>(levelSO.records); //Make a copy of the default records

            levelRecords.Add(float.MaxValue); //Add the 4° record (MaxValue = No real record)
            return new GameLevel( //Default data
                levelSO.level,
                levelRecords,
                -1,
                -1,
                0,
                0
            );
        }

        public static List<GameLevel> ToGameLevelList(List<LevelSO> levelSOList)
        {
            List<GameLevel> levels = new List<GameLevel>();

            foreach(LevelSO level in levelSOList) 
            { 
                levels.Add(ToGameLevel(level));
            }

            return levels;
        }
    }
}

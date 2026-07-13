/*
 * Struct that defines resolution settings and provides logic to extract and manage the list of supported 
 * screen resolutions.
 * The default resolution is the current resolution used by the screen.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameData
{
    [System.Serializable]
    public struct GameResolution
    {
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private uint hzNum;
        [SerializeField] private uint hzDen;

        //List of supported screen resolutions
        private static List<GameResolution> availableResolutions = null;

        public GameResolution(int width, int height, uint hzNum, uint hzDen)
        {
            this.width = width;
            this.height = height;
            this.hzNum = hzNum;
            this.hzDen = hzDen;
        }
        
        public int Width { get { return width; } set { width = value; } }
        public int Height { get { return height; } set {  height = value; } }
        public uint HzNum { get { return hzNum; } set { hzNum = value; } }
        public uint HzDen { get { return hzDen; } set { hzDen = value; } }
       

        //Retrieve the list of resolutions supported by the screen!
        public static List<GameResolution> GetAvailableResolutions()
        {
            Init();
            return availableResolutions;
        }

        //Retrieve the list of resolutions supported by the screen as a list of strings!
        public static List<string> GetAvailableResolutionsAsStrings()
        {
            Init();
            List<string> list = new List<string>();

            foreach (GameResolution res in availableResolutions) 
            {
                list.Add(res.ToString());
            }

            return list;
        }

        public static GameResolution GetUndefinedResolution()
        {
            return new GameResolution(-1, -1, 1, 1);
        }

        public bool IsUndefined()
        {
            if (width == -1 || height == -1) 
                return true;

            return false;
        }

        //Retrieve the resolution from the list at the specified index.
        public static GameResolution GetAvailableResolutionAt(int index)
        {
            Init();

            if(index < 0 || index > availableResolutions.Count-1)
            {
                Debug.LogError("GameResolution.GetAvailableResolutionAt: Invalid index!");
                return GetUndefinedResolution(); 
            }
            
            return availableResolutions[index];
        }

        //Given a resolution, find its associated index in the list.
        public static int GetResolutionIndex(GameResolution resolution)
        {
            Init();
            return availableResolutions.FindIndex(x => x.Equals(resolution)); //-1 => Error!
        }

        //The default resolution refers to the resolution currently used by the screen.
        public static GameResolution GetDefaultResolution()
        {
            Resolution cres = Screen.currentResolution;
            return new GameResolution(cres.width, cres.height, cres.refreshRateRatio.numerator, cres.refreshRateRatio.denominator);
        }

        //Populate the list of available resolutions supported by the screen.
        private static void Init()
        {
            if (availableResolutions == null)
            {
                availableResolutions = new List<GameResolution>();
                Resolution[] screenResolutions = Screen.resolutions;
                
                foreach (Resolution res in screenResolutions)
                {
                    availableResolutions.Add(new GameResolution(res.width, res.height, res.refreshRateRatio.numerator, res.refreshRateRatio.denominator));
                }
            }

            //Debug
            foreach (GameResolution resolution in availableResolutions)
            {
                Debug.Log(resolution.ToString());
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is GameResolution res)
            {
                if (res.width == width && res.height == height && res.hzNum == hzNum && res.hzDen == hzDen)
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            RefreshRate hz = new RefreshRate
            {
                numerator = hzNum,
                denominator = hzDen
            };

            return $"{width}x{height}@{(int)hz.value}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(width, height);
        }
    }
}
/*
 * Static class that manages read and write operations for files or PlayerPrefs, with or without encryption.
 * Provides methods to read/write: General Settings, Input Settings, Save Files/Data, and their associated 
 * Level States.
 * The game supports a maximum of NUM_FILE game save files, and for each save file, there can be another file 
 * containing the state (checkpoint) of a level.
 * Each game save is identified by its slot number, so a save file with slot N and a continue/checkpoint file 
 * with slot N will refer to the same game save.
 * Each file follows this structure: {PATH}/{NAME}{SLOT}.dat
 * General settings and Input settings are stored using PlayerPrefs, whereas game saves are stored as 
 * encrypted binary files.
 */
using GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public static class GameSaver 
{
    private static readonly string K_SETTINGS = "K_SETTINGS";
    private static readonly string K_INPUTSETTINGS = "K_INPUTSETTINGS";
    private static readonly string FILENAME_GD = "SaveData";
    private static readonly string FILETYPE_GD = ".dat";
    private static readonly string FILENAME_CD = "ContinueData";
    private static readonly string FILETYPE_CD = ".dat";
    public  static readonly int    NUM_FILE = 10;


    //GENERAL SETTINGS 
    public static void SaveSettings(GameSetting settings)
    {
        string jsonText = JsonUtility.ToJson(settings); Debug.Log("SAVE\n" + jsonText);
        PlayerPrefs.SetString(K_SETTINGS, jsonText);
        PlayerPrefs.Save();
    }

    public static GameSetting LoadSettings() 
    {
        string jsonText = PlayerPrefs.GetString(K_SETTINGS); Debug.Log("LOAD\n" + jsonText);

        if(string.IsNullOrEmpty(jsonText))
            return GameSetting.GetDefaultSettings();

        return JsonUtility.FromJson<GameSetting>(jsonText);
    }

    //INPUT SETTINGS
    /*
     *  Input settings are managed by the InputSystem package.
     *  The package uses an InputActionAsset containing the default settings.
     *  To apply changes to the settings, we need to use an override. The default settings and the override will
     *  coexist, with the override taking precedence (when present). When the game is closed, the override will 
     *  be lost. 
     *  For this reason, we save the override using PlayerPrefs and load it when required.
     */
    public static void SaveInputSettings(InputActionAsset inputActions)
    {
        string jsonText = inputActions.SaveBindingOverridesAsJson(); Debug.Log("SAVEINPUT\n" + jsonText);
        PlayerPrefs.SetString(K_INPUTSETTINGS, jsonText); //Save the overrides
        PlayerPrefs.Save();
    }

    public static void LoadInputSettings(InputActionAsset inputActions) 
    {
        string jsonText = PlayerPrefs.GetString(K_INPUTSETTINGS); Debug.Log("LOADINPUT\n" + jsonText);

        if (string.IsNullOrEmpty(jsonText)) //No overrides => Default value 
        {   
            inputActions.LoadBindingOverridesFromJson(""); //No overrides
            return;
        }

        inputActions.LoadBindingOverridesFromJson(jsonText); //Load the overrides
    }

    //GAME DATA (Memory)
    //GameMemory is the struct that defines game save data
    public static bool HasSavedMemory()
    { 
        for(int i=0; i<NUM_FILE; i++)
        {
            if (File.Exists(Path.Combine(Application.persistentDataPath, FILENAME_GD+i+FILETYPE_GD)))
            {
                return true;
            }
        }

        return false;
    }

    //Used by LoadDataInfos
    private static GameMemory.PartialMemory? LoadGameInfo(string filename)
    {
        string jsonText = FromBinary(filename, true);

        if (string.IsNullOrEmpty(jsonText)) //No save data (Error)
            return null;

        GameMemory gameMemory = JsonUtility.FromJson<GameMemory>(jsonText);
        return gameMemory.GetPartialMemory();
        //return JsonUtility.FromJson<GameMemory.PartialMemory>(jsonText);
    }

    /*
     * Create a list containing only essential information for each game save 
     * (used in the New Game/Load Game menu). 
     */
    public static List<GameMemory.PartialMemory> LoadDataInfos()
    {
        List<GameMemory.PartialMemory> infos = new List<GameMemory.PartialMemory>();

        for (int i = 0; i < NUM_FILE; i++)
        {
            string filename = Path.Combine(Application.persistentDataPath, FILENAME_GD + i + FILETYPE_GD);
            if (File.Exists(filename))
            {
                GameMemory.PartialMemory? tmp = LoadGameInfo(filename);
                if (tmp != null) // null: Indicates an error, but it will be treated as no data available.
                {
                    GameMemory.PartialMemory info = tmp.Value;
                    info.numSlot = i; // Assign the appropriate slot number for the memory data!
                    infos.Add(info);
                }
            }
        }

        return infos;
    }

    public static void SaveGame(GameMemory gameMemory, int numSlot)
    {
        string filename = Path.Combine(Application.persistentDataPath, FILENAME_GD + numSlot + FILETYPE_GD);
        string jsonText = JsonUtility.ToJson(gameMemory); Debug.Log("SAVEGAME\n" + jsonText);

        ToBinary(filename, jsonText, true);
    }

    public static GameMemory? LoadGame(int numSlot)
    {
        string filename = Path.Combine(Application.persistentDataPath, FILENAME_GD + numSlot + FILETYPE_GD);
        string jsonText = FromBinary(filename, true); Debug.Log("LOADGAME\n" + jsonText);

        if (string.IsNullOrEmpty(jsonText)) //No save data
            return null;

        return JsonUtility.FromJson<GameMemory>(jsonText);
    }

    //GAME DATA (Level State) 
    //For more info see IState interface
    public static void SaveLevelState(List<IState> states, int numSlot)
    {
        string filename = Path.Combine(Application.persistentDataPath, FILENAME_CD + numSlot + FILETYPE_CD);
        LevelStateWrapper wrapper = new LevelStateWrapper(states);
        string jsonText = JsonUtility.ToJson(wrapper); Debug.Log("SAVESTATE\n" + jsonText);
        ToBinary(filename, jsonText, true);
    }

    public static List<IState> LoadLevelState(int numSlot)
    {
        string filename = Path.Combine(Application.persistentDataPath, FILENAME_CD + numSlot + FILETYPE_CD);
        string jsonText = FromBinary(filename, true); Debug.Log("LOADSTATE\n" + jsonText);

        if (string.IsNullOrEmpty(jsonText))
            return null;
        
        LevelStateWrapper wrapper = JsonUtility.FromJson<LevelStateWrapper>(jsonText);
        List<IState> states = new List<IState>();

        for(int i=0; i<wrapper.StateList.Count; i++)
        {
            states.Add(wrapper.StateList[i]);
        }
      
        return states;
    }

    /*
     * JsonUtility does not support the serialization of interface instances. 
     * To address this issue, we use this wrapper to enable polymorphic serialization.
     */
    [System.Serializable]
    private class LevelStateWrapper
    {   //[SerializeReference] Allow polymorphic serialization
        [SerializeReference] public List<IState> StateList;

        public LevelStateWrapper(List<IState> stateList)
        {
            StateList = stateList;
        }
    }

    //To/From Binary
    private static void ToBinary(string filename, string json, bool encrypt)
    {
        //To byte array
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        //Write in binary file
        using (FileStream fileStream = new FileStream(filename, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {

                if(encrypt)
                    writer.Write(Encrypt(bytes));
                else
                    writer.Write(bytes);

                //writer.Flush();
            }
        }
    }

    private static string FromBinary(string filename, bool decrypt)
    {
        if(!File.Exists(filename))
        {
            return null;
        }

        //Reading from binary file
        byte[] bytes;

        using (FileStream fileStream = new FileStream(filename, FileMode.Open))
        {
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                if(decrypt)
                {
                    byte[] encrypted = reader.ReadBytes((int)fileStream.Length);
                    bytes = Decrypt(encrypted);
                }
                else
                    bytes = reader.ReadBytes((int)fileStream.Length);
            }
        }

        //Convert byte array to string
        return Encoding.UTF8.GetString(bytes);
    }

    //Encryption/Decryption
    private static readonly string key = "TXRU-00_SECRET_KEY_32_6578642441"; //256 bits = 32 char

    public static byte[] Encrypt(byte[] plainBytes)
    {
        byte[] iv; //initialization vector
        byte[] encrypted;

        using (Aes aes = Aes.Create()) 
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.GenerateIV();   //Generate random IV
            iv = aes.IV;        //Store it, as it will be appended to the encrypted data.

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream()) //Create the stream
            {   //Create the stream that handles the encrpytion by using the MemoryStream
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainBytes);
                }
                encrypted = memoryStream.ToArray(); //Convert the stream into an array of bytes
            }
        }

        //Append the IV generated to the beginning of the encrypted data.
        byte[] result = new byte[iv.Length + encrypted.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

        return result;
    }

    public static byte[] Decrypt(byte[] cipherBytes) //cipherBytes = IV + Encrypted Data
    {
        byte[] iv = new byte[16];
        byte[] cipher = new byte[cipherBytes.Length - iv.Length];  //Encrypted Data 

        Buffer.BlockCopy(cipherBytes, 0, iv, 0, iv.Length); //Extracting IV
        Buffer.BlockCopy(cipherBytes, iv.Length, cipher, 0, cipher.Length); //Extracting Encrypted Data

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream(cipher))
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (MemoryStream resultStream = new MemoryStream())
                    {
                        cryptoStream.CopyTo(resultStream);
                        return resultStream.ToArray();
                    }
                }
            }
        }
    }
}

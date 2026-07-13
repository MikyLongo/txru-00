/*
 *  Static class serving as an interface between the Localization package and other scripts.
 *  Offers methods to manage and interact with localization functionality.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public static class LocalizationHelper 
{
    //Tables
    public static readonly string UI_MESSAGE = "UI_Message";
    public static readonly string UI_SMART= "UI_Smart";
    public static readonly string UI_LEVEL_INFO = "UI_LevelInfo";
    public static readonly string ITEM_TABLE = "ItemTable";
    public static readonly string GAME_HELPER = "GameHelper";

    //Entries (Universal)
    public static readonly string LK_ERROR = "Error";
    public static readonly string LK_EXIT = "Exit";
    public static readonly string LK_CLOSE = "Close";
    public static readonly string LK_CANCEL = "Cancel";
    public static readonly string LK_SAVE = "Save";
    public static readonly string LK_RESET = "Reset";
    public static readonly string LK_WARNING = "Warning";
    public static readonly string LK_CONFIRM = "Confirm";

    public static readonly int SYSTEM_LOCALE_SELECTOR_INDEX = 2;
    /*
     *  Handle supported languages
     */

    /* Populate the list with the names of the languages supported by the game,
     * and return the total number of supported languages.
     */
    public static int GetLocalizationLanguages(ref List<string> languages)
    {
        //Ensure the localization settings are properly initialized, blocking execution until the Task completes.
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        //Generate a list of available locale strings.
        if (languages == null)
            return -1; //Error
        else
            languages.Clear();

        int selected = 0;
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            Locale locale = LocalizationSettings.AvailableLocales.Locales[i];

            if (LocalizationSettings.SelectedLocale == locale)
                selected = i;

            languages.Add(locale.Identifier.CultureInfo != null ? locale.Identifier.CultureInfo.NativeName : locale.ToString());
        }

        return selected;
    }

    public static Locale GetLanguageByIndex(int index)
    {
        //Ensure the localization settings are properly initialized, blocking execution until the Task completes
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        if (index < 0 || index >= LocalizationSettings.AvailableLocales.Locales.Count)
            return null; //Error!

        return LocalizationSettings.AvailableLocales.Locales[index];
    }

    public static Locale GetCurrentLanguage()
    {
        //Ensure the localization settings are properly initialized, blocking execution until the Task completes
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        return LocalizationSettings.SelectedLocale;
    }

    public static int GetCurrentLanguageIndex()
    {
        //Ensure the localization settings are properly initialized, blocking execution until the Task completes
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        for(int i=0; i<LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            if(LocalizationSettings.AvailableLocales.Locales[i] == LocalizationSettings.SelectedLocale)
                return i;
        }

        return -1; //Error!
    }

    //The default language is set in the Project Settings (EN).
    public static Locale GetDefaultLanguage()
    {
        //Ensure the localization settings are properly initialized, blocking execution until the Task completes
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        return LocalizationSettings.ProjectLocale;
    }

    public static int GetDefaultLanguageIndex()
    {
        //Ensure the localization settings are properly initialized, blocking execution until the Task completes
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        int index = LocalizationSettings.AvailableLocales.Locales.FindIndex(
            x => x.Identifier.Equals(LocalizationSettings.ProjectLocale.Identifier)
        );

        return index < 0? 0 : index;
    }

    //Return the current system language if it is supported; otherwise, return the default language.
    public static Locale GetSystemLanguage()
    {
        //Ensure the localization settings are properly initialized, blocking execution until the Task completes
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        IStartupLocaleSelector sysSelector = LocalizationSettings.StartupLocaleSelectors[SYSTEM_LOCALE_SELECTOR_INDEX];
        Locale locale = sysSelector.GetStartupLocale(LocalizationSettings.AvailableLocales);

        return locale == null? GetDefaultLanguage() : locale;
    }

    public static void SetCurrentLanguage(LocaleIdentifier id)
    {
        //Ensure the localization settings are properly initialized, blocking execution until the Task completes
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(id);
    }

    //SYNC OPERATIONS
    public static string GetString(string table, string entryName)
    {
        //Ensure the localization settings are properly initialized, blocking execution until the Task completes.
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        LocalizedString ls = new LocalizedString(table, entryName);

        if (ls.IsEmpty)
            return null;
        else
            return ls.GetLocalizedString();
    }

    public static string GetSmartString(string table, string entryName, Dictionary<string, IVariable> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return null;

        //Ensure the localization settings are properly initialized, blocking execution until the Task completes.
        LocalizationSettings.InitializationOperation.WaitForCompletion();

        LocalizedString ls = new LocalizedString(table, entryName);

        if (ls.IsEmpty)
            return null;
        else
            return ls.GetLocalizedString(parameters);
    }

    //ASYNC/COROUTINE OPERATIONS
    /*
     * Not used.
     * Retrieve the smart string from the table, populate it using the provided parameters, and execute the
     * callback function (or return the result).
     * Parameters: A dictionary where the key is a string representing the placeholder in the smart string,
     * and the value is an object representing the actual value to replace the placeholder.
     * In case of an error, the callback function will be invoked with a null string (or null will be returned).
     */
    public static IEnumerator GetFormattedString(string table, string key, Dictionary<string, object> parameters, Action<string> callback)
    {
        if (parameters == null || parameters.Count == 0)
            callback(null);

        Task<StringTableEntry> task = GetLocalizedEntry(table, key);
        yield return new WaitUntil(() => task.IsCompleted);

        StringTableEntry localizedString = task.Result;

        if (localizedString == null)
            callback(null);
        else
            callback(localizedString.GetLocalizedString(parameters));
    }

    public static IEnumerator GetFormattedStrings(string table, string key, Dictionary<string, object>[] parameters, Action<string[]> callback)
    {
        if (parameters == null || parameters.Length == 0)
            callback(null);

        Task<StringTableEntry> task = GetLocalizedEntry(table, key);
        yield return new WaitUntil(() => task.IsCompleted);

        StringTableEntry localizedString = task.Result;

        if(localizedString == null)
            callback(null); 
        else
        {
            string[] formattedStrings = new string[parameters.Length];

            for (int i = 0; i < formattedStrings.Length; i++)
            {
                //if(parameters[i] == null)
                //    formattedStrings[i] = null; //Error! No parameter!
                //else
                formattedStrings[i] = localizedString.GetLocalizedString(parameters[i]);
            }

            callback(formattedStrings);
        }
    }

    public static async Task GetFormattedStringAsync(string table, string key, Dictionary<string, object> parameters, Action<string> callback)
    {
        if (parameters == null || parameters.Count == 0)
            callback(null);

        StringTableEntry localizedString = await GetLocalizedEntry(table,key);

        if (localizedString == null)
            callback(null);
        else
            callback(localizedString.GetLocalizedString(parameters));
    }

    public static async Task<string> GetFormattedStringAsync(string table, string key, Dictionary<string, object> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return null;

        StringTableEntry localizedString = await GetLocalizedEntry(table, key);

        if (localizedString == null)
            return null;
        else
            return localizedString.GetLocalizedString(parameters);
    }

    public static async Task GetFormattedStringsAsync(string table, string key, Dictionary<string, object>[] parameters, Action<string[]> callback)
    {
        if (parameters == null || parameters.Length == 0)
            callback(null);

        StringTableEntry localizedString = await GetLocalizedEntry(table, key);

        if (localizedString == null)
            callback(null);
        else
        {
            string[] formattedStrings = new string[parameters.Length];

            for (int i = 0; i < formattedStrings.Length; i++)
            {
                //if(parameters[i] == null)
                //    formattedStrings[i] = null; //Error! No parameter!
                //else
                formattedStrings[i] = localizedString.GetLocalizedString(parameters[i]);
            }

            callback(formattedStrings);
        }
    }

    public static async Task<string[]> GetFormattedStringsAsync(string table, string key, Dictionary<string, object>[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
            return null;

        StringTableEntry localizedString = await GetLocalizedEntry(table, key);

        if (localizedString == null)
            return null;
        else
        {
            string[] formattedStrings = new string[parameters.Length];

            for (int i = 0; i < formattedStrings.Length; i++)
            {
                //if(parameters[i] == null)
                //    formattedStrings[i] = null; //Error! No parameter!
                //else
                formattedStrings[i] = localizedString.GetLocalizedString(parameters[i]);
            }

            return formattedStrings;
        }
    }

    /*
     * Perform the initialization process to retrieve the StringTableEntry.
     * Return null if an error occurs during the process.
     */
    private static async Task<StringTableEntry> GetLocalizedEntry(string table, string key)
    {
        //Ensure that the localization settings are properly initialized.
        await LocalizationSettings.InitializationOperation.Task;

        StringTable stringTable = LocalizationSettings.StringDatabase.GetTable(table);

        if (stringTable != null)
        {
            StringTableEntry localizedEntry = stringTable[key];
            if (localizedEntry != null)
            {
                return localizedEntry;
            }
            else
            {
                Debug.LogWarning($"GetFormattedString: String with {key} not found!");
                return null;
            }
        }
        else
        {
            Debug.LogWarning($"GetFormattedString: Table {table} not found!");
            return null;
        }
    }

    /*
     *  Extension Methods
     */

    //LocalizeStringEvent
    public static LocalizeStringEvent GetLocalizeStringEvent(this GameObject obj)
    {
        return obj.GetComponentInChildren<LocalizeStringEvent>(true);
    }

    //StringReference (LocalizedString)
    public static LocalizedString GetLocalizedString(this GameObject obj)
    {
        if(obj.GetLocalizeStringEvent() is LocalizeStringEvent lse)
            return lse.StringReference;

        return null;
    }

    //Updating LocalizeStringEvent
    public static LocalizedString UpdateLocalizeStringEvent(this GameObject obj, string table, string entryName)
    {
        LocalizedString ls = new LocalizedString(table, entryName);

        if (ls.IsEmpty)
            return null;

        LocalizeStringEvent lse = obj.GetLocalizeStringEvent();

        if (lse == null)
            return null;

        lse.SetTable(table);
        lse.SetEntry(entryName);
        lse.RefreshString();

        return lse.StringReference;
    }

    //Update SmartString
    public static void UpdateSmartString(this GameObject obj, Dictionary<string, IVariable> parameters)
    {
        if (parameters == null || parameters.Count < 1)
            return;

        LocalizedString ls = obj.GetLocalizedString();

        if (ls != null)
        {
            ls.Clear();

            foreach (KeyValuePair<string, IVariable> kvp in parameters)
            {
                ls[kvp.Key] = kvp.Value;
            }

            ls.RefreshString(); //Force the refresh
        }
    }
}

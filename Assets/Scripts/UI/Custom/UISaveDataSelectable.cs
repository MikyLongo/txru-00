/*
 * In the New Game/Load Game menu, a UI element is displayed for each game data profile.
 * This UI element shows the information associated with its respective game data profile.
 * This script, when attached to the UI element, provides a way to interact with it and update its information.
 * Refer to UIHandleSaveFile.cs for more details.
 */
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

public class UISaveDataSelectable : MonoBehaviour
{
    [SerializeField] private GameObject dataInfoContainer;
    [SerializeField] private GameObject noDataContainer;
    [SerializeField] private TMP_Text noData;
    [SerializeField] private TMP_Text numSlot;
    [SerializeField] private TMP_Text unlocked;
    [SerializeField] private TMP_Text completed;
    [SerializeField] private TMP_Text attempts;
    [SerializeField] private TMP_Text wins;
    [SerializeField] private TMP_Text checkpoint;
    [SerializeField] private TMP_Text lastSave;
    [SerializeField] private Button button;
    private bool hasData;

    public bool HasData { get { return hasData; } }

    public void ShowData(bool show)
    {
        hasData = show;
        dataInfoContainer.SetActive(show);
        noDataContainer.SetActive(!show);
    }

    public void SetNumSlot(int value)
    {
        numSlot.text = value.ToString("00");
    }

    public void SetUnlocked(int value)
    {
        unlocked.gameObject.UpdateSmartString(new Dictionary<string, IVariable>() {
            { "num", new IntVariable(){ Value = value } }
        });
    }

    public void SetCompleted(string rank)
    {
        if(string.IsNullOrEmpty(rank))
            completed.gameObject.SetActive(false);
        else
        {
            completed.gameObject.UpdateSmartString(new Dictionary<string, IVariable>() {
                { "rank", new StringVariable(){ Value = rank } }
            });
            completed.gameObject.SetActive(true);
        }
    }

    public void SetAttempts(int value)
    {
        attempts.gameObject.UpdateSmartString(new Dictionary<string, IVariable>() {
            { "num", new IntVariable(){ Value = value } }
        });
    }

    public void SetWins(int value)
    {
        wins.gameObject.UpdateSmartString(new Dictionary<string, IVariable>() {
            { "num", new IntVariable(){ Value = value } }
        });
    }

    public void SetCheckpoint(bool value)
    {
        checkpoint.gameObject.SetActive(value);
    }

    public void SetLastSaveTime(DateTime value)
    {
        lastSave.gameObject.UpdateSmartString(new Dictionary<string, IVariable>() {
            { "value", new DateTimeVariable() {Value = value } }
        });
    }

    public Button Button { get { return button; } set { } }
}

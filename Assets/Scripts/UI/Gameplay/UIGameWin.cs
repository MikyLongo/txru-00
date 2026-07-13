//Script that manages the UI screen displayed when the player completes a level.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

public class UIGameWin : MonoBehaviour, IPageUI
{
    [SerializeField] private Button bNext;
    [SerializeField] private Button bRestart;
    [SerializeField] private Button bExit;
    [SerializeField] private List<TMP_Text> tRecords;
    [SerializeField] private TMP_Text tRecordCurrent;
    [SerializeField] private TMP_Text tRecordBest;
    [SerializeField] private Color cDefaultRecord;
    [SerializeField] private Color cNewRecord;
    [SerializeField] private GameObject firstSelected;

    private void OnEnable()
    {
        firstSelected = bNext.gameObject;
        UIManager.Instance.OpenedPage = this;

        bNext.onClick.AddListener(OnNextClick);
        bRestart.onClick.AddListener(OnRestartClick);
        bExit.onClick.AddListener(OnExitClick);
    }

    private void OnDisable()
    {
        bNext.onClick.RemoveAllListeners();
        bRestart.onClick.RemoveAllListeners();
        bExit.onClick.RemoveAllListeners();
    }

    public void UpdateUI(List<float> records, List<float> rawRecords, int bestRecordIndex, float currentRecord, bool nextLevel, bool restartLevel)
    {
        tRecordCurrent.gameObject.UpdateSmartString(
            new Dictionary<string, IVariable> { 
                { "timeString", new StringVariable() { Value = currentRecord.ToTimeString() } } 
        });

        for (int i = 0; i < tRecords.Count; i++)
        {
            tRecords[i].gameObject.UpdateSmartString(
                new Dictionary<string, IVariable> {
                    { "position", new IntVariable() { Value = i+1 } },
                    { "timeString", new StringVariable() { Value = records[i].ToTimeString() }}
            });

            if(records[i] < rawRecords[i] && !rawRecords.Contains(records[i]))
                tRecords[i].color = cNewRecord;
            else
                tRecords[i].color = cDefaultRecord;
        }

        if (bestRecordIndex == records.Count-1) //If the best record is not within the top 3
        {
            tRecordBest.gameObject.UpdateSmartString(
                new Dictionary<string, IVariable> { 
                    { "timeString", new StringVariable() { Value = records[records.Count - 1].ToTimeString() } } 
            });
            tRecordBest.gameObject.SetActive(true);
        }
        else
            tRecordBest.gameObject.SetActive(false);

        if(nextLevel)
        {
            firstSelected = bNext.gameObject;
            bNext.interactable = true;
        }
        else
        {
            firstSelected = bExit.gameObject;
            bNext.interactable = false;
        }

        bRestart.interactable = restartLevel;

        UIManager.Instance.EventSystemHandler.UpdateSelection(firstSelected);
    }

    private void OnNextClick()
    {
        GameManager.Instance.StartNextLevel();
    }

    private void OnRestartClick()
    {
        GameManager.Instance.RestartLevel();
    }

    private void OnExitClick()
    {
        GameManager.Instance.ReturnToHub();
    }


    //Implementation of the IPageUI interface
    public void OnPausePage()
    {
        //Does Nothing    
    }

    public GameObject GetFirstSelected()
    {
        return firstSelected;
    }
}

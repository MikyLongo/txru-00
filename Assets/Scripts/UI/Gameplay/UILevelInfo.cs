/*
 * Script that manages the modal window displayed at the start of a level.
 * Displays information about the mission, tutorial, and story elements.
 */

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class UILevelInfo : MonoBehaviour, IModalUI
{
    [SerializeField] private TMP_Text levelName;
    [SerializeField] private TMP_Text levelInfo;
    [SerializeField] private TMP_Text levelClose;
    [SerializeField] private GameObject scrollbar;

    private void OnEnable()
    {
        UIManager.Instance.EventSystemHandler.ModalRequestFocus(scrollbar, true);
        UIManager.Instance.OpenedModal = this;
    }

    public void Show(bool show, int level, string infoLocEntry, Dictionary<string, IVariable> infoParams)
    {
        if (show)
        {
            //Updating localized UI
            levelName.gameObject.UpdateSmartString(
                new Dictionary<string, IVariable>() { { "num", new IntVariable() { Value = level } } }
            );
            levelInfo.gameObject.UpdateSmartString(infoParams);
            levelInfo.gameObject.UpdateLocalizeStringEvent(LocalizationHelper.UI_LEVEL_INFO, infoLocEntry);
            levelClose.gameObject.UpdateSmartString(
                new Dictionary<string, IVariable>() { 
                    { "key", new StringVariable() { Value = GameData.GameInput.PrintPauseBinding() } } 
                });
        }

        gameObject.SetActive(show);
    }

    //Implementation of the IModalUI interface
    public void OnPauseModal()
    {
        UIManager.Instance.EventSystemHandler.ModalRequestFocus(null, false);
        UIManager.Instance.OpenedModal = null;
        LevelManager.Instance.LevelInfoClosed();
        gameObject.SetActive(false);
    }
}

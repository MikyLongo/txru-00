/*
 * Script that manages the UI page displaying the New Game/Load Game menu.
 * This menu consists of a list of game data profiles that the user can interact with.
 * Each game data profile is represented as a UI element defined by its prefab, with the UISaveDataSelectable 
 * script attached to enable interaction.
 * 
 * New Game:
 * All game profiles are interactable, whether empty or not.
 * If the user attempts to create a new game with an already existing profile, a message box is displayed 
 * to confirm the overwrite operation.
 * 
 * Load Game:
 * Only existing game profiles are interactable.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIHandleSaveFile : MonoBehaviour, IPageUI
{
    [SerializeField] private UISaveDataSelectable prefabSelectable;
    [SerializeField] private GameObject container;
    [SerializeField] private UICustomScrollView scrollView;
    [SerializeField] private TMP_Text tNewGame;
    [SerializeField] private TMP_Text tLoadGame;
    [SerializeField] private bool load = false; //false = New Game | true = Load Game
    [SerializeField] private List<UISaveDataSelectable> saveData;
    [SerializeField] private List<GameData.GameMemory.PartialMemory> dataInfos;
    [SerializeField] private GameObject firstSelected = null;
    [SerializeField] private bool initialized = false;

    private void Start()
    {
        scrollView.NumSteps = GameSaver.NUM_FILE;
        saveData = new List<UISaveDataSelectable>();
        //dataInfos = GameSaver.LoadDataInfos();
        dataInfos = GameManager.Instance.LoadMemoryInfos();

        //Creates the selectable UI element and initializes it as an empty, non-interactable slot
        for (int i=0; i<GameSaver.NUM_FILE; i++)
        {
            UISaveDataSelectable elem =Instantiate<UISaveDataSelectable>(prefabSelectable, container.transform);
            saveData.Add(elem);
            elem.SetNumSlot(i + 1);
            elem.ShowData(false);
            elem.Button.interactable = false;
        }

        //Loads data into their respective slots
        for (int i = 0; i < dataInfos.Count; i++)
        {
            int index = dataInfos[i].numSlot;

            saveData[index].SetUnlocked(dataInfos[i].unlockedLevel);
            saveData[index].SetAttempts(dataInfos[i].totalAttempts);
            saveData[index].SetWins(dataInfos[i].totalWins);
            saveData[index].SetLastSaveTime(DateTime.ParseExact(dataInfos[i].lastSave, "o", null));
            saveData[index].SetCheckpoint(dataInfos[i].hasContinueSave);
            saveData[index].SetCompleted(dataInfos[i].rank);
            saveData[index].ShowData(true);
        }

        initialized = true;
        UpdateUI();
    }

    private void OnEnable()
    {
        if (initialized)
        {
            UpdateUI();
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < saveData.Count; i++)
        {
            saveData[i].Button.interactable = false;
            saveData[i].Button.onClick.RemoveAllListeners();
            saveData[i].GetComponent<UICustomElement>().onMove.RemoveAllListeners();
        }
        scrollView.ScrollY(0, 0, 0);
    }

    private void UpdateUI()
    {
        tNewGame.gameObject.SetActive(!load);
        tLoadGame.gameObject.SetActive(load);
        
        int j = 0;

        for(int i=0; i<saveData.Count; i++)
        {
            //Updates navigation settings for each game data profile
            Navigation navigation = new Navigation();
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnLeft = null;
            navigation.selectOnRight = null;
            int index = i;

            if (load)
            {
                //Sets as interactable only the slots containing data
                if (j < dataInfos.Count && dataInfos[j].numSlot == i)
                {
                    if (j == 0) //Sets the first selected slot to the first available save data
                        firstSelected = saveData[i].Button.gameObject;

                    saveData[i].Button.interactable = true;
                    saveData[i].Button.onClick.AddListener(() => OnSelect(index));
                    saveData[i].GetComponent<UICustomElement>().onMove.AddListener(Move);

                    //Sets navigation
                    if (j - 1 > -1)
                        navigation.selectOnUp = saveData[dataInfos[j - 1].numSlot].Button;
                    else
                        navigation.selectOnUp = null;

                    if (j + 1 < dataInfos.Count)
                        navigation.selectOnDown = saveData[dataInfos[j + 1].numSlot].Button;
                    else
                        navigation.selectOnDown = null;

                    j++;
                }
                else
                {
                    //No listeners, and interactable is set to false (handled via the Start or OnDisable methods)

                    //Sets navigation
                    navigation.selectOnUp = null;
                    navigation.selectOnDown = null;
                }
            }
            else //false = New Game => Every slot is interactable
            {
                saveData[i].Button.interactable = true;
                saveData[i].Button.onClick.AddListener(() => OnSelect(index));
                saveData[i].GetComponent<UICustomElement>().onMove.AddListener(Move);

                //Sets navigation
                if (i - 1 > -1)
                    navigation.selectOnUp = saveData[i - 1].Button;
                else
                    navigation.selectOnUp = null;

                if(i+1 < saveData.Count)
                    navigation.selectOnDown = saveData[i+1].Button;
                else
                    navigation.selectOnDown = null;
            }

            saveData[i].Button.navigation = navigation;
        }

        if (!load) //If New Game, set the first selected slot to the first one
            firstSelected = saveData[0].Button.gameObject;

        //Moves to the first selected button
        /*
         * On the first startup, the ScrollRect may still be updating its information, such as content size 
         * or scroll position. 
         * Upon completing the update, the ScrollRect resets the position of the vertical scrollbar, 
         * which can lead to the loss of the intended settings.
         * To address this, a coroutine is used to delay the execution of the settings.
         * This issue is noticeable when there is only one save file, especially if it is not among the first 
         * slots (e.g., the last one).
         */

        StartCoroutine(DelayedScroll());

        UIManager.Instance.OpenedPage = this;
    }

    private IEnumerator DelayedScroll()
    {
        yield return new WaitForEndOfFrame();
        MoveToButton(firstSelected);
    }

    //Must be called to make the UI element interactable and function properly
    public void Show(bool show, bool load)
    {
        if (show)
        {
            this.load = load;
        }

        gameObject.SetActive(show);
    }

    public void OnSelect(int numSlot)
    {
        if(load)
        {
            GameManager.Instance.LoadGame(numSlot);
        }
        else
        {
            if (saveData[numSlot].HasData) //Prompts for confirmation to overwrite the game data profile
            {
                UIMessageBox mBox = UIManager.Instance.GetUIMessageBox();
                mBox.Open(
                    LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, LocalizationHelper.LK_WARNING),
                    LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, UIMessageBox.LK_SLOT_HAS_DATA),
                    LocalizationHelper.GetString(LocalizationHelper.UI_MESSAGE, LocalizationHelper.LK_CONFIRM),
                    () => { GameManager.Instance.NewGame(numSlot); },
                    true,
                    null,
                    0,
                    UIMessageBox.MsgType.Warning,
                    true
                );
            }
            else
                GameManager.Instance.NewGame(numSlot);
        }
    }

    //Event handlers
    private void Move(AxisEventData eventData) //Triggered when navigating between game data slots
    {
        MoveToButton(eventData.selectedObject);
    }

    private void MoveToButton(GameObject button)
    {
        float y = button.transform.GetComponent<RectTransform>().anchoredPosition.y;
        float h = button.transform.GetComponent<RectTransform>().sizeDelta.y;
        scrollView.ScrollYCentered(y, h);
    }

    //Implementation of the IPageUI interface
    public GameObject GetFirstSelected()
    {
        return firstSelected;
    }

    public void OnPausePage()
    {
        UIManager.Instance.ShowHandleSaveFileUI(false, false);
    }
}

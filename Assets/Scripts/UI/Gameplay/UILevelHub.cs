//Script that manages the level hub
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

public class UILevelHub : MonoBehaviour, IPageUI
{
    [SerializeField] private Button bTemplate;
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject levelInfo;
    [SerializeField] private TMP_Text tLevelName;
    [SerializeField] private List<TMP_Text> tRecords;
    //[SerializeField] private TMP_Text tRecordB;
    [SerializeField] private TMP_Text tAttempts;
    [SerializeField] private TMP_Text tWin;
    [SerializeField] private Button bStart;
    [SerializeField] private Button bContinue;
    [SerializeField] private List<Button> buttonList;

    [SerializeField] private Color cDefaultRecord;
    [SerializeField] private Color cNewRecord;

    private int selectedLevel;
    private int unlockedLevel;

    private bool initialized = false;
    private GameObject firstSelected = null;


    private void Start()
    {
        levelInfo.SetActive(false);
        bContinue.interactable = false;
        bStart.onClick.AddListener(OnStartLevel);
        bContinue.onClick.AddListener(OnContinueLevel);

        //Instantiates buttons for level selection
        buttonList = new List<Button>();
        unlockedLevel = GameManager.Instance.Memory.UnlockedLevel;

        for (int i = 0; i < GameManager.Instance.GetTotalLevels(); i++)
        {
            int level = i + 1;
            Button b = Instantiate<Button>(bTemplate, content.transform);
            buttonList.Add(b);

            if (i < unlockedLevel)
            {
                //Sets as interactable only the button corresponding to the unlocked level
                b.interactable = true;
                b.onClick.AddListener(() => OnSelectLevel(level));
            }

            if(i == unlockedLevel-1)
            {
                firstSelected = b.gameObject; //The last unlocked level will be the "firstSelected"
            }

            b.gameObject.name = $"bLevel{level}";
            b.gameObject.UpdateSmartString(
                new Dictionary<string, IVariable> { { "level", new IntVariable() { Value = level } } });
            b.gameObject.SetActive(true);
        }

        initialized = true;
        UIManager.Instance.OpenedPage = this;
    }

    private void OnEnable()
    {
        if(initialized)
        {
            bStart.onClick.AddListener(OnStartLevel);
            bContinue.onClick.AddListener(OnContinueLevel);
            for (int i = 0; i < unlockedLevel; i++)
            {
                int ind = i+1;
                buttonList[i].onClick.AddListener(() => { OnSelectLevel(ind); });
            }

            UIManager.Instance.OpenedPage = this;
        }
    }

    private void OnDisable()
    {
        bStart.onClick.RemoveAllListeners();
        bContinue.onClick.RemoveAllListeners();

        for (int i = 0; i < unlockedLevel; i++)
        {
            buttonList[i].onClick.RemoveAllListeners();
        }
    }

    private void OnSelectLevel(int selectedLevel)
    {
        this.selectedLevel = selectedLevel;
        GameData.GameLevel level = GameManager.Instance.GetLevel(selectedLevel);
        GameData.LevelSO rawLevel = GameManager.Instance.GetRawLevel(selectedLevel);

        tLevelName.gameObject.UpdateSmartString(
            new Dictionary<string, IVariable> { { "level", new IntVariable() { Value = level.Level } } });

        if (rawLevel.levelType == GameData.LevelSO.LevelType.LevelGameplay)
        {
            List<float> records = level.Records;
            List<float> rawRecords = rawLevel.records;

            for (int i = 0; i < tRecords.Count; i++)
            {
                if (!tRecords[i].gameObject.activeSelf)
                    tRecords[i].gameObject.SetActive(true);

                tRecords[i].gameObject.UpdateSmartString(
                    new Dictionary<string, IVariable> {
                    { "position", new IntVariable() { Value = i+1 } },
                    { "timeString", new StringVariable() { Value = records[i].ToTimeString() }}
                });

                if (records[i] < rawRecords[i] && !rawRecords.Contains(records[i]))
                    tRecords[i].color = cNewRecord;
                else
                    tRecords[i].color = cDefaultRecord;
            }

            if (!tAttempts.gameObject.activeSelf)
                tAttempts.gameObject.SetActive(true);

            tAttempts.gameObject.UpdateSmartString(
                new Dictionary<string, IVariable> { { "num", new IntVariable() { Value = level.Attemps } } });

            if (!tWin.gameObject.activeSelf)
                tWin.gameObject.SetActive(true);

            tWin.gameObject.UpdateSmartString(
                new Dictionary<string, IVariable> { { "num", new IntVariable() { Value = level.Beaten } } });
        }
        else
        {
            for(int i=0; i<tRecords.Count; i++)
            {
                if (tRecords[i].gameObject.activeSelf)
                    tRecords[i].gameObject.SetActive(false);
            }

            if (tAttempts.gameObject.activeSelf)
                tAttempts.gameObject.SetActive(false);

            if (tWin.gameObject.activeSelf)
                tWin.gameObject.SetActive(false);
        }

        Navigation navStart = bStart.navigation;

        //Sets the start button's left navigation to the button corresponding to the selected level
        navStart.selectOnLeft = buttonList[level.Level - 1];

        if (GameManager.Instance.Memory.ContinueIndex == level.Level - 1)
        {
            navStart.selectOnDown = bContinue;

            //Sets the continue button's left navigation to the button corresponding to the selected level
            Navigation navContinue = bContinue.navigation;
            navContinue.selectOnLeft = buttonList[level.Level-1];
            bContinue.navigation = navContinue;

            bContinue.interactable = true;
        }
        else
        {
            navStart.selectOnDown = null;
            bContinue.interactable = false;
        }

        bStart.navigation = navStart;

        levelInfo.SetActive(true);
    }

    private void OnStartLevel()
    {
        GameManager.Instance.StartLevel(selectedLevel);
    }

    private void OnContinueLevel()
    {
        GameManager.Instance.ContinueLevel(selectedLevel);
    }

    //Implementation of the IPageUI interface
    public void OnPausePage()
    {
        UIManager.Instance.ShowSettingsUI(true);
    }

    public GameObject GetFirstSelected()
    {
        return firstSelected;
    }
}

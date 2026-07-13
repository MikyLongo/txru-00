/*
 * Script that manages the UI of the submenu within the Pause menu in the Gameplay scene.
 * This submenu displays a list of "tutorial" topics that are supplemental to those provided during normal 
 * gameplay.
 * These tutorial topics are progressive and unlock as levels are unlocked.
 * For more information, refer to the GameHelpSO ScriptableObject located in the folder 
 * "Assets/Scripts/Gameplay/Others".
 */

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIGameHelp : MonoBehaviour, IPageUI
{
    [SerializeField] private UICustomScrollView scrollView;
    [SerializeField] private GameObject sideBarContainer;
    [SerializeField] private GameObject descriptionContainer;
    [SerializeField] private Button bTemplate;

    [SerializeField] private TMP_Text title;

    [SerializeField] private RectTransform imgContainer;
    [SerializeField] private Image imgDescription;
    [SerializeField] private AspectRatioFitter imgRatioFitter;

    [SerializeField] private TMP_Text description;

    [SerializeField] private GameHelpSO gameHelpSO;

    [SerializeField] private List<Button> helpersButton = new List<Button>();
    private GameObject firstSelected = null;
    private bool initialized = false;

    private void OnEnable()
    {
        int totalLevels = GameManager.Instance.Memory.UnlockedLevel;

        if (initialized)
        {
            if (helpersButton.Count > 0) 
            {
                UpdateDescription(0, 0);
            }

            int k = 0;
            for (int i = 0; i < gameHelpSO.helpers.Count; i++)
            {
                GameHelpSO.LevelHelpers helpers = gameHelpSO.helpers[i];

                if (helpers.unlockAtLevel <= totalLevels)
                {
                    for (int j = 0; j < helpers.helperElements.Length; j++)
                    {
                        int ic = i;
                        int jc = j;

                        helpersButton[k].onClick.AddListener(() => UpdateDescription(ic, jc));
                        helpersButton[k].GetComponent<UICustomElement>().onMove.AddListener(Move);
                        k++;
                    }
                }
                else
                {
                    break;
                }
            }

            UIManager.Instance.OpenedPage = this;
            return;
        }

        for (int i = 0; i < gameHelpSO.helpers.Count; i++)
        {
            GameHelpSO.LevelHelpers helpers = gameHelpSO.helpers[i];

            if(helpers.unlockAtLevel <=  totalLevels)
            {
                for(int j=0; j< helpers.helperElements.Length; j++)
                {
                    Button b = Instantiate<Button>(bTemplate, sideBarContainer.transform);
                    helpersButton.Add(b);

                    if (firstSelected == null)
                    {
                        firstSelected = b.gameObject;
                        UpdateDescription(i, j);
                    }

                    int ic = i;
                    int jc = j;

                    b.onClick.AddListener(() => UpdateDescription(ic,jc));
                    b.GetComponent<UICustomElement>().onMove.AddListener(Move);
                    b.GetTMPText().gameObject.UpdateLocalizeStringEvent(LocalizationHelper.GAME_HELPER, helpers.helperElements[j].titleKey);
                    b.gameObject.SetActive(true);
                }
            }
            else
            {
                break;
            }
        }

        initialized = true;
        UIManager.Instance.OpenedPage = this;
    }

    private void OnDisable()
    {
        foreach (Button b in helpersButton)
        {
            b.onClick.RemoveAllListeners();
            b.GetComponent<UICustomElement>().onMove.RemoveAllListeners();
        }
    }

    //Event handlers
    private void UpdateDescription(int helperKey, int elementKey)
    {
        GameHelpSO.HelperElement element = gameHelpSO.helpers[helperKey].helperElements[elementKey];

        title.gameObject.UpdateLocalizeStringEvent(LocalizationHelper.GAME_HELPER, element.titleKey);

        if(element.sprite == null)
        {
            imgContainer.gameObject.SetActive(false);
        }
        else
        {
            imgContainer.gameObject.SetActive(true);
            imgDescription.sprite = element.sprite;
            float aspectRatio = element.sprite.rect.width / element.sprite.rect.height;
            imgRatioFitter.aspectRatio = aspectRatio;
        }

        description.gameObject.UpdateLocalizeStringEvent(LocalizationHelper.GAME_HELPER, element.descriptionKey);
    }

    private void Move(AxisEventData eventData)
    {
        float y = eventData.selectedObject.transform.GetComponent<RectTransform>().anchoredPosition.y;
        float h = eventData.selectedObject.transform.GetComponent<RectTransform>().sizeDelta.y;
        scrollView.ScrollYCentered(y, h);
    }

    //Implementation of the IPageUI interface
    public GameObject GetFirstSelected()
    {
        return firstSelected;
    }

    public void OnPausePage()
    {
        UIManager.Instance.ShowGameHelpUI(false);
    }
}

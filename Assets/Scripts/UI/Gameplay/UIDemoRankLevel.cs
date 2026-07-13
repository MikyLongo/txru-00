/*
 * The last level is a unique type of Gameplay scene (a DemoRankLevel scene) as it does not include actual 
 * gameplay. Instead, it displays one of two pages.
 * The first page provides details about the story, while the second shows the rank achieved.
 * To proceed to the next page, the player must press the "Pause/Menu" button.
 * Note: The rank is determined by the result of the player's first attempt at each level.
 */
using TMPro;
using UnityEngine;

public class UIDemoRankLevel : MonoBehaviour, IPageUI
{
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;
    [SerializeField] private TMP_Text tConclusion;
    [SerializeField] private TMP_Text tRank;

    [SerializeField] private Color colorS;
    [SerializeField] private Color colorA;
    [SerializeField] private Color colorB;
    [SerializeField] private Color colorC;
    [SerializeField] private Color colorD;
    [SerializeField] private Color colorE;
    [SerializeField] private Color colorF;

    private static readonly string CONCLUSION_KEY = "Level_Lv6_Conclusion_";
    private int cPage = 0;

    private void Start()
    {
        string rank = GameManager.Instance.Memory.CalcualteDemoRank();

        Color color = colorS;

        switch (rank)
        {
            case "A":
                color = colorA;
            break;

            case "B":
                color = colorB;
            break;

            case "C":
                color = colorC;
            break;

            case "D":
                color = colorD;
            break;
            
            case "E":
                color = colorE;
            break;

            case "F":
                color = colorF;
            break;
        }

        tConclusion.gameObject.UpdateLocalizeStringEvent(LocalizationHelper.UI_LEVEL_INFO, CONCLUSION_KEY + rank);
        tRank.text = rank;
        tRank.color = color;
        page1.SetActive(true);
    }

    //Implementation of the IPageUI interface
    public GameObject GetFirstSelected()
    {
        return null;
    }

    public void OnPausePage()
    {
        if(cPage == 0)
        {
            page1.SetActive(false);
            page2.SetActive(true);
            cPage++;
        }
        else
            GameManager.Instance.ReturnToHub();
    }
}

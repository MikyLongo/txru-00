//Handles the UI in the Gameplay scene by displaying the minimap, item/skill bar, quest, and timer
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplay : MonoBehaviour
{
    //Timer
    [SerializeField] private TMP_Text timer = null;
    [SerializeField] private Color normalTimer = Color.white;
    [SerializeField] private Color endingTimer = Color.red; //Color used by the timer as it approaches the end

    //Minimap
    [SerializeField] private Minimap prefabMinimap = null;
    [SerializeField] private Minimap minimap = null;
    [SerializeField] private RectTransform minimapUI;

    //Equip Items
    [SerializeField] private EquipItemUI[] equipItems = new EquipItemUI[3];
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color unequippedColor = Color.red;
    [SerializeField] private Color equippedColor = Color.green;

    //Quest Items
    [SerializeField] private GameObject questItemsContainer;
    [SerializeField] private QuestItemUI[] questItems = new QuestItemUI[3];
    [SerializeField] private Color qiUncollectedColor = Color.red;
    [SerializeField] private Color qiCollectedColor = Color.green;

    /*
     *  Functions
     */
    //Timer
    public TMP_Text Timer { get { return timer; } }
    public void SetNormalTimer()
    {
        timer.color = normalTimer;
    }

    public void SetEndingTimer()
    {
        timer.color = endingTimer;
    }

    //Minimap
    private void GetMinimap()
    {
        if (minimap == null)
        {
            if ((minimap = GameObject.FindObjectOfType<Minimap>(true)) == null)
            {
                minimap = Instantiate<Minimap>(prefabMinimap,Vector3.zero,Quaternion.identity);
            }
        }
    }

    public void ShowMinimap(bool show)
    {
        GetMinimap();
        minimap.gameObject.SetActive(show);
        minimapUI.gameObject.SetActive(show);
    }

    public void ShowMinimap(bool show, float leftBorder, float rightBorder, float topBorder, float bottomBorder)
    {
        GetMinimap();
        minimap.gameObject.SetActive(show);
        minimap.LeftBorder = leftBorder;
        minimap.RightBorder = rightBorder;
        minimap.TopBorder = topBorder;
        minimap.BottomBorder = bottomBorder;
        minimapUI.gameObject.SetActive(show);
    }


    //Items
    public void GotItem(int index, Sprite texture, int numUse, bool isEquipable)
    {
        if (index < 0 || index >= equipItems.Length)
            return;

        Color color = isEquipable ? unequippedColor : defaultColor;
        equipItems[index].Border.color = color;
        
        equipItems[index].Sprite.sprite = texture;
        equipItems[index].Sprite.gameObject.SetActive(true);

        if (numUse > 0)
        {
            equipItems[index].BGNumUse.color = color;
            equipItems[index].BGNumUse.gameObject.SetActive(true);
            equipItems[index].NumUse.text = $"{numUse}";

        }
        else
            equipItems[index].BGNumUse.gameObject.SetActive(false); 
    }

    public void RemoveItem(int index)
    {
        if(index < 0 || index >= equipItems.Length)
            return;

        equipItems[index].Border.color = defaultColor;
        equipItems[index].Sprite.gameObject.SetActive(false);
        equipItems[index].BGNumUse.gameObject.SetActive(false);
    }

    public void EquippedItem(int index, bool equipped)
    {
        if (index < 0 || index >= equipItems.Length)
            return;

        if(equipped)
        {
            equipItems[index].Border.color = equippedColor;
            equipItems[index].BGNumUse.color = equippedColor;
        }
        else
        {
            equipItems[index].Border.color = unequippedColor;
            equipItems[index].BGNumUse.color = unequippedColor;
        }
    }

    public void UseItem(int index, int numUse)
    {
        if (index < 0 || index >= equipItems.Length)
            return;

        if (numUse > 0)
            equipItems[index].NumUse.text = $"{numUse}";
        else
            equipItems[index].BGNumUse.gameObject.SetActive(false); //Infinite uses (number not displayed)
    }

    //QuestItems
    public void UpdateQuestItemState(QuestItem[] qItems)
    {
        if(qItems == null || qItems.Length == 0)
        {
            questItemsContainer.SetActive(false);
            return;
        }

        questItemsContainer.SetActive(true);

        for (int i=0; i<questItems.Length; i++)
        {
            if (i < qItems.Length)
                questItems[i].Border.gameObject.SetActive(true);
            else
            {
                questItems[i].Border.gameObject.SetActive(false);
                continue;
            }

            if (qItems[i].obtained)
                questItems[i].Border.color = qiCollectedColor;
            else
                questItems[i].Border.color = qiUncollectedColor;

            questItems[i].Sprite.sprite = qItems[i].item.sprite;
        }
    }

    [System.Serializable]
    public struct EquipItemUI
    {
        [SerializeField] private Image iBorder;
        [SerializeField] private Image iSprite;
        [SerializeField] private Image bgNumUse;
        [SerializeField] private TMP_Text tNumUse;

        public EquipItemUI(Image iBorder, Image iSprite, Image bgNumUse,TMP_Text tNumUse)
        {
            this.iBorder = iBorder;
            this.iSprite = iSprite;
            this.bgNumUse = bgNumUse;
            this.tNumUse = tNumUse;
        }

        public Image Border { get { return iBorder; } }
        public Image Sprite { get { return iSprite; } }
        public Image BGNumUse {  get { return bgNumUse; } }
        public TMP_Text NumUse { get { return tNumUse; } }
    }

    [System.Serializable]
    public struct QuestItemUI
    {
        [SerializeField] private Image iBorder;
        [SerializeField] private Image iSprite;

        public QuestItemUI(Image iBorder, Image iSprite)
        {
            this.iBorder = iBorder;
            this.iSprite = iSprite;
        }

        public Image Border { get { return iBorder; } }
        public Image Sprite { get { return iSprite; } }
    }
}

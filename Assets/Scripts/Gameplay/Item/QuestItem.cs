/*
 * Class that defines quest items and their states.
 */
using UnityEngine;

[System.Serializable]
public class QuestItem 
{
    [SerializeField] public Item item;
    [SerializeField] public bool obtained;

    public QuestItem(Item item, bool obtained)
    {
        this.item = item;
        this.obtained = obtained;
    }

    public QuestItem(Item item)
    {
        this.item = item;
        obtained = false;
    }
}

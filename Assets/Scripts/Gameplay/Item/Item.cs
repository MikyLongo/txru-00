/*
 * ScriptableObject (SO) used to define items in the game.
 * It specifies the basic characteristics of the item, determines if it is usable, the number of uses, 
 * and whether it is equipable by referencing the Equip prefab.
 * Each item is assigned a unique ID, which is managed through a ScriptableObject file called ItemListID 
 * (see ItemListID).
 * Since changes to a SO are permanent, and items may contain mutable data, the SO provides an internal, 
 * serializable class to store only the mutable information along with their states.
 */

using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/BaseItem")]
public class Item : ScriptableObject
{
    public int id; //To be set with itemIDs SO file in Script/Gameplay/Item (id=0 indicates no item!)
    public string itemName;
    public Sprite sprite;
    public UseType useType;
    public int numUse; //-1 indicates infinite uses
    public bool isEquipable;
    public Equip prefabEquip;

    public enum UseType
    {
        QUEST,
        BOX,
        INVISIBLE,
        DASH,
        //Shooting
        SHOOT, //General category
        SHOOT_SR_LASER
    }

    public virtual Item Copy()
    {
        return Instantiate(this);
    }

    public virtual SerializableItem Serialize(bool equipped)
    {
        return new SerializableItem(id, numUse, equipped);
    }

    [System.Serializable]
    public class SerializableItem 
    {
        public int id;
        public int numUse;
        public bool equipped;

        public SerializableItem(int id, int numUse, bool equipped)
        {
            this.id = id;
            this.numUse = numUse;
            this.equipped = equipped;
        }

        public virtual Item Deserialize()
        {
            //Locate the Item ScriptableObject
            Item item = GameManager.Instance.ItemsIDs.Get(id);

            if (item != null)
            {   //Make a copy
                item = Instantiate(item);
                //Restore the previously saved value
                item.numUse = numUse;

                return item;
            }

            return null;
        }
    }
}

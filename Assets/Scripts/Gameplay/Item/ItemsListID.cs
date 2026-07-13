//ScriptableObject (SO) designed to assign unique IDs to items (Item SO) through the Inspector.
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemIDs", menuName = "ItemsList/ItemIDs")]
public class ItemsListID : ScriptableObject 
{
    public List<ItemWrapper> items;

    [System.Serializable]
    public struct ItemWrapper : ISerializationCallbackReceiver
    {
        public Item item;
        public int id;

        //This method is called after the object is deserialized
        public void OnAfterDeserialize()
        {
            if(item != null)
                item.id = id;
        }

        //This method is called before the object is serialized
        public void OnBeforeSerialize() { }
    }

    public Item Get(int id)
    {
        if (id < 0)
            return null; //Not found

        foreach (ItemWrapper wrapper in items)
        {
            if(wrapper.id == id)
                return wrapper.item;
        }

        return null; //Not found
    }
}

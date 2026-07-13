/*
 * Defines a generic GameObject representing an Item that can be collected/looted (see Item ScriptableObject).
 * When the player attempts to collect the item, a message will appear on screen providing information about 
 * the collection process (see the ItemMessage class).
 * The item can be a usable item (part of the player's item/skill bar), a quest item, or both.
 * It can also serve as a Guardable entity.
 */
using UnityEngine;

public class LootableItem : MonoBehaviour, IStateSaveable, IGuardable
{
    [SerializeField] private ItemMessage itemMessage;
    [SerializeField] private Item item;
    [SerializeField] private bool isUsable;
    [SerializeField] private bool isQuest;
    [SerializeField] private bool looted = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == EngineConf.Layer.PLAYER && other.CompareTag(EngineConf.Tag.PLAYER))
        {
            if(isUsable) //An item can be usable
            {
                if(PlayerManager.Instance.AddItem(item.Copy())) //true: Item successfully looted
                {
                    if (isQuest) //and a quest item
                    {
                        LevelManager.Instance.QuestItemCollected(item);
                    }
                    itemMessage.ShowMessage(ItemMessage.ItemMessageType.LOOTED);
                }
                else //false: Item not looted
                {
                    itemMessage.ShowMessage(ItemMessage.ItemMessageType.NOTLOOTED);
                    return;
                }
            }
            else // If it is not a usable item, it must be a quest item! (Looted)
            {
                LevelManager.Instance.QuestItemCollected(item);
                itemMessage.ShowMessage(ItemMessage.ItemMessageType.LOOTED);
            }

            looted = true;
        }
    }

    //Guardable
    public Vector3 Position 
    { 
        get 
        { 
            return transform.position; 
        } 
    }

    public bool IsSecure()
    {
        return !looted; //Secure: Item not looted
    }


    //Entity State
    public IState SaveState()
    {
        CustomEntityState state = new CustomEntityState();

        state.looted = looted;

        return state;
    }

    public void LoadState(IState state)
    {
        CustomEntityState entityState = state as CustomEntityState;

        looted = entityState.looted;
        if (looted)
            itemMessage.Dispose();
    }

    [System.Serializable]
    private class CustomEntityState : IState
    {
        [SerializeField] public bool looted;
    }
}
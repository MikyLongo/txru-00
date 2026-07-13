/*
 * Class defining the Equip functionality, offering the following:
 * - Method: Use – Implements the usage logic (default behavior: does nothing, but can be overridden).
 * - Usage Type: Specifies the type of usage (refer to the Item Scriptable Object).
 * - AttachInfo: Provides details about who can use it and the body part where it will be attached.
 */
using UnityEngine;

public class Equip : MonoBehaviour
{
    [SerializeField] protected AttachInfo[] attachmentsInfo;
    [SerializeField] protected Item.UseType useType;

    public virtual void Use()
    {
        //Do nothing!
    }

    public virtual bool WearableBy(Player.PlayerType playerType)
    {
        foreach (AttachInfo attach in attachmentsInfo)
        {
            if(attach.UsableBy == playerType) 
                return true;
        }

        return false;
    }

    public virtual AttachInfo? GetAttachInfo(Player.PlayerType playerType)
    {
        foreach (AttachInfo attach in attachmentsInfo)
        {
            if (attach.UsableBy == playerType)
                return attach;
        }

        return null;
    }

    public Item.UseType GetUseType() { return useType; }

    [System.Serializable]
    public struct AttachInfo
    {
        [SerializeField] private Player.PlayerType usableBy; //Specifies the type of player allowed to equip the weapon.
        [SerializeField] private Player.PlayerPart part;     //Player part 
        [SerializeField] private Vector3 position; 
        //Defines the relative position for the attachment.
        //The player will determine the position and part to understand where it should be attached.

        public AttachInfo(Player.PlayerType usableBy, Player.PlayerPart part, Vector3 position)
        {
            this.usableBy = usableBy;
            this.part = part;
            this.position = position;
        }

        public Player.PlayerType UsableBy { get { return usableBy; } }
        public Player.PlayerPart Part { get { return part; } }
        public Vector3 Position { get { return position; } }
    }
}

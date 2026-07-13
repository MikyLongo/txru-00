//A custom Item type that defines an item providing invisibility.
using UnityEngine;

[CreateAssetMenu(fileName = "NewInvItem", menuName = "Items/InvisibilityItem")]
public class InvisibilityItem : Item
{
    public float duration;
}

//ScriptableObject (SO) used to define the audio clip to play when looting an item.

using UnityEngine;

[CreateAssetMenu(menuName = "Sound/AudioMapper/ItemLooting", fileName = "NewItemLootingAudioMap", order = 2)]
public class ItemLootingAudioMapper : ScriptableObject
{
    public AudioClip looted;
    public AudioClip notLooted;
}

/*
 * ScriptableObject (SO) used to define the audio clip to play for a Player action.
 * Each SO file is designated for a specific Player form or interaction with the environment.
 */

using UnityEngine;

[CreateAssetMenu(menuName = "Sound/AudioMapper/Action", fileName = "NewActionAudioMap", order = 1)]
public class ActionAudioMapper : ScriptableObject
{
    public AudioClip walk;
    public AudioClip jumpLand;
    public AudioClip death;
}

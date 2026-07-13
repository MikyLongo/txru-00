/*
 * ScriptableObject (SO) used to define the audio clip to play for a Enemy action.
 * Each SO file is designated for a specific Enemy form or interaction with the environment.
 */

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Sound/AudioMapper/Enemy", fileName = "NewEnemyAudioMap", order = 3)]
public class EnemyAudioMapper : ScriptableObject
{
    public AudioClip spottedPlayer;
    public List<AudioClip> shootingSounds;
    public List<AudioClip> movements;
    public List<AudioClip> getKilled;
}

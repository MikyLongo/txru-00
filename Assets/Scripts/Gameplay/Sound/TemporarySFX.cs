/*
 * Script attached to a prefab with an AudioSource that is instantiated to play an SFX and then destroyed.
 */
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TemporarySFX : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if(audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlaySFX(AudioClip clip, bool _3d = true, float minDistance = 1f, float maxDistance = 10f, AudioRolloffMode rolloffMode = AudioRolloffMode.Linear)
    {
        audioSource.clip = clip;

        if(_3d)
        {
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.rolloffMode = rolloffMode;
        }
        else
        {
            audioSource.spatialBlend = 0f;
        }

        audioSource.volume = SoundManager.Instance.GetSoundEffectsVolume();
        audioSource.Play();

        Destroy(gameObject,clip.length);
    }
    

}

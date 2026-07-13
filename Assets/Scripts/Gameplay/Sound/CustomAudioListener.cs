/*
 * Defines a custom AudioListener that follows the player instead of the camera, always maintaining the world's 
 * forward direction.
 * Note: The player is represented by different GameObjects depending on whether it is a box or a bot. When the 
 * player transforms from Bot to Box for the first time in the scene, both GameObjects (PlayerBot and PlayerBox) 
 * will exist, even if the player returns to the Bot form. With this setup, there will always be only one 
 * AudioListener.
 */

using UnityEngine;

[RequireComponent(typeof(AudioListener))]
public class CustomAudioListener : MonoBehaviour
{
    [SerializeField] private AudioListener audioListener;

    private void Awake()
    {
        audioListener = GetComponent<AudioListener>();
    }

    void Update()
    {
        if(PlayerManager.Instance.Player is Player player)
        {
            transform.position = player.transform.position;
        }
    }
}

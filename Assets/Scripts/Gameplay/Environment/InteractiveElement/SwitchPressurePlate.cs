/*
 * Defines a pressure plate representing a switch that triggers a UnityEvent when pressed.
 * Upon contact, the pressure plate changes its state from not pressed to pressed and remains pressed.
 * The switch has a red "glowing" color when not pressed, and its color changes to a "glowing" green when 
 * pressed.
 */

using UnityEngine;
using UnityEngine.Events;

public class SwitchPressurePlate : MonoBehaviour, IStateSaveable
{
    [SerializeField] private Light dirLight;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color pressedColor = Color.green;
    [SerializeField] private Color unpressedColor = Color.red;
    [SerializeField] private bool pressed = false;
    [SerializeField] private UnityEvent eventResponse;
    [SerializeField] private AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (pressed)
            return;

        if(EngineConf.Tag.IsEntitiesBody(other.tag))
        {
            pressed = true;
            eventResponse?.Invoke();
            dirLight.color = pressedColor;
            meshRenderer.material.color = pressedColor;
            audioSource.volume = SoundManager.Instance.GetSoundEffectsVolume();
            audioSource.Play();
        }
    }

    //Entity State
    public IState SaveState()
    {
        CustomEntityState state = new CustomEntityState();
        state.pressed = pressed;
        return state;
    }

    public void LoadState(IState state)
    {
        CustomEntityState _state = state as CustomEntityState;
        pressed = _state.pressed;

        if(pressed)
        {
            dirLight.color = pressedColor;
            meshRenderer.material.color = pressedColor;
        }
    }

    [System.Serializable]
    private class CustomEntityState : IState
    {
        [SerializeField] public bool pressed;
    }
}

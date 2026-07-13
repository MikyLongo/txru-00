/*
 * Used in the Main Menu prefab to display the keys for interacting with the UI.
 * Note: If a gamepad is connected, it takes priority over the keyboard and displays its keys instead.
 */
using TMPro;
using UnityEngine;
using GameData;

public class UIInputHelp : MonoBehaviour
{
    [SerializeField] private TMP_Text up;
    [SerializeField] private TMP_Text down;
    [SerializeField] private TMP_Text left;
    [SerializeField] private TMP_Text right;
    [SerializeField] private TMP_Text submit;
    [SerializeField] private TMP_Text menu;

    private void OnEnable()
    {
        GameInput.DeviceType device = GameInput.DeviceType.Keyboard;

        if (GameInput.IsGamepadConnected())
            device = GameInput.DeviceType.Gamepad;


        up.text = GameInput.GetHumanReadableString(GameInput.GetForwardUIBinding(device));
        down.text = GameInput.GetHumanReadableString(GameInput.GetBackwardUIBinding(device));
        left.text = GameInput.GetHumanReadableString(GameInput.GetLeftUIBinding(device));
        right.text = GameInput.GetHumanReadableString(GameInput.GetRightUIBinding(device));
        submit.text = GameInput.GetHumanReadableString(GameInput.GetSubmitUIBinding(device));
        menu.text = GameInput.GetHumanReadableString(GameInput.GetPauseUIBinding(device));
    }
}

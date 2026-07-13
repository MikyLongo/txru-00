/*
 * Script that handles the overlay.
 * - Calculates and displays FPS (if required).
 * - Shows notification messages to the player (currently device connected/disconnected).
 */

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class UIOverlay : MonoBehaviour
{
    //Notification Message:
    [SerializeField] private RectTransform msgContainer;
    [SerializeField] private RectTransform fakeContainer;
    [SerializeField] private MessageElement[] tMessages; // Represents a rotating list
    [SerializeField] private int firstElement = 0;
    [SerializeField] private float msgTimer = 5f;
    [SerializeField] private Color normalMsgColor;
    [SerializeField] private Color okMsgColor;
    [SerializeField] private Color warningMsgColor;
    [SerializeField] private Color errorMsgColor;
    //FPS:
    [SerializeField] private TMP_Text tFPS;
    private Coroutine fpsCoroutine = null;

    //Localization:
    public static readonly string LK_DEVICE_CONNECTED = "UI_OL_DeviceConnected";
    public static readonly string LK_DEVICE_DISCONNECTED = "UI_OL_DeviceDisconnected";
    public static readonly string LK_DEVICE_RECONNECTED = "UI_OL_DeviceReconnected";

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;

        DisposeAllMessageElement();
        DisposeFPSCoroutine();
    }


    //DISPLAY NOTIFICATION MESSAGE
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if(device is Keyboard || device is Gamepad)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    ShowMessage(LocalizationHelper.UI_SMART, LK_DEVICE_CONNECTED,
                        new Dictionary<string, IVariable>() { { "name", new StringVariable() { Value = device.displayName } } }, 
                        okMsgColor);
                break;

                /*
                 * Checking InputDeviceChange.Reconnected isn't possible because when it is reconnected,
                 * the system triggers two events: InputDeviceChange.Added and InputDeviceChange.Reconnected,
                 * which results in duplicate messages.
                 */

                /*
                case InputDeviceChange.Reconnected:
                    ShowMessage(LocalizationHelper.UI_SMART, LK_DEVICE_RECONNECTED,
                        new Dictionary<string, IVariable>() { { "name", new StringVariable() { Value = device.displayName } } },
                        okMsgColor);
                break;
                */

                case InputDeviceChange.Removed:
                    ShowMessage(LocalizationHelper.UI_SMART, LK_DEVICE_DISCONNECTED,
                        new Dictionary<string, IVariable>() { { "name", new StringVariable() { Value = device.displayName } } },
                        warningMsgColor);
                    break;
            }
        }
    }

    /*
     * Utilizes a rotating array to display messages!
     * - The first element of the array represents either an unused or a used message.
     * - If the first element is used, it indicates that all messages have been displayed. In this case, the oldest
     *   message will be updated.
     * - Rotating array logic: The first element is used, then moved to the end of the array.
     *   => The first element is either unused or the oldest displayed message.
     *   => The last element is either unused or the newest displayed message.
     * To determine the first element of the array, the index "firstElement" is used.
     */

    private void ShowMessage(string table, string entry, Dictionary<string, IVariable> parameters, Color color)
    {
        DisposeMessageElement(tMessages[firstElement]);

        TMP_Text msg = tMessages[firstElement].tMsg;

        //Move it to be the last message displayed!
        msg.transform.SetParent(fakeContainer,false);
        msg.transform.SetParent(msgContainer, false);

        //Updates text
        LocalizedString locString = msg.gameObject.GetLocalizedString();

        if (locString == null)
            return;

        if (parameters == null) //Simple localized string
            msg.gameObject.UpdateLocalizeStringEvent(table, entry).Clear(); //Clears old parameters
        else
        {
            msg.gameObject.UpdateLocalizeStringEvent(table, entry);
            msg.gameObject.UpdateSmartString(parameters);
        }

        msg.color = color;

        //Rotates the element and starts the timer coroutine
        MessageElement elem = tMessages[firstElement];

        firstElement++;
        if (firstElement == tMessages.Length)
            firstElement = 0;

        elem.coroutine = StartCoroutine(MsgCoroutine(elem));
    }

    private IEnumerator MsgCoroutine(MessageElement elem)
    {
        elem.tMsg.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(msgTimer);
        elem.tMsg.gameObject.SetActive(false);
        elem.coroutine = null;
    }

    private void DisposeMessageElement(MessageElement elem)
    {
        if (elem.coroutine != null)
        {
            StopCoroutine(elem.coroutine);
            elem.coroutine = null;
        }

        elem.tMsg.gameObject.SetActive(false);
    }

    private void DisposeAllMessageElement()
    {
        for(int i=0; i<tMessages.Length; i++)
            DisposeMessageElement(tMessages[i]);

        firstElement = 0;
    }

    //SHOW FPS
    public void ShowFPS(bool show)
    {
        tFPS.gameObject.SetActive(show);
        DisposeFPSCoroutine();
        if (show)
        {
            fpsCoroutine = StartCoroutine(FPSCoroutine());
        }
    }

    private IEnumerator FPSCoroutine()
    {
        float t = 0f;
        while(true)
        {
            t += (Time.unscaledDeltaTime - t) * 0.1f; //Uses an exponential filter to stabilize the value
            float fps = 1.0f / t;
            tFPS.text = string.Format("FPS: {0:0.}", fps);
            yield return null;
        }
    }

    private void DisposeFPSCoroutine()
    {
        if(fpsCoroutine != null)
        {
            StopCoroutine(fpsCoroutine);
            fpsCoroutine = null;
        }
    }


    [Serializable]
    public class MessageElement
    {
        [SerializeField] public TMP_Text tMsg;
        public Coroutine coroutine = null;
    }
}

/*
 * Script that manages a modal message box.
 * The message box is assigned one of three types: Generic, Warning, or Error.
 * The type influences the color of the title (if displayed).
 * 
 * The message box must have a message. It can also include:
 * - A title.
 * - A close button, where additional behavior can be defined by another script.
 * - An extra button, with behavior defined by another script.
 * - A timer for automatic closure of the message box.
 * - A background screen behind the message box to indicate that interaction with other elements is blocked.
 * - A value specifying which action (close button or extra button) to invoke when an external script
 *   requests a forced action.
 */

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMessageBox : MonoBehaviour, IModalUI
{
    [SerializeField] private TMP_Text t_title;
    [SerializeField] private TMP_Text t_message;
    [SerializeField] private TMP_Text t_timer;
    [SerializeField] private GameObject timerBG;
    [SerializeField] private GameObject panelBlockBG;
    [SerializeField] private Button bButton1;
    [SerializeField] private Button bClose;
    [SerializeField] private Color colorGeneric = Color.white;
    [SerializeField] private Color colorWarning = Color.yellow;
    [SerializeField] private Color colorError = Color.red;
    private bool closed = true;
    private int forceAction = 2;

    private GameObject firstSelected = null; 
    
    private Coroutine timerCoroutine;

    public enum MsgType
    {
        Generic,
        Warning,
        Error
    }

    /*
     * title: Can be null or ""
     * message: Can't be null or ""
     * textButton1: If null or "" there is no button1, otherwise it require action1 != null
     * action1: If there is no button1 it will be ignored
     * withCloseButton: 
     *  - true: display the close button
     *  - false: don't display the close button
     *  Note: if no buttons are used and timer is not set then the message box will be permanent!
     * action2: Get executed by close button and timer (exclusively). Can be null.
     * closeAfter: If <= 0 means message without timer
     * type: The type of message box (generic, warning, error)
     * blockBG: If true block the background behind the message from receiving input
     * forceAction: When a request to force action is made do: 0 do nothing, 1 execute button1, 2 execute button2
     * showTimer: Is valid only when closeAfter is > 0! true = show the timer, false = the timer is active but not showed.
     */
    /*
     * Parameters:
     * - title: Can be null or an empty string ("").
     * - message: Cannot be null or an empty string ("").
     * - textButton1: If null or an empty string (""), no button1 is displayed; otherwise, action1 must not be null.
     * - action1: Ignored if button1 is not displayed.
     * - withCloseButton: 
     *    - true: Displays the close button.
     *    - false: Does not display the close button.
     *    Note: If no buttons are displayed and no timer is set, the message box will be permanent.
     * - action2: Executed exclusively by the close button or the timer. Can be null.
     * - closeAfter: If <= 0, the message box will have no timer.
     * - type: The type of the message box (Generic, Warning, Error).
     * - blockBG: If true, blocks the background behind the message box from receiving input.
     * - forceAction: Specifies the forced action when requested:
     *    - 0: Do nothing.
     *    - 1: Execute button1.
     *    - 2: Execute button2.
     * - showTimer: Only valid when closeAfter > 0.
     *    - true: Displays the timer.
     *    - false: The timer is active but not displayed.
     */
    public void Open(string title, string message, string textButton1 = null, Action action1 = null, bool withCloseButton = true, Action action2 = null, float closeAfter = 0, MsgType type = MsgType.Generic, bool blockBG = true, int forceAction = 2, bool showTimer = false)
    {
        //Checks for Errors
        if(string.IsNullOrEmpty(message)) 
        {
            Debug.LogError("MessageBox cannot be displayed without a message!\r\n");
            return;
        }

        closed = false;

        //Message
        t_message.text = message;

        //PanelBlockBG
        panelBlockBG.SetActive(blockBG);

        //Title
        if(string.IsNullOrEmpty(title)) 
        { 
            t_title.text = ""; 
        }
        else
        {
            t_title.text = title;
            switch (type)
            {
                case MsgType.Generic:
                    t_title.color = colorGeneric;
                break;

                case MsgType.Warning:
                    t_title.color = colorWarning;
                break;

                case MsgType.Error:
                    t_title.color = colorError;
                break;
            }
        }

        firstSelected = null;

        //Button1
        if(string.IsNullOrEmpty(textButton1) || action1 == null)
        {
            bButton1.gameObject.SetActive(false);
            if(forceAction == 1)
                forceAction = 2; // No button1 available => Force action will default to button2
        }
        else
        {
            firstSelected = bButton1.gameObject;
            bButton1.SetText(textButton1); //Extension method
            bButton1.gameObject.SetActive(true);
            bButton1.onClick.AddListener(() =>
            {
                Close();
                action1();
            });
        }

        //Close Button
        if (withCloseButton)
        {
            if (firstSelected == null) //Only applies if button1 is not used
                firstSelected = bClose.gameObject;

            bClose.gameObject.SetActive(true);
            if (action2 == null)
                bClose.onClick.AddListener(Close);
            else
                bClose.onClick.AddListener(() =>
                {
                    Close();
                    action2();
                });
        }
        else
        {
            bClose.gameObject.SetActive(false);
            if (forceAction == 2)
                forceAction = 0; //No button2 available -> No force action will be executed
        }

        this.forceAction = forceAction;

        //Notifies the opening of the MessageBox to update the event system (focus gained)
        UIManager.Instance.EventSystemHandler.ModalRequestFocus(firstSelected, true);
        UIManager.Instance.OpenedModal = this;
        //Displays the message box
        gameObject.SetActive(true);

        //Timer 
        TimerDispose();
        if (closeAfter > 0) //Starts the timer
            timerCoroutine = StartCoroutine(Timer(closeAfter, action2, showTimer));
    }

    public void Close()
    {
        if (!closed)
        {
            //Notifies the UIManager of the MessageBox closure to update the event system (focus lost)
            UIManager.Instance.EventSystemHandler.ModalRequestFocus(null, false);
            UIManager.Instance.OpenedModal = null;
            closed = true;
            gameObject.SetActive(false);
        }
    }

    private void ForceAction()
    {
        if (!closed)
        {
            if(forceAction == 1)
            {
                bButton1.onClick.Invoke();
            }
            else if(forceAction == 2)
            {
                bClose.onClick.Invoke();
            }
        }
    }

    private void OnDisable()
    {
        bButton1.onClick.RemoveAllListeners();
        bClose.onClick.RemoveAllListeners();

        TimerDispose();
    }

    //Coroutine
    private IEnumerator Timer(float time, Action action, bool showTimer)
    {
        float t = 0f;

        if(showTimer)
        {
            string format = "00";

            if (time >= 100)
                format = "000";

            t_timer.text = time.ToString(format);
            timerBG.SetActive(true);

            while (t < time)
            {
                yield return 0;
                t += Time.unscaledDeltaTime;
                t_timer.text = Mathf.Ceil(time - t).ToString(format);
            }

            timerBG.SetActive(false);
        }
        else
        {
            while (t < time)
            {
                yield return 0;
                t += Time.unscaledDeltaTime;
            }
        }

        timerCoroutine = null;

        Close(); 
        
        if (action != null)
            action();
    }

    private void TimerDispose()
    {
        if(timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
            timerBG.SetActive(false);
        }
    }

    //Implementation of the IModalUI interface
    public void OnPauseModal()
    {
        ForceAction();
    }

    //Localization strings

    //UI_MESSAGE
    public static readonly string LK_CLOSING_GAME_T = "MBClosingGameTitle";
    public static readonly string LK_CLOSING_GAME_AC = "MBClosingGameAskConfirm";
    public static readonly string LK_CHANGE_N_SAVED_T = "MBChangeNotSavedTitle";
    public static readonly string LK_CHANGE_N_SAVED_AC = "MBChangeNotSavedAskConfirm";
    public static readonly string LK_ERROR_OCCURRED = "MBErrorOccurred";
    public static readonly string LK_SAVE_SUCCESSFULL = "MBSaveSuccesfull";
    public static readonly string LK_KEYBIND_ALREADY_USED_T = "MBKeybindAlreadyUsedTitle";
    public static readonly string LK_SLOT_HAS_DATA = "SDSlotHasData";

    //UI_SMART
    public static readonly string LK_ASK_TO_PRESS_KEY = "MBAskToPressKey";
    public static readonly string LK_KEYBIND_ALREADY_USED_TEXT = "MBKeybindAlreadyUsedText";

}

/*
 * Handler for the EventSystem.
 * Determines which UI container and UI element should have focus during the opening or closing of 
 * an IPageUI or IModalUI.
 * IModalUI overlaps IPageUI and takes priority.
 */

using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class UIEventSystemHandler
{
    [SerializeField] private EventSystem eventSystem = null;
    [SerializeField] private GameObject pageFirstSelected = null;
    [SerializeField] private GameObject oldSelected = null;
    //oldSelected refers to the last element selected in the PageUI before opening a ModalUI.
    [SerializeField] private bool modalRequested = false;

    public UIEventSystemHandler(EventSystem eventSystem)
    {
        this.eventSystem = eventSystem;
        Init();
    }

    private void Init()
    {
        pageFirstSelected = null;
        oldSelected = null;
        modalRequested = false;
    }

    /*
     * selectedObj: The UI element that will gain focus (ignored if the request is false).
     * request: 
     * -    true    => A modal is opening and/or requesting focus.
     * -    false   => A modal is closing and/or losing focus.
     */
    public void ModalRequestFocus(GameObject selectedObj, bool request)
    {
        if(request) //Gain focus
        {
            modalRequested = true;
            oldSelected = eventSystem.currentSelectedGameObject; 
            eventSystem.SetSelectedGameObject(selectedObj);
        }
        else
        {
            if(oldSelected == null)
                eventSystem.SetSelectedGameObject(pageFirstSelected);
            else
                eventSystem.SetSelectedGameObject(oldSelected);

            modalRequested = false;
        }
    }

    public void PageOpened(GameObject selectedObj) 
    {
        pageFirstSelected = selectedObj;
        oldSelected = null;

        if(!modalRequested) //If no modal windows are requesting focus, give focus to the opened page.
        {
            eventSystem.SetSelectedGameObject(selectedObj);
        }
    }

    public void UpdateSelection(GameObject selectedObj) //Used within IPageUI to change the focused UI element.
    {
        if (modalRequested) 
        {
            //If the modal is open, the selected object will only become available once the modal is closed,
            //so it is stored as oldSelected.
            oldSelected = selectedObj;
        }
        else
        {
            oldSelected = eventSystem.currentSelectedGameObject;
            eventSystem.SetSelectedGameObject(selectedObj);
        }
    }
}

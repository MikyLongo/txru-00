using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Dropdown), typeof(UICustomElement))]
public class UICustomDropDown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown ddUI = null;
    [SerializeField] private UICustomElement ddEvents = null;
    [SerializeField] private UICustomScrollView scrollView = null;
    [SerializeField] private List<UICustomElement> options = null;

    private void Awake()
    {
        ddUI = GetComponent<TMP_Dropdown>();
        ddEvents = GetComponent<UICustomElement>();
    }

    private void OnEnable()
    {
        ddEvents.onSubmit.AddListener(OnDDSubmit);
    }

    private void OnDisable()
    {
        ddEvents.onSubmit.RemoveAllListeners();
    }

    private void OnDDSubmit(BaseEventData eventData)
    {
        //Retrieves the ScrollView 
        scrollView = ddUI.transform.GetComponentInChildren<UICustomScrollView>(false);

        //Retrieves options as UICustomElement
        scrollView.GetComponent<ScrollRect>().content.GetComponentsInChildren<UICustomElement>(false, options);

        //Points to the currently selected option
        scrollView.DDScrollY(ddUI.value, options.Count);

        //Adds listeners to the Move event
        foreach (UICustomElement element in options) 
        {
            element.onMove.AddListener(OnOptionMove);
        }
    }

    private void OnOptionMove(AxisEventData eventData)
    {
        int index = options.FindIndex(x => x == eventData.selectedObject.transform.GetComponent<UICustomElement>());

        if(index > -1)
            scrollView.DDScrollY(index, options.Count);
    }
}

/*
 * Script to attach to UI elements to extend their functionality by enabling the handling of events
 * that would otherwise require creating a custom class for each element to achieve extended functionality.
 */

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UICustomElement : MonoBehaviour, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler
{
    [SerializeField] private UICustomElementEvent<BaseEventData> _onSubmit;
    [SerializeField] private UICustomElementEvent<BaseEventData> _onCancel;
    [SerializeField] private UICustomElementEvent<BaseEventData> _onSelect;
    [SerializeField] private UICustomElementEvent<BaseEventData> _onDeselect;
    [SerializeField] private UICustomElementEvent<AxisEventData> _onMove;


    public UICustomElementEvent<BaseEventData> onSubmit { get => _onSubmit; set => _onSubmit = value; }
    public UICustomElementEvent<BaseEventData> onCancel { get => _onCancel; set => _onCancel = value; }
    public UICustomElementEvent<BaseEventData> onSelect { get => _onSelect; set => _onSelect = value; }
    public UICustomElementEvent<BaseEventData> onDeselect { get => _onDeselect; set => _onDeselect = value; }
    public UICustomElementEvent<AxisEventData> onMove { get => _onMove; set => _onMove = value; }

    public void OnSubmit(BaseEventData eventData)
    {
        onSubmit?.Invoke(eventData);
    }

    public void OnCancel(BaseEventData eventData)
    {
        onCancel?.Invoke(eventData);
    }

    public void OnSelect(BaseEventData eventData)
    {
        onSelect?.Invoke(eventData);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        onDeselect?.Invoke(eventData);
    }

    public void OnMove(AxisEventData eventData)
    {
        onMove?.Invoke(eventData);
    }
}

[System.Serializable]
public class UICustomElementEvent<T> : UnityEvent<T> { }
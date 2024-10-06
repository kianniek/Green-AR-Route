using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonPressRelease : MonoBehaviour, IPointerDownHandler, IPointerClickHandler,
    IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
{
    
    [SerializeField] private UnityEvent<PointerEventData> onPointerDown;
    [SerializeField] private UnityEvent<PointerEventData> onPointerClick;
    [SerializeField] private UnityEvent<PointerEventData> onPointerUp;
    [SerializeField] private UnityEvent<PointerEventData> onPointerExit;
    [SerializeField] private UnityEvent<PointerEventData> onPointerEnter;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown.Invoke(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onPointerClick.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onPointerUp.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExit.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnter.Invoke(eventData);
    }
}

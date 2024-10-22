using UnityEngine;
using UnityEngine.Events;

public class EventTriggerOneTime : MonoBehaviour
{
    public OneTimeEvent oneTimeEvent;
    public UnityEvent onEventTriggered;
    private void Start()
    {
        if (oneTimeEvent != null)
        {
            TriggerEvent();
        }
    }
    
    public void TriggerEvent()
    {
        if (!oneTimeEvent.hasBeenTriggered)
        {
            oneTimeEvent.hasBeenTriggered = true;
            onEventTriggered?.Invoke();
        }
    }
}
using UnityEngine;
using FMODUnity;

public class FMODEmitterController : MonoBehaviour
{
    private StudioEventEmitter eventEmitter;
    private FMODStudioEventQueueManager queueManager;

    void Start()
    {
        eventEmitter = GetComponent<StudioEventEmitter>();
        queueManager = FindObjectOfType<FMODStudioEventQueueManager>();
    }

    void Update()
    {
        if (queueManager != null && eventEmitter != null)
        {
            if (eventEmitter.IsPlaying() && queueManager.IsEventCrossing(eventEmitter))
            {
                // Pause the event and queue it for later
                eventEmitter.Stop();
                queueManager.QueueEvent(eventEmitter);
            }
        }
    }

    public void PlayEvent()
    {
        if (queueManager != null && eventEmitter != null)
        {
            queueManager.QueueEvent(eventEmitter);
        }
    }
}
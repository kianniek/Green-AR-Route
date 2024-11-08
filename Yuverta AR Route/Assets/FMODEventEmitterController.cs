using UnityEngine;
using FMODUnity;

public class FMODEmitterController : MonoBehaviour
{
    private StudioEventEmitter eventEmitter;
    private FMODStudioEventQueueManager queueManager;

    public bool playOnAwake = false;
    public bool playOnStart = false;
    public bool playOnEnable = false;
    public bool playOnDisable = false;

    void Start()
    {
        eventEmitter = GetComponent<StudioEventEmitter>();
        queueManager = FindObjectOfType<FMODStudioEventQueueManager>();

        if (playOnStart)
        {
            PlayEvent();
        }
    }

    void OnEnable()
    {
        if (playOnEnable)
        {
            PlayEvent();
        }
    }

    void OnDisable()
    {
        if (playOnDisable)
        {
            PlayEvent();
        }
    }

    void Awake()
    {
        if (playOnAwake)
        {
            PlayEvent();
        }
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
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODStudioEventQueueManager : MonoBehaviour
{
    private Queue<StudioEventEmitter> eventQueue = new Queue<StudioEventEmitter>();
    private StudioEventEmitter currentEventEmitter;
    private bool isPlaying = false;
    
    public bool QueueEmpty => eventQueue.Count == 0;
    public bool IsPlaying => isPlaying;

    void Update()
    {
        // Check if the current event has finished playing
        if (isPlaying && !IsEventPlaying())
        {
            isPlaying = false;
            PlayNextEventInQueue();
        }
#if DEBUG
        //print the queue in one long string
        string queueString = "";
        foreach (StudioEventEmitter eventEmitter in eventQueue)
        {
            queueString += eventEmitter.name + ", ";
        }
        Debug.Log("Queue: " + queueString);
#endif
    }

    public void QueueEvent(StudioEventEmitter eventEmitter)
    {
        //check if event is already in the queue
        if (!eventQueue.Contains(eventEmitter) || currentEventEmitter != eventEmitter)
        {
            eventQueue.Enqueue(eventEmitter);
        }
        

        if (!isPlaying)
        {
            PlayNextEventInQueue();
        }
    }

    private void PlayNextEventInQueue()
    {
        if (eventQueue.Count > 0)
        {
            currentEventEmitter = eventQueue.Dequeue();
            currentEventEmitter.Play();
            isPlaying = true;
        }
    }

    private bool IsEventPlaying()
    {
        if (currentEventEmitter != null)
        {
            return currentEventEmitter.IsPlaying();
        }
        return false;
    }

    public bool IsEventCrossing(StudioEventEmitter eventEmitter)
    {
        // Check if the current event is crossing over except for itself    
        return currentEventEmitter != null && currentEventEmitter.IsPlaying() && currentEventEmitter != eventEmitter;
    }
    
    public void SetCurrentEventEmitter(StudioEventEmitter eventEmitter)
    {
        currentEventEmitter = eventEmitter;
    }
}
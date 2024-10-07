using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public bool stopEventsOnLoad = true;

    private void OnEnable()
    {
        StopAllEvents();
    }

    public void StopAllEvents()
    {
        // Get the master bus
        Bus masterBus = RuntimeManager.GetBus("bus:/");
        
        // Stop all events
        masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        
        Debug.Log("All events stopped");
    }
}
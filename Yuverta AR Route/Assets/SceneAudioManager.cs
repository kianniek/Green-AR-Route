using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SceneAudioManager : MonoBehaviour
{
    private Bus masterBus;

    void Awake()
    {
        // Get the master bus (master channel group)
        masterBus = RuntimeManager.GetBus("bus:/");
    }

    public void StopAllSounds()
    {
        // Stop all sounds on the master bus
        masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); // Use STOP_MODE.IMMEDIATE to stop instantly
    }

    private void OnDestroy()
    {
        // Stop all sounds on the master bus when the object is destroyed
        StopAllSounds();
    }
}
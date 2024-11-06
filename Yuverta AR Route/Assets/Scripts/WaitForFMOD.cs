using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity; // FMOD Unity Integration namespace
using FMOD.Studio;
using UnityEngine.Events; // FMOD Studio API namespace

public class WaitForFMOD : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter eventInstance;

    // UnityEvents that can be assigned in the Unity Editor
    public UnityEvent OnEventStarted = new();
    public UnityEvent OnEventStopped = new();
    public UnityEvent OnEventPlaying = new();

    private bool hasStartedPlaying;
    private bool hasStoppedPlaying;

    private void Awake()
    {
        // Initialize the flag to true
        hasStoppedPlaying = true;
        hasStartedPlaying = false;
    }

    private void Start()
    {
        if (eventInstance == null)
        {
            eventInstance = GetComponent<StudioEventEmitter>();
        }
    }

    private void Update()
    {
        if (eventInstance.IsPlaying())
        {
            if (!hasStartedPlaying)
            {
                hasStartedPlaying = true;
                hasStoppedPlaying = false;

                OnEventStarted.Invoke(); // Trigger Unity event
            }
            else
            {
                OnEventPlaying.Invoke(); // Trigger Unity event
            }
        }
        else if (!hasStoppedPlaying)
        {
            hasStartedPlaying = false;
            hasStoppedPlaying = true;

            OnEventStopped.Invoke(); // Trigger Unity event
        }
    }

    public void Reset()
    {
        hasStartedPlaying = false;
        hasStoppedPlaying = false;
    }
    
    public void PlayEvent()
    {
        eventInstance.Play();
    }
    
    public void StopEvent()
    {
        eventInstance.Stop();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class FmodParameterController : MonoBehaviour
{
    public EventReference eventReference;
    public string parameterName = "";
    [SerializeField] int initialParameterValue = 0;
    
    FMOD.Studio.EventInstance eventState;

    int parameterValue;
    FMOD.Studio.PARAMETER_ID parameterID;

    void Start()
    {
        eventState = FMODUnity.RuntimeManager.CreateInstance(eventReference);
        eventState.start();

        FMODUnity.RuntimeManager.AttachInstanceToGameObject(eventState, transform);

        FMOD.Studio.EventDescription eventDescription;
        eventState.getDescription(out eventDescription);
        FMOD.Studio.PARAMETER_DESCRIPTION parameterDescription;
        eventDescription.getParameterDescriptionByName(parameterName, out parameterDescription);
        parameterID = parameterDescription.id;
        SetParameterValue(initialParameterValue);
    }

    void OnDestroy()
    {
        eventState.release();
        //stop the events when the object is disabled allow the event to fade out
        eventState.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
    
    void OnDisable()
    {
        //stop the events when the object is disabled allow the event to fade out
        eventState.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    void OnEnable()
    {
        eventState.start();
    }
    
    public void SetParameterToNextValue()
    {
        FMOD.Studio.EventDescription eventDescription;
        eventState.getDescription(out eventDescription);
        FMOD.Studio.PARAMETER_DESCRIPTION parameterDescription;
        eventDescription.getParameterDescriptionByName(parameterName, out parameterDescription);
        
        eventState.getParameterByID(parameterID, out float preValue);
        Debug.Log($"PreValue {parameterDescription.name.ToString()}: " + preValue);
        parameterValue++;
        eventState.setParameterByID(parameterID, parameterValue);
        
        eventState.getParameterByID(parameterID, out float postValue);
        Debug.Log($"PostValue {parameterDescription.name.ToString()}: " + postValue);
    }
    
    public void SetParameterValue(int value)
    {
        FMOD.Studio.EventDescription eventDescription;
        eventState.getDescription(out eventDescription);
        FMOD.Studio.PARAMETER_DESCRIPTION parameterDescription;
        eventDescription.getParameterDescriptionByName(parameterName, out parameterDescription);
        
        eventState.getParameterByID(parameterID, out float preValue);
        Debug.Log($"PreValue {parameterDescription.name.ToString()}: " + preValue);
        parameterValue = value;
        eventState.setParameterByID(parameterID, parameterValue);
        
        eventState.getParameterByID(parameterID, out float postValue);
        Debug.Log($"PostValue {parameterDescription.name.ToString()}: " + postValue);
    }
}
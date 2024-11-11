using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FmodParameterController : MonoBehaviour
{
    public EventReference eventReference;
    public string parameterName = "";
    [SerializeField] int initialParameterValue = 0;

    FMOD.Studio.EventInstance eventState;

    float parameterValue;
    FMOD.Studio.PARAMETER_ID parameterID;

    [Tooltip("If the parameter is not 0 to 1, set the scale to convert the value")]
    public int progressScale = 1;

    //stop the event when an other bank is unloaded
    [SerializeField] bool stopOnOtherBankUnload = false;
    [SerializeField, BankRef] string otherBankName;
    private bool otherBankVolume = true;


    void Start()
    {
        eventState = RuntimeManager.CreateInstance(eventReference);
        eventState.start();
        RuntimeManager.AttachInstanceToGameObject(eventState, transform);

        EventDescription eventDescription;
        eventState.getDescription(out eventDescription);
        PARAMETER_DESCRIPTION parameterDescription;
        eventDescription.getParameterDescriptionByName(parameterName, out parameterDescription);
        parameterID = parameterDescription.id;
        SetParameterValue(initialParameterValue);

        if (stopOnOtherBankUnload)
        {
            StartCoroutine(CheckOtherBankStatus());
        }
    }

    IEnumerator CheckOtherBankStatus()
    {
        while (Application.isPlaying)
        {
            FMODUnity.RuntimeManager.StudioSystem.getBankList(out var loadedBanks);
            var medianVolume = 0f;

            foreach (var bank in loadedBanks)
            {
                bank.getPath(out var path);
                var splitPath = path.Split('/');

                if (splitPath[1] != otherBankName)
                {
                    continue;
                }

                bank.getEventList(out var eventDescriptions);

                foreach (var eventDesc in eventDescriptions)
                {
                    eventDesc.getPath(out var eventPath);

                    // Mute the event
                    FMODUnity.RuntimeManager.StudioSystem.getEvent(eventPath, out var eventInstance);

                    // Mute the event
                    eventInstance.getInstanceList(out var instances);

                    foreach (var instance in instances)
                    {
                        instance.getVolume(out var volume);
                        medianVolume += volume;
                    }
                }
            }
            
            medianVolume /= loadedBanks.Length;

            eventState.setVolume(Mathf.Clamp01(medianVolume));

            yield return new WaitForSeconds(0.5f); // Check periodically
        }
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
        parameterValue = value * progressScale;
        eventState.setParameterByID(parameterID, (int)parameterValue);

        eventState.getParameterByID(parameterID, out float postValue);
        Debug.Log($"PostValue {parameterDescription.name.ToString()}: " + postValue);
    }

    public void SetParameterValue(float value)
    {
        FMOD.Studio.EventDescription eventDescription;
        eventState.getDescription(out eventDescription);
        FMOD.Studio.PARAMETER_DESCRIPTION parameterDescription;
        eventDescription.getParameterDescriptionByName(parameterName, out parameterDescription);

        eventState.getParameterByID(parameterID, out float preValue);
        Debug.Log($"PreValue {parameterDescription.name.ToString()}: " + preValue);
        parameterValue = value * progressScale;
        eventState.setParameterByID(parameterID, parameterValue);

        eventState.getParameterByID(parameterID, out float postValue);
        Debug.Log($"PostValue {parameterDescription.name.ToString()}: " + postValue);
    }

    public void SetParameterValueFloatToInt(float value)
    {
        SetParameterValue((int)value);
    }
}
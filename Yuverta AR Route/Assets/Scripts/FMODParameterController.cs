using System;
using FMOD;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;
using Debug = UnityEngine.Debug; // Make sure to include this to use FMOD in Unity

public class FMODParameterController : MonoBehaviour
{
    public StudioEventEmitter fmodEventInstance;

    public string parameterName;

    public int customStepTreshold = 1;
    public bool useCustomStepTreshold = false;

    private int stepCounter = 0;
    private int currentStep = 0;

    public void SetFMODParameter(float value)
    {
        // Assume these are the parameter limits from FMOD, can be adjusted as needed
        float minParameterValue = fmodEventInstance.EventInstance.getParameterByName(parameterName, out var min, out var max) == RESULT.OK ? min : 0.0f;
        float maxParameterValue = fmodEventInstance.EventInstance.getParameterByName(parameterName, out min, out max) == RESULT.OK ? max : 1.0f;

        // Cap the value within the allowed limits
        value = Mathf.Clamp(value, minParameterValue, maxParameterValue);

        if (useCustomStepTreshold)
        {
            stepCounter++;
            if (stepCounter < customStepTreshold)
            {
                return;
            }

            stepCounter = 0;
            currentStep++;

            // Ensure currentStep doesn't exceed max value
            currentStep = Mathf.Clamp(currentStep, (int)minParameterValue, (int)maxParameterValue);

            Debug.Log($"Setting parameter {parameterName} to step {currentStep}");

            // Set the parameter and handle errors
            RESULT result = fmodEventInstance.EventInstance.setParameterByName(parameterName, currentStep);
            if (result != RESULT.OK)
            {
                Debug.LogError($"Error setting FMOD parameter {parameterName} to {currentStep}: {result}");
                return;
            }

            // If debugging, retrieve the parameter value and log it
#if DEBUG
            fmodEventInstance.EventInstance.getParameterByName(parameterName, out var parameterValueStep);
            Debug.Log($"Parameter {parameterName} set to {parameterValueStep}");
#endif

            return;
        }

        Debug.Log($"Setting parameter {parameterName} to {value}");

        // Set the parameter and handle errors
        RESULT resultValue = fmodEventInstance.EventInstance.setParameterByName(parameterName, value);
        if (resultValue != RESULT.OK)
        {
            Debug.LogError($"Error setting FMOD parameter {parameterName} to {value}: {resultValue}");
            return;
        }

        // If debugging, retrieve the parameter value and log it
#if DEBUG
        fmodEventInstance.EventInstance.getParameterByName(parameterName, out var parameterValue);
        Debug.Log($"Parameter {parameterName} set to {parameterValue}");
#endif
    }


    public void SetFMODParameter(int value)
    {
        SetFMODParameter((float)value);
    }
}
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

    // This function will be called by the Unity event
    public void SetFMODParameter(float value)
    {
        if (useCustomStepTreshold)
        {
            stepCounter++;
            if (stepCounter < customStepTreshold)
            {
                return;
            }

            stepCounter = 0;
            currentStep++;
            
            Debug.Log($"Setting parameter {parameterName} to {currentStep}");
        
            RESULT result = fmodEventInstance.EventInstance.setParameterByName(parameterName, currentStep);
            fmodEventInstance.SetParameter(parameterName, currentStep);
            Debug.Log($"Set parameter result: {result}");
            fmodEventInstance.EventInstance.getParameterByName(parameterName, out var parameterValueStep);
            Debug.Log($"Parameter {parameterName} set to {parameterValueStep}");
            
            return;
        }
        
        Debug.Log($"Setting parameter {parameterName} to {value}");
        
        fmodEventInstance.EventInstance.setParameterByName(parameterName, value);
        
        fmodEventInstance.EventInstance.getParameterByName(parameterName, out var parameterValue);
        Debug.Log($"Parameter {parameterName} set to {parameterValue}");
        
    }

    public void SetFMODParameter(int value)
    {
        if (useCustomStepTreshold)
        {
            stepCounter++;
            if (stepCounter < customStepTreshold)
            {
                return;
            }

            stepCounter = 0;
            currentStep++;
            
            Debug.Log($"Setting parameter {parameterName} to {currentStep}");
        
            RESULT result = fmodEventInstance.EventInstance.setParameterByName(parameterName, currentStep);
            fmodEventInstance.SetParameter(parameterName, currentStep);

            Debug.Log($"Set parameter result: {result}");
            
            fmodEventInstance.EventInstance.getParameterByName(parameterName, out var parameterValueStep);
            Debug.Log($"Parameter {parameterName} set to {parameterValueStep}");
            
            return;
        }
        
        Debug.Log($"Setting parameter {parameterName} to {value}");
        
        fmodEventInstance.EventInstance.setParameterByName(parameterName, value);
        
        fmodEventInstance.EventInstance.getParameterByName(parameterName, out var parameterValue);
        Debug.Log($"Parameter {parameterName} set to {parameterValue}");
    }
}
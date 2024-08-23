using UnityEngine;
using FMODUnity;  // Make sure to include this to use FMOD in Unity

public class FMODParameterController : MonoBehaviour
{
    public StudioEventEmitter fmodEventInstance;
    public string parameterName;

    // This function will be called by the Unity event
    public void SetFMODParameter(float value)
    {
        fmodEventInstance.SetParameter(parameterName, value);
    }
    
    public void SetFMODParameter(int value)
    {
        fmodEventInstance.SetParameter(parameterName, value);
    }
}
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "OneTimeEvent", menuName = "ScriptableObjects/OneTimeEvent", order = 1)]
public class OneTimeEvent : ScriptableObject
{
  

    public bool hasBeenTriggered = false;

    public void ResetEvent()
    {
        hasBeenTriggered = false;
    }

    void OnApplicationQuit()
    {
        ResetEvent();
    }
}
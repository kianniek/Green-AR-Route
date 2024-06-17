using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Tooltip("Add this script to a button that will be used to answer a quiz question")]
public class QuizButton : MonoBehaviour
{
    public int buttonIndex;
    
    // Define a new UnityEvent that takes an int as a parameter
    [System.Serializable]
    public class IntEvent : UnityEvent<int> { }

    public IntEvent onRaycastHit;

    public void OnClick()
    {
        onRaycastHit.Invoke(buttonIndex);
    }
}

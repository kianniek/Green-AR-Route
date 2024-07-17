using System.Collections;
using System.Collections.Generic;
using Events;
using Events.GameEvents.Typed;
using UnityEngine;
using UnityEngine.Events;

[Tooltip("Add this script to a button that will be used to answer a quiz question")]
public class QuizButton : MonoBehaviour
{
    public int buttonIndex;

    public IntEvent onRaycastHit;

    public void OnClick()
    {
        onRaycastHit.Invoke(buttonIndex);
    }
}

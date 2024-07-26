using System;
using System.Collections;
using System.Collections.Generic;
using Events;
using Events.GameEvents.Typed;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Add this script to a button that will be used to answer a quiz question
/// </summary>

public class QuizButton : MonoBehaviour
{
    public int buttonIndex;

    public IntEvent onRaycastHit;

    private TMP_Text buttonText;
    
    public string ButtonText
    {
        get => buttonText.text;
        set => buttonText.text = value;
    }

    private void Awake()
    {
        buttonText = GetComponentInChildren<TMP_Text>();
    }

    public void OnClick()
    {
        onRaycastHit.Invoke(buttonIndex);
    }
}

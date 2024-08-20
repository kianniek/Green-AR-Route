using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonsListener : MonoBehaviour
{
    // This script takes in a list of buttons and listens for their clicks
    
    [SerializeField] private List<Button> buttons;
    
    [SerializeField] private UnityEvent onButtonClicked = new();

    private void OnEnable()
    {
        foreach (var button in buttons)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }
    
    private void OnDisable()
    {
        foreach (var button in buttons)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
    
    private void OnButtonClicked()
    {
        onButtonClicked.Invoke();
    }
}

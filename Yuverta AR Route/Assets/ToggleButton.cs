using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    
    [SerializeField] private bool _isOn = false;
    
    [SerializeField] private UnityEvent<bool> onButtonToggled;
    
    // Start is called before the first frame update
    
    private void Awake()
    {
        _button ??= GetComponent<Button>();
    }
    
    private void OnEnable()
    {
        _button ??= GetComponent<Button>();
        _button.onClick.AddListener(ToggleBehavour);
    }
    
    private void OnDisable()
    {
        _button ??= GetComponent<Button>();
        _button.onClick.RemoveListener(ToggleBehavour);
    }

    private void ToggleBehavour()
    {
        _isOn = !_isOn;
        
        onButtonToggled.Invoke(_isOn);
    }
}

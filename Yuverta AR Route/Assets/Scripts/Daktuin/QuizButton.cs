using System;
using System.Collections;
using System.Collections.Generic;
using Events;
using Events.GameEvents.Typed;
using LeTai.Asset.TranslucentImage;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Add this script to a button that will be used to answer a quiz question
/// </summary>
public class QuizButton : MonoBehaviour
{
    public int buttonIndex;

    public IntEvent onRaycastHit;

    private TMP_Text buttonText;
    private Image buttonVisual;

    private Color originalColor;

    public string ButtonText
    {
        get => buttonText.text;
        set => buttonText.text = value;
    }

    private void OnDisable()
    {
        ResetVisualsColor();
    }

    private void Awake()
    {
        buttonVisual = GetComponent<Image>();
        buttonText = GetComponentInChildren<TMP_Text>();

        originalColor = buttonVisual.color;
    }

    public void OnClick()
    {
        onRaycastHit.Invoke(buttonIndex);
    }

    public void FlashButton(Color color, float duration)
    {
        StartCoroutine(FlashButtonCoroutine(color, duration));
    }
    
    public void SetButtonColor(Color color)
    {
        buttonVisual.color = color;
    }
    
    public void ResetVisualsColor()
    {
        buttonVisual.color = originalColor;
    }

    private IEnumerator FlashButtonCoroutine(Color color, float duration)
    {
        buttonVisual.color = color;
        yield return new WaitForSeconds(duration);
        buttonVisual.color = originalColor;
        yield return null;
    }

    //color inverter
    public static Color InvertColor(Color color)
    {
        return new Color(1f - color.r, 1f - color.g, 1f - color.b);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Events;
using Events.GameEvents.Typed;
using LeTai.Asset.TranslucentImage;
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
    private TranslucentImage buttonVisual;

    private Color originalColor;
    private Color originalTextColor;

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
        buttonText = GetComponentInChildren<TMP_Text>();
        buttonVisual = GetComponent<TranslucentImage>();

        originalColor = buttonVisual.color;
        originalTextColor = buttonText.color;
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
        buttonText.color = InvertColor(originalTextColor);
    }
    
    public void ResetVisualsColor()
    {
        buttonVisual.color = originalColor;
        buttonText.color = originalTextColor;
    }

    private IEnumerator FlashButtonCoroutine(Color color, float duration)
    {
        buttonVisual.color = color;
        buttonText.color = InvertColor(originalTextColor);
        yield return new WaitForSeconds(duration);
        buttonVisual.color = originalColor;
        buttonText.color = originalTextColor;
        yield return null;
    }

    //color inverter
    public static Color InvertColor(Color color)
    {
        return new Color(1f - color.r, 1f - color.g, 1f - color.b);
    }
}
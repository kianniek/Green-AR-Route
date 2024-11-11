using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDiabledListner : MonoBehaviour
{
    public Button button;             // The button whose state you want to check
    public Image iconImage;           // The icon (Image) attached to the button
    public float disabledAlpha = 0.5f; // Transparency level when the button is disabled
    public float normalAlpha = 1f;    // Transparency level when the button is enabled

    private void Awake()
    {
        button ??= GetComponentInParent<Button>();
        iconImage ??= GetComponent<Image>();
    }

    void Start()
    {
        // Set the initial transparency based on whether the button is enabled or not
        UpdateIconTransparency();
        
        // Add listener for when the button's interactable state changes
        button.onClick.AddListener(UpdateIconTransparency);
    }

    void Update()
    {
        // Continuously update the icon transparency whenever the button state changes
        UpdateIconTransparency();
    }

    void UpdateIconTransparency()
    {
        // Check if the button is interactable (enabled)
        if (button.interactable)
        {
            SetIconTransparency(normalAlpha);
        }
        else
        {
            SetIconTransparency(disabledAlpha);
        }
    }

    // Function to set the transparency of the icon
    void SetIconTransparency(float alpha)
    {
        Color iconColor = iconImage.color;
        iconColor.a = alpha; // Change the alpha value of the icon
        iconImage.color = iconColor; // Apply the new color to the image
    }
}
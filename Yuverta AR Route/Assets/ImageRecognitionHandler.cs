using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using TMPro;

[Serializable]
public struct ImageEvent
{
    [SerializeField] internal string nameOfImage;
    [SerializeField] internal string promptText; // Text to display when image is recognized
    [SerializeField] internal UnityEvent action;
}

public class ImageRecognitionHandler : MonoBehaviour
{
    public ImageRecognitionEvent imageRecognitionEvent;

    public ImageEvent[] imageEvents;
    public TMP_Text promptTextObject; // Reference to the TMP text object in the scene

    private string currentRecognizedImage = null;
    private string currentPromptText = null;

    void OnEnable()
    {
        if (imageRecognitionEvent != null)
        {
            imageRecognitionEvent.OnImageRecognized += HandleImageRecognized;
            imageRecognitionEvent.OnImageRemoved += HandleImageRemoved;
        }
    }

    void OnDisable()
    {
        if (imageRecognitionEvent != null)
        {
            imageRecognitionEvent.OnImageRecognized -= HandleImageRecognized;
            imageRecognitionEvent.OnImageRemoved -= HandleImageRemoved;
        }
    }

    void Update()
    {
        // Detect user tap on screen
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleScreenTap();
        }
    }

    private void HandleImageRecognized(ARTrackedImage trackedImage)
    {
        // Only process the first recognized image
        Debug.Log("Image recognized: " + trackedImage.referenceImage.name);
        if (currentRecognizedImage == null)
        {
            currentRecognizedImage = trackedImage.referenceImage.name;

            foreach (var imageEvent in imageEvents)
            {
                if (imageEvent.nameOfImage == trackedImage.referenceImage.name)
                {
                    currentPromptText = imageEvent.promptText;
                    promptTextObject.text = currentPromptText; // Update the prompt text on screen
                }
            }
        }
    }

    private void HandleImageRemoved(ARTrackedImage trackedImage)
    {
        Debug.Log("Image removed: " + trackedImage.referenceImage.name);
        if (currentRecognizedImage == trackedImage.referenceImage.name)
        {
            currentRecognizedImage = null;
            currentPromptText = null;
            promptTextObject.text = ""; // Clear the prompt text

            // Check if there are other tracked images to switch to
            foreach (var otherTrackedImage in FindObjectsOfType<ARTrackedImage>())
            {
                if (otherTrackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
                {
                    HandleImageRecognized(otherTrackedImage);
                    break;
                }
            }
        }
    }

    private void HandleScreenTap()
    {
        if (currentRecognizedImage != null)
        {
            foreach (var imageEvent in imageEvents)
            {
                if (imageEvent.nameOfImage == currentRecognizedImage)
                {
                    imageEvent.action.Invoke(); // Invoke the action on screen tap
                }
            }
        }
    }
}
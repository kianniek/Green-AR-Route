using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using TMPro;

[Serializable]
public struct ImageEvent
{
    [SerializeField] internal string nameOfImage;
    [SerializeField] internal int imageSceneIndex;
    [SerializeField] internal string promptText; // Text to display when image is recognized
}

public class ImageRecognitionHandler : MonoBehaviour
{
    public ImageRecognitionEvent imageRecognitionEvent;

    public ImageEvent[] imageEvents;
    public TMP_Text promptTextObject; // Reference to the TMP text object in the scene

    private string currentRecognizedImage = null;
    private string currentPromptText = null;
    
    [SerializeField] internal SceneSwap sceneSwap;
    
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
            Debug.Log("Tapped Screen in ImageRecognitionHandler.cs");
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
        Debug.Log("Screen tapped");
        Debug.Log("Current recognized image: " + currentRecognizedImage);
        if (currentRecognizedImage == null)
            return;
        
        Debug.Log("Screen tapped for image: " + currentRecognizedImage);
        
        foreach (var imageEvent in imageEvents)
        {
            Debug.Log("Checking image event: " + imageEvent.nameOfImage);
            if (imageEvent.nameOfImage == currentRecognizedImage)
            {
                Debug.Log("Invoking action for image: " + currentRecognizedImage);
                sceneSwap.SwitchToScene(imageEvent.imageSceneIndex);
                //break out of the loop
                break;
            }
        }
    }
}
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

    [Header("Events")] [SerializeField] internal UnityEvent onImageTracked = new();
    [SerializeField] internal UnityEvent onImageRemoved = new();
    [SerializeField] internal UnityEvent onImageChanged = new();
    [SerializeField] internal UnityEvent onTapped = new();

    [SerializeField] private Animator promptAnimator;

    [SerializeField] internal SceneSwap sceneSwap;
    
    private ARAnchor anchor;

    void OnEnable()
    {
        if (imageRecognitionEvent != null)
        {
            imageRecognitionEvent.OnImageRecognized += HandleImageRecognized;
            imageRecognitionEvent.OnImageRecognizedStarted += HandleImageRecognizedStarted;
            imageRecognitionEvent.OnImageRemoved += HandleImageRemoved;
        }
    }

    void OnDisable()
    {
        if (imageRecognitionEvent != null)
        {
            imageRecognitionEvent.OnImageRecognized -= HandleImageRecognized;
            imageRecognitionEvent.OnImageRecognizedStarted -= HandleImageRecognizedStarted;

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

    private void HandleImageRecognizedStarted(ARTrackedImage trackedImage)
    {
        onImageTracked.Invoke();
        promptAnimator.SetBool("Hidden", false);
        promptAnimator.SetBool("Middle", true);
        promptAnimator.SetBool("Top", false);
    }


    private void HandleImageRecognized(ARTrackedImage trackedImage)
    {
        anchor = trackedImage.gameObject.GetComponent<ARAnchor>();
        // Only process the first recognized image
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
        else if (currentRecognizedImage != trackedImage.referenceImage.name)
        {
            currentRecognizedImage = trackedImage.referenceImage.name;
            foreach (var imageEvent in imageEvents)
            {
                if (imageEvent.nameOfImage == trackedImage.referenceImage.name)
                {
                    currentPromptText = imageEvent.promptText;
                    promptTextObject.text = currentPromptText; // Update the prompt text on screen
                    onImageChanged.Invoke();
                }
            }
        }
        
        promptAnimator.SetBool("Hidden", false);
        promptAnimator.SetBool("Middle", true);
        promptAnimator.SetBool("Top", false);
    }

    private void HandleImageRemoved(ARTrackedImage trackedImage)
    {
        onImageRemoved.Invoke();
        if (currentRecognizedImage == trackedImage.referenceImage.name)
        {
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
        
        promptAnimator.SetBool("Hidden", false);
        promptAnimator.SetBool("Middle", false);
        promptAnimator.SetBool("Top", true);
    }

    private void HandleScreenTap()
    {
        promptAnimator.SetBool("Hidden", true);
        promptAnimator.SetBool("Middle", false);
        promptAnimator.SetBool("Top", false);
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
                onTapped.Invoke();
                Debug.Log("Invoking action for image: " + currentRecognizedImage);
                sceneSwap.SwitchToScene(imageEvent.imageSceneIndex);
                //break out of the loop
                break;
            }
        }
    }
}
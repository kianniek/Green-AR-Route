using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using TMPro;
using System.Collections;

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

    private ARTrackedImage currentRecognizedImage = null;
    private string currentPromptText = null;

    [Header("Events")] [SerializeField] internal UnityEvent onImageTracked = new();
    [SerializeField] internal UnityEvent onImageRemoved = new();
    [SerializeField] internal UnityEvent onImageChanged = new();
    [SerializeField] internal UnityEvent onTapped = new();

    [SerializeField] private Animator promptAnimator;

    [SerializeField] internal SceneSwap sceneSwap;

    private ARAnchor anchor;
    [SerializeField] private float hideDelay = 2f; // Time to wait before hiding the text

    void OnEnable()
    {
        if (imageRecognitionEvent != null)
        {
            imageRecognitionEvent.OnImageRecognized.AddListener(HandleImageRecognized);
            imageRecognitionEvent.OnImageRecognizedStarted.AddListener(HandleImageRecognizedStarted);
            imageRecognitionEvent.OnImageRemoved.AddListener(HandleImageRemoved);
        }
    }

    void OnDisable()
    {
        if (imageRecognitionEvent != null)
        {
            imageRecognitionEvent.OnImageRecognized.RemoveListener(HandleImageRecognized);
            imageRecognitionEvent.OnImageRecognizedStarted.RemoveListener(HandleImageRecognizedStarted);
            imageRecognitionEvent.OnImageRemoved.RemoveListener(HandleImageRemoved);
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
    }

    private void HandleImageRecognized(ARTrackedImage trackedImage)
    {
        anchor = trackedImage.gameObject.GetComponent<ARAnchor>();

        // Only process the first recognized image
        if (currentRecognizedImage == null)
        {
            currentRecognizedImage = trackedImage;

            foreach (var imageEvent in imageEvents)
            {
                if (imageEvent.nameOfImage == trackedImage.referenceImage.name)
                {
                    Debug.Log(imageEvent.promptText);
                    currentPromptText = imageEvent.promptText;
                    promptTextObject.text = currentPromptText; // Update the prompt text on screen
                }
            }
        }
        else if (currentRecognizedImage != trackedImage)
        {
            currentRecognizedImage = trackedImage;
            foreach (var imageEvent in imageEvents)
            {
                if (imageEvent.nameOfImage != trackedImage.referenceImage.name)
                    continue;

                Debug.Log(imageEvent.promptText);
                currentPromptText = imageEvent.promptText;
                promptTextObject.text = currentPromptText; // Update the prompt text on screen
                onImageChanged.Invoke();
            }
        }
    }

    private void HandleImageRemoved(ARTrackedImage trackedImage)
    {
        if (currentRecognizedImage == trackedImage)
        {
            onImageRemoved.Invoke();
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
            if (imageEvent.nameOfImage == currentRecognizedImage.referenceImage.name)
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
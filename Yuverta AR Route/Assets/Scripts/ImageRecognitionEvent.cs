using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageRecognitionEvent : MonoBehaviour
{
    private ARTrackedImageManager _trackedImageManager;

    public UnityEvent<ARTrackedImage> OnImageRecognized = new();
    public UnityEvent<ARTrackedImage> OnImageRemoved = new();
    public UnityEvent<ARTrackedImage> OnImageRecognizedStarted = new();

    void Awake()
    {
        _trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        _trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        _trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            if (trackedImage.referenceImage != null)
            {
                Debug.Log($"New image recognized: {trackedImage.referenceImage.name}");
            }
            else
            {
                Debug.LogWarning("Added image reference is null.");
            }
        
            OnImageRecognizedStarted?.Invoke(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.referenceImage != null)
            {
                Debug.Log($"trackedImage: {trackedImage.referenceImage.name} | {trackedImage.trackingState}");
            }
            else
            {
                Debug.LogWarning("Updated image reference is null.");
            }

            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                OnImageRecognized?.Invoke(trackedImage);
            }
        
            if (trackedImage.trackingState == TrackingState.Limited)
            {
                OnImageRemoved?.Invoke(trackedImage);
            }
        }
    }
}
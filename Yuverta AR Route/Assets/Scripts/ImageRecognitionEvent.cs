using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageRecognitionEvent : MonoBehaviour
{
    private ARTrackedImageManager _trackedImageManager;

    public event Action<ARTrackedImage> OnImageRecognized;
    public event Action<ARTrackedImage> OnImageRemoved;

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
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            // Image has been recognized for the first time
            OnImageRecognized?.Invoke(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            // Image tracking has been updated (position, rotation, etc.)
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                OnImageRecognized?.Invoke(trackedImage);
            }
            
            if(trackedImage.trackingState == TrackingState.None)
            {
                // Image tracking has been lost
                OnImageRemoved?.Invoke(trackedImage);
            }
        }
    }
}
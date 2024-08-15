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
    public event Action<ARTrackedImage> OnImageRecognizedStarted;

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
            Debug.Log("Added");
            OnImageRecognizedStarted?.Invoke(trackedImage);
            
            //make an anchor if the tracked image is recognized and the image doesnt already have an anchor
            var anchor = trackedImage.gameObject.GetComponent<ARAnchor>();
            if (anchor == null)
            {
                anchor = trackedImage.gameObject.AddComponent<ARAnchor>();
            }
            
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            Debug.Log(trackedImage.referenceImage + "  "+ trackedImage.referenceImage.name +" | "+ trackedImage.trackingState);
            // Image tracking has been updated (position, rotation, etc.)
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                OnImageRecognized?.Invoke(trackedImage);
            }
            
            if(trackedImage.trackingState == TrackingState.Limited)
            {
                OnImageRemoved?.Invoke(trackedImage);
            }
        }
    }
}
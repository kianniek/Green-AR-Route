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
            Debug.Log($"New image recognized: {trackedImage.referenceImage.name}");

            OnImageRecognizedStarted?.Invoke(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            Debug.Log($"trackedImage: {trackedImage.referenceImage.name} | {trackedImage.trackingState}");

            switch (trackedImage.trackingState)
            {
                case TrackingState.Tracking:
                    OnImageRecognized?.Invoke(trackedImage);
                    break;
                case TrackingState.Limited:
                    OnImageRemoved?.Invoke(trackedImage);
                    break;
            }
        }
    }
}
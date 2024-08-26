using System;
using System.Collections.Generic;
using System.Linq;
using Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class QRCodeManager : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    public ImageRecognitionEvent imageRecognitionEvent;

    private ARTrackedImage currentRecognizedImage = null;


    [Header("Events")] 
    [SerializeField] internal UnityEvent onImageTracked = new();
    [SerializeField] internal UnityEvent onImageRemoved = new();

    [Serializable]
    public class QRCode
    {
        public string name;
        public bool scanned;
        public int index;
        public IntEvent action;
    }

    public List<QRCode> qrCodes;


    private void OnEnable()
    {
        if (imageRecognitionEvent != null)
        {
            imageRecognitionEvent.OnImageRecognized.AddListener(HandleImageRecognized);
            imageRecognitionEvent.OnImageRecognizedStarted.AddListener(HandleImageRecognizedStarted);
            imageRecognitionEvent.OnImageRemoved.AddListener(HandleImageRemoved);
        }else
        {
            Debug.LogError("ImageRecognitionEvent is null");
        }
    }

    private void OnDisable()
    {
        if (imageRecognitionEvent != null)
        {
            imageRecognitionEvent.OnImageRecognized.RemoveListener(HandleImageRecognized);
            imageRecognitionEvent.OnImageRecognizedStarted.RemoveListener(HandleImageRecognizedStarted);
            imageRecognitionEvent.OnImageRemoved.RemoveListener(HandleImageRemoved);
        }else
        {
            Debug.LogError("ImageRecognitionEvent is null");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Invoke");
            qrCodes[0].action.Invoke(0);
            qrCodes[0].action.Invoke(1);
            qrCodes[0].action.Invoke(2);
            qrCodes[0].action.Invoke(3);
        }
    }

    private void HandleImageRecognizedStarted(ARTrackedImage trackedImage)
    {
        onImageTracked.Invoke();
    }

    private void HandleImageRecognized(ARTrackedImage trackedImage)
    {
        Debug.Log($"OnTrackedImagesChanged | {trackedImage.referenceImage.name} | {trackedImage.trackingState}");
        ProcessTrackedImage(trackedImage);
    }

    private void HandleImageRemoved(ARTrackedImage trackedImage)
    {
        if (currentRecognizedImage == trackedImage)
        {
            onImageRemoved.Invoke();
        }
    }
    
    private bool isFMODDone = false; // The boolean to track FMOD status

    public void SetFMODStatus(bool status)
    {
        isFMODDone = status;
    }

    private void ProcessTrackedImage(ARTrackedImage trackedImage)
    {
        // Check if FMOD event is still active
        if (isFMODDone)
        {
            Debug.Log("Cannot collect leaf, FMOD event is still active.");
            return;
        }
        
        var qrCodeName = trackedImage.referenceImage.name;

        foreach (var qrCode in qrCodes.Where(qrCode => qrCode.name == qrCodeName))
        {
            switch (qrCode.scanned)
            {
                case true:
                    break;
                case false:
                    qrCode.scanned = true;
                    qrCode.action.Invoke(qrCode.index);
                    break;
            }
        }
    }
}
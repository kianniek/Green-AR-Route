using System;
using System.Collections.Generic;
using System.Linq;
using Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

#if UNITY_EDITOR
using UnityEditor;
using GUILayout = UnityEngine.GUILayout;


[CanEditMultipleObjects]
#endif
public class QRCodeManager : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    public ImageRecognitionEvent imageRecognitionEvent;

    private ARTrackedImage currentRecognizedImage = null;


    [Header("Events")] [SerializeField] internal UnityEvent onImageTracked = new();
    [SerializeField] internal UnityEvent onImageRemoved = new();
    [SerializeField] internal UnityEvent onImageNewScanned = new();
    [SerializeField] internal UnityEvent on3thScanned = new();
    [SerializeField] internal UnityEvent onLastImageScanned = new();

    [Serializable]
    public class QRCode
    {
        public string name;
        public bool scanned;
        public int index;
        public IntEvent action;
    }

    public List<QRCode> qrCodes;

    private bool hasScannedAll => qrCodes.All(qrCode => qrCode.scanned);
    private bool hasScanned3th => qrCodes.Count(qrCode => qrCode.scanned) == 3;
    private bool hasInvokedLastImageScanned = false;

    private void OnEnable()
    {
        if (imageRecognitionEvent != null)
        {
            imageRecognitionEvent.OnImageRecognized.AddListener(HandleImageRecognized);
            imageRecognitionEvent.OnImageRecognizedStarted.AddListener(HandleImageRecognizedStarted);
            imageRecognitionEvent.OnImageRemoved.AddListener(HandleImageRemoved);
        }
        else
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
        }
        else
        {
            Debug.LogError("ImageRecognitionEvent is null");
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
                    onImageNewScanned.Invoke();
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        if (hasScannedAll && !hasInvokedLastImageScanned)
        {
            if (!isFMODDone)
            {
                onLastImageScanned.Invoke();
                hasInvokedLastImageScanned = true;
            }
        }

        if (hasScanned3th)
        {
            if (!isFMODDone)
            {
                on3thScanned.Invoke();
            }
        }
    }
}


//add a unity editor script that adds a button to the inspector to simulate the scanning of all qr codes
#if UNITY_EDITOR

[CustomEditor(typeof(QRCodeManager))]
public class QRCodeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var qrCodeManager = (QRCodeManager)target;

        if (GUILayout.Button("Simulate Scanning All QR Codes"))
        {
            foreach (var qrCode in qrCodeManager.qrCodes)
            {
                qrCode.scanned = true;
                qrCode.action.Invoke(qrCode.index);
            }
        }
    }
}
#endif
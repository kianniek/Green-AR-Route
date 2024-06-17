using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class QRCodeManager : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    
    [Serializable]
    public struct QRCode 
    {
        public string name;
        public bool scanned;
        public UnityEvent action;
    }
    
    private List<QRCode> qrCodes;
    
    public XRReferenceImageLibrary referenceImageLibrary;
    
    void Start()
    {
        trackedImageManager.referenceLibrary = referenceImageLibrary;

        foreach (var image in referenceImageLibrary)
        {
            qrCodes.Add(new QRCode
            {
                name = image.name,
                scanned = false,
                action = new UnityEvent()
            });
        }
    }
    
    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            ProcessTrackedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            ProcessTrackedImage(trackedImage);
        }
    }

    private void ProcessTrackedImage(ARTrackedImage trackedImage)
    {
        string qrCodeName = trackedImage.referenceImage.name;

        QRCode scannedQRCode = new QRCode();
        foreach (var qrCode in qrCodes)
        {
            if (qrCode.name == qrCodeName)
            {
                switch (qrCode.scanned)
                {
                    case true:
                        ScannedObject(qrCode);
                        break;
                    case false:
                        scannedQRCode = qrCode;
                        break;
                }
            }
        }
        
        if (scannedQRCode.name == null) return;
        scannedQRCode.scanned = true;
        scannedQRCode.action.Invoke();
    }

    //Function for when a QR code is scanned for the second time
    private void ScannedObject(QRCode qrCode)
    {
        //Temp code dunno what to do yet
        Debug.Log($"Scanned object: {qrCode.name}");
    }
}
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

    [Serializable]
    public class QRCode
    {
        public string name;
        public bool scanned;
        public int index;
        public IntEvent action;
    }

    public List<QRCode> qrCodes;

    public XRReferenceImageLibrary referenceImageLibrary;
    
    private float time;

    private void Start()
    {
        time = Time.time;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Invoke");
            qrCodes[0].action.Invoke(0);
            qrCodes[0].action.Invoke(1);
            qrCodes[0].action.Invoke(2);
            qrCodes[0].action.Invoke(3);
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
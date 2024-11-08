using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(CropTracker), typeof(CropGrowthSystem))]
public class CropContainer : MonoBehaviour
{
    [Tooltip("This is the location where the crop will spawn when the seed is planted.")] [SerializeField]
    private Transform cropSpawnLocation;

    [Tooltip("This is the text that will display the name of the crop.")] [SerializeField]
    private TextMeshPro cropDisplayName;

    [SerializeField] private CropScript cropScript;

    public CropScript CropScript => cropScript;

    public Transform CropSpawnLocation => cropSpawnLocation;

    private bool rightCrop;
    private Camera _camera;

    public bool currentCropIsRightCrop => rightCrop;

    public UnityEvent onCropHarvested = new();
    public UnityEvent onCropFirstHarvested = new();
    public UnityEvent<CropScript> onCropPlanted = new();

    bool firstHarvest = false;

    bool inputBlocked = false;

    private CropObject cropObjectBeforeWrongCrop;

    public void BlockInput(bool value)
    {
        inputBlocked = value;
    }

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Input.touchCount > 0 && !inputBlocked)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                HarvestCrop(touch.position);
                Debug.Log("Touch position: " + touch.position);
            }
        }
    }

    public void NewCrop(CropScript newCropObject, CropObject.CropType nextCorrectCropType)
    {
        // Update the current crop script
        var currentCropObject = cropScript;
        cropScript = newCropObject;

        // Determine if the new crop is the correct one
        rightCrop = nextCorrectCropType == cropScript.cropObject.currentCropType ||
                    nextCorrectCropType == CropObject.CropType.none;
        Debug.Log("Correct crop: " +
                  cropScript.cropObject.currentCropType); // Update the display with the new crop's name
        
        if (rightCrop)
        {
            cropObjectBeforeWrongCrop = currentCropObject.cropObject;
        }

        cropDisplayName.text = cropScript.cropObject.cropName;
        // Invoke the crop planted event
        if (firstHarvest == false)
        {
            return;
        }

        onCropPlanted.Invoke(cropScript);
    }

    private void HarvestCrop(Vector2 touchPosition)
    {
        Debug.Log("Harvesting crop");
        // Check if the crop is fully grown
        if (!cropScript.IsFullyGrown && currentCropIsRightCrop)
            return;

        Debug.Log("Crop is fully grown");

        // Check with a raycast if the touch actually hit the crop
        var ray = _camera!.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.collider.gameObject.CompareTag("Crop"))
            {
                Debug.Log("Crop harvested");

                if (firstHarvest == false)
                {
                    firstHarvest = true;
                    onCropFirstHarvested.Invoke();
                }

                if (!rightCrop)
                {
                    cropDisplayName.text = cropObjectBeforeWrongCrop.cropName;
                }

                onCropHarvested.Invoke();
                cropScript.HarvestCrop();
            }

            Debug.Log(hit.collider.gameObject.name);
        }
    }
}
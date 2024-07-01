using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CropTracker), typeof(CropGrowthSystem))]
public class CropContainer : MonoBehaviour
{
    [Tooltip("This is the location where the crop will spawn when the seed is planted.")]
    [SerializeField] private Transform cropSpawnLocation;
    
    [Tooltip("This is the text that will display the name of the crop.")]
    [SerializeField] private TextMeshPro cropNameDisplay;
    
    [SerializeField] private CropScript cropScript;
    
    public CropScript CropScript => cropScript;
    
    public Transform CropSpawnLocation => cropSpawnLocation;
    
    private bool rightCrop;
    public bool currentCropIsRightCrop => rightCrop;
    
    public UnityAction onCropHarvested;
    public UnityAction onCropPlanted;

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                HarvestCrop(touch.position);
            }
        }
    }

    public void NewCrop(CropScript newCropObject, CropObject.CropType nextCorrectCropType)
    {
        // Update the current crop script
        cropScript = newCropObject;
    
        // Determine if the new crop is the correct one
        rightCrop = nextCorrectCropType == cropScript.cropObject.currentCropType || nextCorrectCropType == CropObject.CropType.none;
        Debug.Log("Correct crop: " + cropScript.cropObject.currentCropType);
        
        // Update the display with the new crop's name
        cropNameDisplay.text = cropScript.cropObject.cropName;
    
        // Invoke the crop planted event
        onCropPlanted.Invoke();
    
        // Add the new crop's HarvestCrop method to the onCropHarvested event
        onCropHarvested += newCropObject.HarvestCrop;
    }

    private void HarvestCrop(Vector2 touchPosition)
    {
        // Check if the crop is fully grown
        if (cropScript.growthStage < cropScript.growthStages.Count - 1) 
            return;
        
        // Check with a raycast if the touch actually hit the crop
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out var hit, 1000f))
        {
            if (hit.collider.gameObject.CompareTag("Crop"))
            {
                onCropHarvested.Invoke();
            }
        }
    }
}

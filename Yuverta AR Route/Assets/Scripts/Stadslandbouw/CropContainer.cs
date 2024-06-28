using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedField.Global

[RequireComponent(typeof(CropTracker), typeof(CropGrowthSystem))]
public class CropContainer : MonoBehaviour
{
    [Tooltip("This list contains the spawn locations for the seeds the crop tracker spawns")]
    public List<Transform> seedsSpawnLocations; 
    [Tooltip("This is the location where the crop will spawn when the seed is planted.")]
    public Transform cropSpawnLocation;
    [Tooltip("This is the text that will display the name of the crop.")]
    [SerializeField] private TextMeshPro cropNameDisplay;
    public CropScript currentCropObject;
    private CropScript lastCorrectCropObject;
    
    /// <summary>
    /// This bool is to store if the crop is correct or not.
    /// </summary>
    private bool rightCrop;
    
    public UnityAction onCropHarvested;
    public UnityAction onCropPlanted;

    public InputActionReference harvestTap;

    public void Enable()
    {
        harvestTap.action.started += HarvestCrop;
    }
    
    private void Disable()
    {
        harvestTap.action.started -= HarvestCrop;
    }

    public void NewCrop(CropScript newCropObject)
    {
        if (rightCrop) lastCorrectCropObject = currentCropObject;
        
        currentCropObject = newCropObject;
        if (lastCorrectCropObject) rightCrop = lastCorrectCropObject.cropObject.nextCrop == currentCropObject.cropObject.cropType;
        else rightCrop = true;
        
        //Spawning the first stage of the crop
        newCropObject.GrowCrop();
        cropNameDisplay.text = currentCropObject.cropObject.cropName;
        onCropPlanted.Invoke();
        
        onCropHarvested += newCropObject.HarvestCrop;
    }
    
    private void HarvestCrop(InputAction.CallbackContext context)
    {
        if (currentCropObject.growthStage < currentCropObject.cropObject.growthStages.Count - 1) return;
        Debug.Log("Harvesting crop");
        //Check with a rayCast if the touch actually hit the crop
        var ray = Camera.main!.ScreenPointToRay(context.ReadValue<Vector2>());
        if (Physics.Raycast(ray, out var hit, 1000f))
        {
            if (hit.collider.gameObject.CompareTag("Crop"))
            {
                onCropHarvested.Invoke();
                Disable();
            }
        }
    }
}

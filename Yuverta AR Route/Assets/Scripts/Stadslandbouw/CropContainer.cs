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
    
    public UnityEvent onCropHarvested;
    public UnityEvent onCropPlanted;

    public InputActionReference harvestTap;
    
    // Start is called before the first frame update
    void Start()
    {
        onCropHarvested = new UnityEvent();
        onCropPlanted = new UnityEvent();
        harvestTap.action.performed += _ => HarvestCrop();
    }

    public void NewCrop(CropScript newCropObject)
    {
        if (rightCrop) lastCorrectCropObject = currentCropObject;
        
        currentCropObject = newCropObject;
        rightCrop = lastCorrectCropObject.cropObject.nextCrop == currentCropObject.cropObject.cropType;
        
        //Spawning the first stage of the crop
        newCropObject.GrowCrop();
        cropNameDisplay.text = currentCropObject.cropObject.cropName;
        onCropPlanted.Invoke();
        
        onCropHarvested.AddListener(newCropObject.HarvestCrop);
        newCropObject.fullyGrown.AddListener(harvestTap.action.Enable);
    }
    
    private void HarvestCrop()
    {
        if (currentCropObject.growthStage != currentCropObject.cropObject.growthStages.Count - 1) return;
        
        //Check with a rayCast if the touch actually hit the crop
        var pointerReference = new PointerEventData(EventSystem.current);
        var ray = Camera.main.ScreenPointToRay(pointerReference.position);
        if (Physics.Raycast(ray, out var hit, 1000f))
        {
            if (hit.collider.gameObject != currentCropObject.gameObject) return;
        }
        
        onCropHarvested.Invoke();
        harvestTap.action.Disable();
    }
}

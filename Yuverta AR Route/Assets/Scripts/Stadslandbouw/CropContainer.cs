using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(CropTracker), typeof(CropGrowthSystem))]
public class CropContainer : MonoBehaviour
{
    [Tooltip("This list contains the spawn locations for the seeds the crop tracker spawns")]
    public List<Transform> seedsSpawnLocations; 
    public Transform cropSpawnLocation;
    [SerializeField] private TextMeshPro cropNameDisplay;
    public CropScript currentCropObject;
    private CropScript lastCorrectCropObject;
    private CropScript lastCropObject;
    private bool rightCrop;
    
    public UnityEvent onCropHarvested;
    public UnityEvent onCropPlanted;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NewCrop(CropScript newCropObject)
    {
        if (rightCrop)
        {
            lastCorrectCropObject = currentCropObject;
        }
        else lastCropObject = currentCropObject;
        
        currentCropObject = newCropObject;
        rightCrop = lastCorrectCropObject.cropObject.nextCrop == currentCropObject.cropObject.cropName;
        
        //Spawning the first stage of the crop
        newCropObject.GrowCrop();
        cropNameDisplay.text = currentCropObject.cropObject.cropName;
        onCropPlanted.Invoke();
        
        onCropHarvested.AddListener(newCropObject.HarvestCrop);
    }
    
    public void HarvestCrop()
    {
        if (currentCropObject.growthStage == currentCropObject.cropObject.growthStages.Count - 1)
        {
            onCropHarvested.Invoke();
        }
    }
}

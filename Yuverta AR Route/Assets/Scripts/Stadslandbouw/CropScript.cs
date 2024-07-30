using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

// ReSharper disable InconsistentNaming

public class CropScript : MonoBehaviour
{
    public CropObject cropObject;
    private GameObject currentChild;
    private int growthStage;
    private UnityEvent fullyGrown = new();
    [FormerlySerializedAs("growthStages")] public List<GameObject> growthStagesList;
    private GameObject deadCrop;
    
    [SerializeField] private CropContainer cropContainer;
    [SerializeField] private CropTracker cropTracker;
    
    private bool hasCrop;
    
    //All crops have 6 growth stages
    private const int GROWTH_STAGES = 6;
    
    public bool IsFullyGrown => growthStage == GROWTH_STAGES - 1;

    private void Start()
    {
        if(cropTracker == null) 
            cropTracker = FindObjectOfType<CropTracker>();
    }

    public void NewCrop(CropObject newCrop)
    {
        //If there is already a crop return
        if (hasCrop) 
            return;
        
        //Setting the new crop object
        cropObject = newCrop;
        
        //Creating the new crop
        Instantiate(cropObject.cropPrefab, transform);
        
        //Adding the growth stages to the list and setting them to inactive
        for (int i = 0; i < GROWTH_STAGES; i++)
        {
            growthStagesList.Add(transform.GetChild(0).GetChild(i).gameObject);
            growthStagesList[i].SetActive(false);
        }

        //Initializing the dead crop
        for (var i = GROWTH_STAGES; i < transform.GetChild(0).childCount; i++)
        {
            if (!transform.GetChild(0).GetChild(i).gameObject.name.Contains("Dead")) 
                continue;
            
            deadCrop = transform.GetChild(0).GetChild(i).gameObject;
            deadCrop.SetActive(false);
            break;
        }
        
        //Setting the current child to the first growth stage
        growthStage = -1;
        
        GrowCrop();
        
        hasCrop = true;
    }

    public void FullyGrowCrop(CropObject newCrop)
    {
        //Setting the new crop object
        cropObject = newCrop;
        
        //Creating the new crop
        Instantiate(cropObject.cropPrefab, transform);
        
        //Adding the growth stages to the list
        for (int i = 0; i < GROWTH_STAGES; i++)
        {
            growthStagesList.Add(transform.GetChild(0).GetChild(i).gameObject);
            growthStagesList[i].SetActive(false);
        }
        
        //Upgrading the growth stage for the next round
        growthStage = growthStagesList.Count - 1;
        
        //Setting the current child to the last growth stage
        currentChild = growthStagesList[growthStage];
        currentChild.SetActive(true);
        currentChild.tag = "Crop";
        
        fullyGrown.Invoke();
        Debug.Log("Fully grown");
        
        hasCrop = true;
    }
    
    public void GrowCrop()
    {
        if (currentChild) 
            currentChild.SetActive(false);
        
        //Upgrading the growth stage for the next round
        growthStage++;
        
        currentChild = growthStagesList[growthStage];
        currentChild.SetActive(true);
        currentChild.tag = "Crop";

        //If the crop is not the right crop, show the dead crop at the last growth stage
        if (growthStage == GROWTH_STAGES - 1 && !cropContainer.currentCropIsRightCrop)
        {
            deadCrop.SetActive(true);
            deadCrop.tag = "Crop";
            growthStagesList[growthStage].SetActive(false);
            
            fullyGrown.Invoke();
            Debug.Log("Fully grown");
            return;
        }
        

        if (growthStage < growthStagesList.Count) 
            return;
        
        fullyGrown.Invoke();
        Debug.Log("Fully grown");
    }
    
    public void HarvestCrop()
    {
        Destroy(transform.GetChild(0).gameObject);
        
        growthStagesList.Clear();
        hasCrop = false;
        cropTracker.NewRound();
    }
}

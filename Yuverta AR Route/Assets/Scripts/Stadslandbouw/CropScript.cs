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
    [SerializeField] private UnityEvent fullyGrownCorrect = new();
    [SerializeField] private UnityEvent fullyGrownWrong = new();
    [SerializeField] private UnityEvent<int> growthStageChanged = new();
    [FormerlySerializedAs("growthStages")] public List<GameObject> growthStagesList;
    private GameObject deadCrop;
    
    [SerializeField] private CropContainer cropContainer;
    [SerializeField] private CropTracker cropTracker;
    
    private bool hasCrop;
    
    //All crops have 6 growth stages
    private const int GROWTH_STAGES = 6;
    
    public bool IsFullyGrown => growthStage == GROWTH_STAGES - 1;
    
    public int amountToGetCorrect = 6;
    public UnityEvent onCorrectAmountReached = new();
    
    private int correctInARow;
    
    [SerializeField] private UnityEvent OnChildUpdate = new();

    
    private void OnEnable()
    {
        cropContainer.onCropHarvested.AddListener(() => OnChildUpdate.Invoke());
        cropContainer.onCropFirstHarvested.AddListener(() => OnChildUpdate.Invoke());
        cropContainer.onCropPlanted.AddListener((context) => OnChildUpdate.Invoke());
        cropTracker.onNewRound.AddListener(() => OnChildUpdate.Invoke());
        cropTracker.onInitializeFirstCrop.AddListener(() => OnChildUpdate.Invoke());
        cropTracker.onPickedSeed.AddListener(() => OnChildUpdate.Invoke());
    }
    private void OnDisable()
    {
        cropContainer.onCropHarvested.RemoveListener(() => OnChildUpdate.Invoke());
        cropContainer.onCropFirstHarvested.RemoveListener(() => OnChildUpdate.Invoke());
        cropContainer.onCropPlanted.RemoveListener((context) => OnChildUpdate.Invoke());
        cropTracker.onNewRound.RemoveListener(() => OnChildUpdate.Invoke());
        cropTracker.onInitializeFirstCrop.RemoveListener(() => OnChildUpdate.Invoke());
        cropTracker.onPickedSeed.RemoveListener(() => OnChildUpdate.Invoke());
    }
    private void Start()
    {
        if(cropTracker == null) 
            cropTracker = FindObjectOfType<CropTracker>();
        
        OnChildUpdate.Invoke();
    }

    public void NewCrop(CropObject newCrop)
    {
        //If there is already a crop return
        if (hasCrop)
        {
            return;
        }
        
        //Setting the new crop object
        cropObject = newCrop;
        
        //Creating the new crop
        var prefab = Instantiate(cropObject.cropPrefab, transform);
        //Setting the new crop as the first child
        prefab.transform.SetSiblingIndex(0);
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
        OnChildUpdate.Invoke();
    }

    public void FullyGrowCrop(CropObject newCrop)
    {
        //Setting the new crop object
        cropObject = newCrop;
        
        //Creating the new crop
        var prefab = Instantiate(cropObject.cropPrefab, transform);
        
        //Setting the new crop as the first child
        prefab.transform.SetSiblingIndex(0);
        
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
        
        Debug.Log("Fully grown");
        
        hasCrop = true;
        OnChildUpdate.Invoke();
    }
    
    public void GrowCrop()
    {
        Debug.Log("Growing crop");
        if (currentChild) 
            currentChild.SetActive(false);
        
        //Upgrading the growth stage for the next round
        growthStage++;
        growthStageChanged.Invoke(growthStage);
        
        currentChild = growthStagesList[growthStage];
        currentChild.SetActive(true);
        currentChild.tag = "Crop";

        //If the crop is not the right crop, show the dead crop at the last growth stage
        if (growthStage == 1 && !cropContainer.currentCropIsRightCrop)
        {
            deadCrop.SetActive(true);
            deadCrop.tag = "Crop";
            growthStagesList[growthStage].SetActive(false);
            growthStage = GROWTH_STAGES - 1;
            
            fullyGrownWrong.Invoke();
            Debug.Log("Fully grown");
            OnChildUpdate.Invoke();
            return;
        }
        

        if (growthStage < growthStagesList.Count - 1) 
            return;
        
        fullyGrownCorrect.Invoke();
        
        correctInARow++;

        if (correctInARow == amountToGetCorrect)
        {
            onCorrectAmountReached.Invoke();
            correctInARow = 0;
        }
        
        Debug.Log("Fully grown");
        OnChildUpdate.Invoke();
    }
    
    public void HarvestCrop()
    {
        Destroy(transform.GetChild(0).gameObject);
        
        growthStagesList.Clear();
        hasCrop = false;
        cropTracker.NewRound();
        OnChildUpdate.Invoke();
    }
}

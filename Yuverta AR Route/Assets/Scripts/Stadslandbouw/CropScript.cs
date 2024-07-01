using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
// ReSharper disable InconsistentNaming

public class CropScript : MonoBehaviour
{
    public CropObject cropObject;
    private Transform parent;
    private GameObject currentChild;
    public int growthStage;
    private UnityAction fullyGrown;
    public List<GameObject> growthStages;
    private GameObject deadCrop;
    
    [SerializeField] private CropContainer cropContainer;
    [SerializeField] private CropTracker cropTracker;
    private bool hasCrop;
    
    //All crops have 6 growth stages
    private const int ChildCount = 6;

    private void Awake()
    {
        if(cropTracker == null) 
            cropTracker = FindObjectOfType<CropTracker>();
    }

    // Start is called before the first frame update
    void Start()
    {
        parent = transform;
    }

    public void NewCrop(CropObject newCrop)
    {
        //If there is already a crop return
        if (hasCrop) 
            return;
        
        //Setting the new crop object
        cropObject = newCrop;
        
        //Creating the new crop
        Instantiate(cropObject.cropPrefab, parent);
        
        //Adding the growth stages to the list
        for (int i = 0; i < ChildCount; i++)
        {
            growthStages.Add(transform.GetChild(0).GetChild(i).gameObject);
            growthStages[i].SetActive(false);
        }

        //Setting the dead crop
        for (var i = ChildCount; i < transform.GetChild(0).childCount; i++)
        {
            if (!transform.GetChild(0).GetChild(i).gameObject.name.Contains("Dead")) 
                continue;
            
            deadCrop = transform.GetChild(0).GetChild(i).gameObject;
            deadCrop.SetActive(false);
            break;
        }
        
        growthStage = 0;
        
        GrowCrop();
        
        hasCrop = true;
    }
    
    public void GrowCrop()
    {
        if (currentChild) 
            currentChild.SetActive(false);
        
        currentChild = growthStages[growthStage];
        currentChild.SetActive(true);
        currentChild.tag = "Crop";

        if (growthStage == ChildCount - 1 && !cropContainer.currentCropIsRightCrop)
        {
            deadCrop.SetActive(true);
            deadCrop.tag = "Crop";
            growthStages[growthStage].SetActive(false);
        }
        
        //Upgrading the growth stage for the next round
        growthStage++;

        if (growthStage < growthStages.Count) 
            return;
        
        fullyGrown.Invoke();
        Debug.Log("Fully grown");
    }
    
    public void HarvestCrop()
    {
        Destroy(transform.GetChild(0).gameObject);
        
        growthStages.Clear();
        hasCrop = false;
        cropTracker.NewRound();
    }
}

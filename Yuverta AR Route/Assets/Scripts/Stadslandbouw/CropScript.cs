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
    public int growthStage = 0;
    public UnityEvent fullyGrown;
    
    // Start is called before the first frame update
    void Start()
    {
        fullyGrown = new UnityEvent();
        parent = transform;
    }
    
    public void GrowCrop()
    {
        Destroy(currentChild);
        currentChild = Instantiate(cropObject.growthStages[growthStage], parent);
        growthStage++;
        
        if (growthStage == cropObject.growthStages.Count) fullyGrown.Invoke();
    }
    
    public void HarvestCrop()
    {
        FindObjectOfType<CropTracker>().NewRound();
        Destroy(currentChild);
        Destroy(gameObject);
    }
}

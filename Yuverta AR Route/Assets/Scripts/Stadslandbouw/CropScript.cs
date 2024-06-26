using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropScript : MonoBehaviour
{
    public CropObject cropObject;
    private Transform parent;
    private GameObject currentChild;
    public int growthStage = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        parent = transform;
    }
    
    public void GrowCrop()
    {
        Destroy(currentChild);
        currentChild = Instantiate(cropObject.growthStages[growthStage], parent);
        growthStage++;
    }
    
    public void HarvestCrop()
    {
        FindObjectOfType<CropTracker>().NewRound();
        Destroy(currentChild);
        Destroy(gameObject);
    }
}

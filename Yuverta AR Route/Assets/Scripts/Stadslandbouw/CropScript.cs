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
    private UnityAction fullyGrown;
    
    [SerializeField] private CropContainer cropContainer;
    
    // Start is called before the first frame update
    void Start()
    {
        fullyGrown = cropContainer.Enable;
        parent = transform;
    }
    
    public void GrowCrop()
    {
        Destroy(currentChild);
        currentChild = Instantiate(cropObject.growthStages[growthStage], parent);
        currentChild.tag = "Crop";
        growthStage++;
        Debug.Log(fullyGrown);

        if (growthStage >= cropObject.growthStages.Count)
        {
            fullyGrown.Invoke();
            Debug.Log("Fully grown");
        }
    }
    
    public void HarvestCrop()
    {
        FindObjectOfType<CropTracker>().NewRound();
        Destroy(currentChild);
    }
}

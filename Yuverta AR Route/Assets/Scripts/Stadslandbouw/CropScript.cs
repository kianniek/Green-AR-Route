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
    public List<GameObject> growthStages;
    private GameObject deadCrop;
    
    [SerializeField] private CropContainer cropContainer;
    private bool gotCrop;
    
    //All crops have 6 growth stages
    private const int ChildCount = 6;
    
    // Start is called before the first frame update
    void Start()
    {
        fullyGrown = cropContainer.Enable;
        parent = transform;
    }

    public void NewCrop(CropObject newCrop)
    {
        if (gotCrop) return;
        
        gotCrop = true;
        cropObject = newCrop;
        Instantiate(cropObject.cropPrefab, parent);
        
        for (int i = 0; i < ChildCount; i++)
        {
            growthStages.Add(transform.GetChild(0).GetChild(i).gameObject);
            growthStages[i].SetActive(false);
        }

        for (int i = ChildCount; i < transform.GetChild(0).childCount; i++)
        {
            if (transform.GetChild(0).GetChild(i).gameObject.name.Contains("Dead"))
            {
                deadCrop = transform.GetChild(0).GetChild(i).gameObject;
                deadCrop.SetActive(false);
                break;
            }
        }
        
        growthStage = 0;
        
        GrowCrop();
    }
    
    public void GrowCrop()
    {
        if (currentChild) currentChild.SetActive(false);
        currentChild = growthStages[growthStage];
        currentChild.SetActive(true);
        currentChild.tag = "Crop";
        Debug.Log(fullyGrown);

        if (growthStage == ChildCount - 1 && !cropContainer.rightCrop)
        {
            deadCrop.SetActive(true);
            growthStages[growthStage].SetActive(false);
        }
        
        //Upgrading the growth stage for the next round
        growthStage++;
        
        if (growthStage >= growthStages.Count)
        {
            fullyGrown.Invoke();
            Debug.Log("Fully grown");
        }
    }
    
    public void HarvestCrop()
    {
        FindObjectOfType<CropTracker>().NewRound();
        
        Destroy(transform.GetChild(0).gameObject);
        growthStages.Clear();
        gotCrop = false;
    }
}

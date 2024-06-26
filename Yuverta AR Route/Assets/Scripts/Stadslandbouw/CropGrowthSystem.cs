using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CropGrowthSystem : MonoBehaviour
{
    public List<Button> cropGrowButtons;
    private CropScript currentCrop;
    private CropContainer cropContainer;
    
    private void Start()
    {
        foreach (var button in cropGrowButtons)
        {
            button.onClick.AddListener(() => GrowCrop(button));
        }
        
        cropContainer = FindObjectOfType<CropContainer>();
        cropContainer.onCropPlanted.AddListener(EnableButtons);
        cropContainer.onCropHarvested.AddListener(DisableButtons);
    }
    
    private void GrowCrop(Button button)
    {
        currentCrop.GrowCrop();
    }
    
    public void NewCrop(CropScript cropScript)
    {
        currentCrop = cropScript;
    }
    
    private void EnableButtons()
    {
        foreach (var button in cropGrowButtons)
        {
            button.interactable = true;
        }   
    }

    private void DisableButtons()
    {
        foreach (var button in cropGrowButtons)
        {
            button.interactable = false;
        }
    }
}

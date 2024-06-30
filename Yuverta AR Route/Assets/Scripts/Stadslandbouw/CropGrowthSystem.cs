using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable InconsistentNaming

public class CropGrowthSystem : MonoBehaviour
{
    public List<Button> cropGrowButtons;
    private CropScript currentCrop;
    private CropContainer cropContainer;
    
    private Animator growthAnimator;
    
    private void Start()
    {
        foreach (var button in cropGrowButtons)
        {
            button.onClick.AddListener(() => GrowCrop(button));
        }
        
        cropContainer = FindObjectOfType<CropContainer>();
        
        cropContainer.onCropPlanted += EnableButtons;
        cropContainer.onCropPlanted += () => NewCrop(cropContainer.CropScript);
        cropContainer.onCropHarvested += DisableButtons;
        
        growthAnimator = GetComponent<Animator>();
        
        DisableButtons();
    }
    
    private void GrowCrop(Button button)
    {
        currentCrop.GrowCrop();
        button.interactable = false;
        //Temporarily use the name of the button as the animation name
        growthAnimator.Play(button.name);
    }
    
    private void NewCrop(CropScript cropScript)
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

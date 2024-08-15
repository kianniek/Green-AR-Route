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
    [SerializeField] private CropContainer cropContainer;

    private Animator growthAnimator;

    private void Awake()
    {
        growthAnimator = GetComponent<Animator>();

        DisableButtons();
    }

    private void OnEnable()
    {
        foreach (var button in cropGrowButtons)
        {
            button.onClick.AddListener(() => GrowCrop(button));
        }
        cropContainer.onCropPlanted.AddListener(EnableButtons);
        cropContainer.onCropPlanted.AddListener(NewCrop);

        cropContainer.onCropHarvested.AddListener(DisableButtons);
    }

    private void OnDisable()
    {
        foreach (var button in cropGrowButtons)
        {
            button.onClick.RemoveListener(() => GrowCrop(button));
        }
        cropContainer.onCropPlanted.RemoveListener(EnableButtons);
        cropContainer.onCropPlanted.RemoveListener(NewCrop);

        cropContainer.onCropHarvested.RemoveListener(DisableButtons);
    }

    private void GrowCrop(Button button)
    {
        currentCrop.GrowCrop();
        button.interactable = false;
        //Temporarily use the name of the button as the animation name
        growthAnimator.Play(button.name);
    }

    private void EnableButtons(CropScript cropScript)
    {
        foreach (var button in cropGrowButtons)
        {
            button.interactable = true;
        }
    }

    private void NewCrop(CropScript cropScript)
    {
        currentCrop = cropScript;
    }

    public void DisableButtons()
    {
        foreach (var button in cropGrowButtons)
        {
            button.interactable = false;
        }
    }
}

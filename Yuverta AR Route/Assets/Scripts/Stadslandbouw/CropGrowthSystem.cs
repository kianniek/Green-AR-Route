using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CropGrowthSystem : MonoBehaviour
{
    public List<Button> cropGrowButtons;
    private CropScript currentCrop;
    [SerializeField] private CropContainer cropContainer;

    private Animator growthAnimator;

    // Store the listener to allow both Add and Remove
    private List<UnityEngine.Events.UnityAction> buttonListeners = new List<UnityEngine.Events.UnityAction>();

    private void Awake()
    {
        growthAnimator = GetComponent<Animator>();
        DisableButtons();
    }

    private void OnEnable()
    {
        for (int i = 0; i < cropGrowButtons.Count; i++)
        {
            int index = i; // Avoid closure issue
            UnityEngine.Events.UnityAction listener = () => GrowCrop(cropGrowButtons[index]);
            buttonListeners.Add(listener);
            cropGrowButtons[i].onClick.AddListener(listener);
        }
        
        cropContainer.onCropPlanted.AddListener(EnableButtons);
        cropContainer.onCropPlanted.AddListener(NewCrop);
        cropContainer.onCropHarvested.AddListener(DisableButtons);
    }

    private void OnDisable()
    {
        for (int i = 0; i < cropGrowButtons.Count; i++)
        {
            cropGrowButtons[i].onClick.RemoveListener(buttonListeners[i]);
        }
        buttonListeners.Clear();
        
        cropContainer.onCropPlanted.RemoveListener(EnableButtons);
        cropContainer.onCropPlanted.RemoveListener(NewCrop);
        cropContainer.onCropHarvested.RemoveListener(DisableButtons);
    }

    private void GrowCrop(Button button)
    {
        currentCrop.GrowCrop();
        button.interactable = false;
        growthAnimator.Play(button.name);  // Temporarily use the name of the button as the animation name
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

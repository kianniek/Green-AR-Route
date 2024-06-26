using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(CropTracker), typeof(CropGrowthSystem))]
public class CropContainer : MonoBehaviour
{
    [Tooltip("This list contains the spawn locations for the seeds the crop tracker spawns")]
    public List<Transform> seedsSpawnLocations; 
    public Transform cropSpawnLocation;
    [SerializeField] private TextMeshPro cropNameDisplay;
    public CropScript currentCropObject;
    private CropScript lastCorrectCropObject;
    private CropScript lastCropObject;
    private bool rightCrop;
    
    public UnityEvent onCropHarvested;
    public UnityEvent onCropPlanted;
    
    [SerializeField] private InputActionProperty dragDeltaAction;
    private bool hitCrop;
    private bool newCrop;

    public void NewCrop(CropScript newCropObject)
    {
        if (rightCrop)
        {
            lastCorrectCropObject = currentCropObject;
        }
        else lastCropObject = currentCropObject;
        
        currentCropObject = newCropObject;
        rightCrop = lastCorrectCropObject.cropObject.nextCrop == currentCropObject.cropObject.cropName;
        
        //Spawning the first stage of the crop
        newCropObject.GrowCrop();
        cropNameDisplay.text = currentCropObject.cropObject.cropName;
        onCropPlanted.Invoke();
        
        onCropHarvested.AddListener(newCropObject.HarvestCrop);
        
        dragDeltaAction.action.Enable();
        newCrop = true;
    }
    
    public void HarvestCrop()
    {
        if (currentCropObject.growthStage != currentCropObject.cropObject.growthStages.Count - 1) return;
        
        onCropHarvested.Invoke();
        dragDeltaAction.action.Disable();
    }
    
    private void Update()
    {
        if (dragDeltaAction.action != null && dragDeltaAction.action.triggered)
        {
            Vector2 touchPosition = dragDeltaAction.action.ReadValue<Vector2>();

            if (Touchscreen.current.primaryTouch.press.isPressed)
            {
                var eventData = new PointerEventData(EventSystem.current)
                {
                    position = touchPosition
                };
                OnPointerDown(eventData);
            }
            else if (Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                var eventData = new PointerEventData(EventSystem.current)
                {
                    position = touchPosition
                };
                OnDrag(eventData);
            }
            else if (Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                var eventData = new PointerEventData(EventSystem.current)
                {
                    position = touchPosition
                };
                OnEndDrag(eventData);
                OnPointerUp(eventData);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Send a ray from the touch position into the world to check if it collides with objects
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //If the ray hits a crop, set the drag object to the crop
            if (hit.collider.CompareTag("Crop"))
            {
                hitCrop = true;
                
                if (!newCrop)
                {
                    FindObjectOfType<CropTracker>().PickedSeed(hit.collider.name);
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (hitCrop)
        {
            HarvestCrop();
        }
        
        newCrop = false;
        hitCrop = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }
}

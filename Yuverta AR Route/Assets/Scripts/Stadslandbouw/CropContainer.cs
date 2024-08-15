using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(CropTracker),typeof(CropGrowthSystem))]
public class CropContainer : MonoBehaviour
{
    [Tooltip("This is the location where the crop will spawn when the seed is planted.")]
    [SerializeField] private Transform cropSpawnLocation;
    
    [Tooltip("This is the text that will display the name of the crop.")]
    [SerializeField]private TextMeshPro cropDisplayName;
    
    [SerializeField] private CropScript cropScript;
    
    public CropScript CropScript => cropScript;
    
    public Transform CropSpawnLocation => cropSpawnLocation;
    
    private bool rightCrop;
    private Camera _camera;
    
    public bool currentCropIsRightCrop => rightCrop;
    
    public UnityEvent onCropHarvested = new ();
    public UnityEvent<CropScript> onCropPlanted = new ();

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                HarvestCrop(touch.position);
            }
        }
    }

    public void NewCrop(CropScript newCropObject, CropObject.CropType nextCorrectCropType)
    {
        // Update the current crop script
        cropScript = newCropObject;
    
        // Determine if the new crop is the correct one
        rightCrop = nextCorrectCropType == cropScript.cropObject.currentCropType || nextCorrectCropType == CropObject.CropType.none;
        Debug.Log("Correct crop: " + cropScript.cropObject.currentCropType);
        
        // Update the display with the new crop's name
        cropDisplayName.text = cropScript.cropObject.cropName;
    
        // Invoke the crop planted event
        onCropPlanted.Invoke(cropScript);
    }

    private void HarvestCrop(Vector2 touchPosition)
    {
        // Check if the crop is fully grown
        if (!cropScript.IsFullyGrown) 
            return;
        
        // Check with a raycast if the touch actually hit the crop
        var ray = _camera!.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.collider.gameObject.CompareTag("Crop"))
            {
                Debug.Log("Crop harvested");
                onCropHarvested.Invoke();
                cropScript.HarvestCrop();
            }
        }
    }
}

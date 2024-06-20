using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;

public class ARTapToPlaceObject : MonoBehaviour
{
    public GameObject objectToPlace;
    public GameObject placementIndicator;
    private ARRaycastManager arRaycastManager;
    private Pose placementPose;
    private bool placementPoseIsValid = false;

    public InputActionAsset inputActionAsset;
    private InputAction tapAction;
    private Camera arCamera;

    void Start()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        arCamera = Camera.main;

        // Get the tap action from the input action asset
        var arInputActions = inputActionAsset.FindActionMap("TouchScreen");
        tapAction = arInputActions.FindAction("Tap");

        // Enable the tap action
        tapAction.Enable();

        // Subscribe to the performed event of the tap action
        tapAction.performed += OnTapPerformed;
    }

    void OnDestroy()
    {
        // Unsubscribe from the performed event when the script is destroyed
        tapAction.performed -= OnTapPerformed;
    }

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();
    }

    private void OnTapPerformed(InputAction.CallbackContext context)
    {
        if (placementPoseIsValid)
        {
            PlaceObject();
        }
    }

    private void PlaceObject()
    {
        Instantiate(objectToPlace, placementPose.position, placementPose.rotation);
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = arCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);
        
        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;
            
            var cameraForward = arCamera.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }
}

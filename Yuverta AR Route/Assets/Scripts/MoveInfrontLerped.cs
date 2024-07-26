using System;
using System.Collections;
using UnityEngine;

public class MoveInfrontLerped : MonoBehaviour
{
    [SerializeField] private GameObject parentObject;

    [Tooltip("The distance the quiz elements should be from the camera")] [SerializeField]
    private float distanceFromCamera;

    [SerializeField] private bool keepInFrontOfCamera = true;

    [Header("Lerp Settings")] public float lerpSpeed = 5f;
    [SerializeField] private float lerpThreshold = 0.1f;

    private Camera mainCamera;
    private Vector3 boundingBoxCenterOffset;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (parentObject == null)
        {
            parentObject = gameObject;
        }

        // Calculate bounding box center offset
        CalculateBoundingBoxCenterOffset();
    }

    private void Start()
    {
        PositionAndRotateParentObject();
    }

    private void Update()
    {
        KeepObjectInfrontOfCamera();
    }

    private void CalculateBoundingBoxCenterOffset()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            var bounds = meshRenderer.bounds;
            boundingBoxCenterOffset = parentObject.transform.position - bounds.center;
        }
        else
        {
            boundingBoxCenterOffset = Vector3.zero;
        }
    }

    private void PositionAndRotateParentObject()
    {
        var newPosition = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera +
                          boundingBoxCenterOffset;
        parentObject.transform.position = newPosition;
        parentObject.transform.LookAt(mainCamera.transform);
    }

    private void KeepObjectInfrontOfCamera()
    {
        if (!keepInFrontOfCamera) 
            return;
        
        var newPosition = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera +
                          boundingBoxCenterOffset;
        if (Vector3.Distance(parentObject.transform.position, newPosition) > lerpThreshold)
        {
            parentObject.transform.position = Vector3.Slerp(parentObject.transform.position, newPosition,
                lerpSpeed * Time.deltaTime);
        }

        parentObject.transform.LookAt(mainCamera.transform);
    }
}
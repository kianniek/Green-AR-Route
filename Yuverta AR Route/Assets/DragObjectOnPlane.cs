using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragObjectOnPlane : MonoBehaviour
{
    private Camera arCamera;

    private bool isDragging = false;
    private Transform selectedObject;

    private Vector3 offset;

    [SerializeField]
    private float lerpSpeed = 10f;
    private Vector3 targetPosition;
    [SerializeField] private float fallBackDistance = 5f;
    
    [SerializeField] private string tagToDrag = "WorldUI";

    private Plane horizontalPlane;
    void Start()
    {
        arCamera = Camera.main;
        targetPosition = transform.position;
    }

    void Update()
    {
        if (isDragging)
        {
            LerpToPosition();
        }
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleTouchBegan(touch);
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        HandleTouchMoved(touch);
                    }
                    break;

                case TouchPhase.Ended:
                    if (isDragging)
                    {
                        HandleTouchEnded();
                    }
                    break;
            }
        }
    }

    private void HandleTouchBegan(Touch touch)
    {
        Ray ray = arCamera.ScreenPointToRay(touch.position);
        RaycastHit hitObject;

        if (Physics.Raycast(ray, out hitObject))
        {
            if (hitObject.transform.CompareTag(tagToDrag))
            {
                selectedObject = hitObject.transform;
                isDragging = true;

                Vector3 screenPoint = arCamera.WorldToScreenPoint(selectedObject.position);
                horizontalPlane = new Plane(Vector3.up, selectedObject.position);
            }
        }
    }

    private void HandleTouchMoved(Touch touch)
    {
        Ray ray = arCamera.ScreenPointToRay(touch.position);
        float distance;

        if (horizontalPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            targetPosition = new Vector3(hitPoint.x, selectedObject.position.y, hitPoint.z);
        }
    }

    private void HandleTouchEnded()
    {
        isDragging = false;
        selectedObject = null;
    }

    private void LerpToPosition()
    {
        //if the object position is out of the far clipping plane, set target position
        var distance = Vector3.Distance(selectedObject.position, arCamera.transform.position);
        if (distance > arCamera.farClipPlane)
        {
            var direction = (selectedObject.position - arCamera.transform.position).normalized;
            direction.y = selectedObject.position.y;
            targetPosition = direction * fallBackDistance;
        }
        
        selectedObject.position = Vector3.Lerp(selectedObject.position, targetPosition, Time.deltaTime * lerpSpeed);
    }
}

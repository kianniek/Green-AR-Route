using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SharedFunctionality : MonoBehaviour
{
    public static SharedFunctionality Instance;
    
    public static bool IsQuitting { get; private set; }

    private void OnApplicationQuit()
    {
        IsQuitting = true;
    }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    public RaycastHit[] TouchToRay()
    {
        Vector2 touchPosition;
        try
        {
            touchPosition = Input.GetTouch(0).position;
        }
        catch (Exception e)
        {
            touchPosition = Input.mousePosition;
        }
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        var hits = Physics.RaycastAll(ray);
        return hits;
    }

    public bool TouchUI()
    {
        if (EventSystem.current != null)
        {
            if (EventSystem.current.IsPointerOverGameObject(-1)) return true;
            if (Input.touchCount > 0 && 
                EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return true;
        }

        return false;
    }
    
    public Vector2 WorldToCanvasPosition(Canvas canvas, Camera camera, Vector3 worldPosition)
    {
        // Convert the world position to screen position
        Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition);

        // Convert the screen position to a position relative to the canvas
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPosition, canvas.worldCamera, out Vector2 canvasPosition);
        
        return screenPosition;
    }

    public Vector3 ObjectMovement(ARRaycastManager raycastManager, GameObject obj)
    {
        // Create a list to hold the hit results
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        Vector3 position = Application.isEditor ? Input.mousePosition : Input.GetTouch(0).position;

        // Raycast from the touch position
        Vector3 rayCast = ARRayCast(raycastManager, position, obj, hits);

        if (rayCast != Vector3.zero)
        {
            return new Vector3(rayCast.x, obj.transform.position.y, rayCast.z);
        }

        // Physics raycast as fallback
        var ray = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 hit2 = ARRayCast(raycastManager, hit.point, obj, hits);
            if (hit2 != Vector3.zero)
            {
                return new Vector3(hit2.x, obj.transform.position.y, hit2.z);
            }
        }

        return obj.transform.position;
    }

    Vector3 ARRayCast(ARRaycastManager raycastManager, Vector3 position, GameObject gameObject, List<ARRaycastHit> hits)
    {
        if (raycastManager.Raycast(position, hits, TrackableType.Planes))
        {
            // Get the hit position
            Pose hitPose = hits[0].pose;

            // Calculate the adjusted position at the fixed y-level
            Vector3 adjustedPosition = GetPointOnYPlane(hitPose.position, gameObject.transform.position.y);

            // Move the object to the adjusted position
            return Vector3.Lerp(gameObject.transform.position, adjustedPosition, Time.deltaTime * 10);
        }

        return Vector3.zero;
    }

    /// <summary>
        /// Calculate the objects position relative to its y-level and the last touch/mouse position.
        /// </summary>
        Vector3 GetPointOnYPlane(Vector3 hitPosition, float yLevel)
        {
            // Create a ray from the camera through the touch position
            Vector3 touchPosition = Application.isEditor ? Input.mousePosition : Input.GetTouch(0).position;
            Ray ray = Camera.main!.ScreenPointToRay(touchPosition);
    
            // Calculate the distance to the yLevel plane from the camera
            float distanceToYLevel = (yLevel - ray.origin.y) / ray.direction.y;
    
            // Get the point of intersection
            Vector3 pointOnYPlane = ray.origin + ray.direction * distanceToYLevel;
    
            return new Vector3(pointOnYPlane.x, yLevel, pointOnYPlane.z);
        }


    /// <summary>
    /// Calculate the objects position relative to its y-level and the last touch/mouse position.
    /// </summary>
    
    
    public Vector3 GetTouchWorldPosition()
    {
        if (Input.touchCount <= 0) return GetMouseWorldPosition();
    
        var touchPosition = Input.GetTouch(0).position;
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        
        var rayCastHit = new List<ARRaycastHit>();
        return FindObjectOfType<ARRaycastManager>().Raycast(ray, rayCastHit) ? rayCastHit[0].pose.position : Vector3.zero;
    }
    
    //Debug
    public static Vector3 GetMouseWorldPosition()
    {
        var ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out var rayCastHit) ? rayCastHit.point : Vector3.zero;
    }
}

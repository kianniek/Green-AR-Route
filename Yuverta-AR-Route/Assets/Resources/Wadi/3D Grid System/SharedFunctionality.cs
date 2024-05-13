using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;

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
            DontDestroyOnLoad(gameObject);
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

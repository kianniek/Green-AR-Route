using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CameraHeightOffGround : MonoBehaviour
{
    // Reference to the ARCamera
    public Camera arCamera;
    private ARRaycastManager raycastManager;

    void Start()
    {
        // Get the ARRaycastManager component
        raycastManager = FindObjectOfType<ARRaycastManager>();

        // Make sure the camera reference is assigned
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
    }

    void Update()
    {
        // Perform a raycast downward from the camera position
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        Vector3 cameraPosition = arCamera.transform.position;
        Vector2 screenCenter = arCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));
        
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            // Get the closest hit plane
            ARRaycastHit hit = hits[0];
            float groundHeight = hit.pose.position.y;

            // Calculate the camera height off the ground
            float cameraHeight = cameraPosition.y - groundHeight;

            // Output the camera height to the console or use it in your logic
            Debug.Log("Camera Height Off the Ground: " + cameraHeight);
        }
        else
        {
            Debug.Log("No ground plane detected.");
        }
    }
    
    public Vector3 GetGroundPosition()
    {
        // Perform a raycast downward from the camera position
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        Vector3 cameraPosition = arCamera.transform.position;
        Vector2 screenCenter = arCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));
        
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            // Get the closest hit plane
            ARRaycastHit hit = hits[0];
            return hit.pose.position;
        }
        else
        {
            Debug.Log("No ground plane detected.");
            return Vector3.zero;
        }
    }

    public Vector3[] GetGroundPositionsAround()
    {
        Vector3[] positions = new Vector3[4];
        Vector3[] aroundPos = new Vector3[4];
    
        aroundPos[0] = Vector3.zero;
        aroundPos[1] = Vector3.forward;
        aroundPos[2] = Vector3.right;
        aroundPos[3] = -Vector3.forward;
        aroundPos[4] = -Vector3.right;
        
        // Perform a raycast downward from the camera position
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        Vector3 cameraPosition = arCamera.transform.position;
    
        for (int i = 0; i < aroundPos.Length; i++)
        {
            // Ar raycast hit
            Vector3 direction = aroundPos[i];
            Vector3 screenCenter = arCamera.transform.position + direction;
            
            if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
            {
                // Get the closest hit plane
                ARRaycastHit hit = hits[0];
                positions[i] = hit.pose.position;
            }
            else
            {
                Debug.Log("No ground plane detected.");
            }
        }
        
        return positions;
    }

    public Vector3 GetGroundPlanePos()
    {
        var g= GetGroundPositionsAround();
        
        //get the postion that is the lowest
        Vector3 groundPlane = g[0];
        for (int i = 1; i < g.Length; i++)
        {
            if (g[i].y < groundPlane.y)
            {
                groundPlane = g[i];
            }
        }

        return groundPlane;
    }
}
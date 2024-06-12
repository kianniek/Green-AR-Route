using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Button = UnityEngine.UI.Button;

public class DisplayVideoManager : BaseManager
{
    public static DisplayVideoManager Instance;
    public ImageTracking imageTracking;
    private ARRaycastManager raycastManager;

    private GameObject videoPlayerObj;
    private VideoPlayerScript videoPlayerScript;

    [NonSerialized] public GameObject dragObject;
    [Tooltip("The object to spawn when the video ends")]
    [SerializeField] private GameObject objectToSpawn;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        //Setting up swipe detection
        SwipeDetection.Instance.currentManager = this;
        SwipeDetection.Instance.tagToCheck = "VideoPlayer";
        
        //Finding scripts
        imageTracking = FindObjectOfType<ImageTracking>();
        raycastManager = FindObjectOfType<ARRaycastManager>();
        
        //Temporary testing code
        VideoPlayerSpawned(FindObjectOfType<VideoPlayerScript>().gameObject);
        /*videoPlayerScript = FindObjectOfType<VideoPlayerScript>();
        videoPlayerObj = videoPlayerScript.gameObject;*/
    }

    public void VideoPlayerSpawned(GameObject videoPlayer)
    {
        videoPlayerObj = videoPlayer;
        videoPlayerScript = videoPlayerObj.GetComponent<VideoPlayerScript>();
        dragObject = videoPlayer.gameObject.transform.GetChild(0).gameObject;
        videoPlayerScript.videoPlayer.loopPointReached += VideoEnded;
    }

    private void VideoEnded(VideoPlayer videoPlayer)
    {
        Instantiate(objectToSpawn);
        Destroy(videoPlayerObj);
        Destroy(gameObject);
    }

    public override void SelectedObject(GameObject selectedObject)
    {
        Debug.Log(selectedObject.tag);
        if (selectedObject.CompareTag("VideoPlayer"))
        {
            videoPlayerScript.OnClick();
            return;
        }
        
        Debug.Log("Hit");
        StartCoroutine(OnUIDrag());
    }

    public override void UpdateObject()
    {
        videoPlayerScript.OnClick();
    }

    private IEnumerator OnUIDrag()
    {
        while (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            Debug.Log("Dragging");
            videoPlayerObj.transform.position = ObjectMovement(raycastManager, videoPlayerObj);
            yield return new WaitForFixedUpdate();
        }
    }
    
    private Vector3 ObjectMovement(ARRaycastManager raycastManager, GameObject obj)
    {
        Debug.Log("Object Movement called");

        // Create a list to hold the hit results
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        // Determine the input position based on the platform
        Vector3 position = Application.isEditor ? (Vector3)Input.mousePosition : (Vector3)Input.GetTouch(0).position;
        Debug.Log("Input position: " + position);

        // Try AR raycast
        Vector3 arRaycastPosition = ARRayCast(raycastManager, position, obj, hits);
        if (arRaycastPosition != Vector3.zero)
        {
            Debug.Log("AR Raycast hit position: " + arRaycastPosition);
            return new Vector3(arRaycastPosition.x, obj.transform.position.y, arRaycastPosition.z);
        }

        Debug.Log("AR Raycast failed");

        // Try physics raycast as fallback
        var ray = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Physics Raycast hit: " + hit.point);
            Vector3 fallbackRaycastPosition = ARRayCast(raycastManager, hit.point, obj, hits);
            if (fallbackRaycastPosition != Vector3.zero)
            {
                Debug.Log("Fallback AR Raycast hit position: " + fallbackRaycastPosition);
                return new Vector3(fallbackRaycastPosition.x, obj.transform.position.y, fallbackRaycastPosition.z);
            }
        }

        Debug.Log("Both Raycasts failed");

        // Return the original position if no valid hit
        return obj.transform.position;
    }

    Vector3 ARRayCast(ARRaycastManager raycastManager, Vector3 position, GameObject gameObject, List<ARRaycastHit> hits)
    {
        if (raycastManager.Raycast(position, hits, TrackableType.Planes))
        {
            // Get the hit position
            Pose hitPose = hits[0].pose;
            Debug.Log("ARRaycast hit: " + hitPose.position);

            // Calculate the adjusted position at the fixed y-level
            Vector3 adjustedPosition = GetPointOnYPlane(hitPose.position, gameObject.transform.position.y);
            Debug.Log("Adjusted position: " + adjustedPosition);

            return adjustedPosition; // Return the adjusted position directly
        }

        return Vector3.zero; // Return zero vector if no hit
    }

    /*Vector3 GetPointOnYPlane(Vector3 hitPosition, float yLevel)
    {
        // Create a ray from the camera through the touch position
        Vector3 touchPosition = Application.isEditor ? Input.mousePosition : Input.GetTouch(0).position;
        Ray ray = Camera.main!.ScreenPointToRay(touchPosition);
    
        // Calculate the distance to the yLevel plane from the camera
        float distanceToYLevel = (yLevel - ray.origin.y) / ray.direction.y;
    
        // Get the point of intersection
        Vector3 pointOnYPlane = ray.origin + ray.direction * distanceToYLevel;
    
        return new Vector3(pointOnYPlane.x, yLevel, pointOnYPlane.z);
    }*/
    
    Vector3 GetPointOnYPlane(Vector3 hitPosition, float yLevel)
    {
        // Simply adjust the y-coordinate to the fixed y-level
        Vector3 pointOnYPlane = new Vector3(hitPosition.x, yLevel, hitPosition.z);
        Debug.Log("Calculated point on Y plane: " + pointOnYPlane);

        return pointOnYPlane;
    }
}

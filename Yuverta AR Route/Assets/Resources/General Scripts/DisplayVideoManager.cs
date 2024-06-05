using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using UnityEngine.XR.ARFoundation;
using Button = UnityEngine.UI.Button;

public class DisplayVideoManager : BaseManager
{
    public static DisplayVideoManager Instance;
    public ImageTracking imageTracking;
    private ARRaycastManager raycastManager;

    private GameObject videoPlayerObj;
    private VideoPlayerScript videoPlayerScript;

    public GameObject dragObject;
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
            videoPlayerObj.transform.position = SharedFunctionality.Instance.ObjectMovement(raycastManager, videoPlayerObj);
            yield return new WaitForFixedUpdate();
        }
    }
}

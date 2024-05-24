using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class DisplayVideoHandler : BaseManager
{
    public static DisplayVideoHandler Instance;
    public ImageTracking imageTracking;

    private GameObject videoPlayerObj;
    private VideoPlayerScript videoPlayerScript;
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
        imageTracking = FindObjectOfType<ImageTracking>();
        SwipeDetection.Instance.currentManager = this;
        SwipeDetection.Instance.tagToCheck = "VideoPlayer";
        
        //Temporary testing code
        videoPlayerScript = FindObjectOfType<VideoPlayerScript>();
        videoPlayerObj = videoPlayerScript.gameObject;
    }

    public void VideoPlayerSpawned(GameObject videoPlayer)
    {
        videoPlayerObj = videoPlayer;
        videoPlayerScript = videoPlayerObj.GetComponent<VideoPlayerScript>();
    }
    
    public override void SelectedObject(GameObject selectedObject)
    {
        videoPlayerScript.OnClick();
    }

    public override void UpdateObject()
    {
        videoPlayerScript.OnClick();
    }
    
}

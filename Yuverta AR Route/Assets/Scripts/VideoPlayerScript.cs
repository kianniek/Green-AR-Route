using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class VideoPlayerScript : MonoBehaviour
{
    [SerializeField] public VideoPlayer videoPlayer;

    //public string videoUrl;
    [SerializeField] public VideoClip videoClip;

    [SerializeField] public UnityEvent onVideoEnd = new();

    
    private Camera _mainCamera;
    
    public void Start()
    {
        _mainCamera = Camera.main;
        videoPlayer.clip = videoClip; //When this is on the clip is loaded first instead of the url
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.Prepare();
        videoPlayer.Pause();
    }

    public void OnClick()
    {
        switch (videoPlayer.isPaused)
        {
            case false:
                videoPlayer.Pause();
                break;

            case true:
                videoPlayer.Play();
                break;
        }
    }

    private void Update()
    {
        //check if video has ended
        if (videoPlayer.isPlaying && videoPlayer.frame >= (long)videoPlayer.frameCount - 1)
        {
            videoPlayer.Pause();
            onVideoEnd.Invoke();
        }

        //Check if touch input is detected
        if (Input.touchCount <= 0) 
            return;
        
        var touch = Input.GetTouch(0);

        if (touch.phase != TouchPhase.Began) 
            return;
        
        var ray = _mainCamera.ScreenPointToRay(touch.position);
        
        if (!Physics.Raycast(ray, out var hit)) 
            return;
        
        if (hit.collider.CompareTag("VideoPlayer"))
        {
            OnClick();
        }
    }
}
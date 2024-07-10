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
    
    [SerializeField] public UnityEvent onVideoEnd = new ();

    public void Start()
    {
        //videoPlayer.url = SetVideoUrl(); //Also work with URL's
        videoPlayer.clip = videoClip; //When this is on the clip is loaded first instead of the url
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource; //It works it is just a bit loud at the moment
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.Prepare();  
        videoPlayer.Pause();
        //gameObject.transform.LookAt(Camera.main!.transform);
        StartCoroutine(FollowUser());
    }

    private string SetVideoUrl()
    {
        var folderPath = Path.Combine(Application.streamingAssetsPath, "videos");
        // Load all video file paths from the folder
        var videoPaths = Directory.GetFiles(folderPath, "*.mp4"); // Change "*.mp4" to match your video file extension

        if (videoPaths.Length == 0)
        {
            return null;
        }
        
        var newIndex = Random.Range(0, videoPaths.Length);

        // Set the video clip to the VideoPlayer
        return videoPaths[newIndex];
        
        //DevTesting
        //PlayVideo();
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
                StartCoroutine(FollowUser());
                break;
        }
    }

    private IEnumerator FollowUser()
    {
        while (!videoPlayer.isPaused)
        {
            gameObject.transform.LookAt(Camera.main!.transform);
            yield return new WaitForFixedUpdate();
        }
    }

    private void Update()
    {
        //check if video has ended
        if (videoPlayer.isPlaying && videoPlayer.frame >= (long) videoPlayer.frameCount - 1)
        {
            videoPlayer.Pause();
            onVideoEnd.Invoke();
        }
        
        //Check if touch input is detected
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                var ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.collider.CompareTag("VideoPlayer"))
                    {
                        OnClick();
                    }
                }
            }
        }
    }
}

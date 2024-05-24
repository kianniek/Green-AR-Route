using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class VideoPlayerScript : MonoBehaviour
{
    [SerializeField] public VideoPlayer videoPlayer;
    //private const string folderPath = @"C:\Users\Trist\OneDrive\Documenten\GitHub\Onder-het-Maaiveld\Onder-het-Maaiveld\Assets\Resources\Videos"; // Path to the folder containing videos
    //public string videoUrl;
    [SerializeField] public VideoClip videoClip;

    public void Start()
    {
        //videoPlayer.url = SetVideoUrl(); //Also work with URL's
        videoPlayer.clip = videoClip; //When this is on the clip is loaded first instead of the url
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource; //It works it is just a bit loud at the moment
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.Prepare();  
        videoPlayer.Pause();
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
        if (!videoPlayer.isPlaying && !videoPlayer.isPaused)
        {
            videoPlayer.Play();
            return;
        }
        
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

    private IEnumerator FollowUser()
    {
        while (!videoPlayer.isPaused)
        {
            gameObject.transform.LookAt(Camera.main!.transform);
            Debug.Log("Following user");
            yield return new WaitForFixedUpdate();
        }
        
        if (!videoPlayer.isPlaying) yield break;

        while (videoPlayer.isPlaying && videoPlayer.isPaused)
        {
            yield return new WaitForFixedUpdate();
        }
    }
}

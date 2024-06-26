using UnityEngine;

public class VoiceOverManager : MonoBehaviour
{
    VoiceOverManager instance;
    private bool playingClip;
    private Camera mainCamera;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void PlayVoiceOver(AudioClip voiceOver)
    {
        if (playingClip)
        {
            AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
            audioSources[0].Stop();
        };
        AudioSource.PlayClipAtPoint(voiceOver, mainCamera.transform.position);
        playingClip = true;
    }
}

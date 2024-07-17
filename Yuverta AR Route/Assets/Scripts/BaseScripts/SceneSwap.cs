using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSwap : MonoBehaviour
{
    // Singleton instance
    public static SceneSwap Instance { get; private set; }

    // UI Slider to display loading progress
    public Slider loadingSlider;

    private void Awake()
    {
        // Ensure there's only one instance of the SceneSwap class
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Function to switch to the next scene
    public void SwitchToNextScene()
    {
        int nextSceneIndex = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings;
        LoadScene(nextSceneIndex);
    }

    // Function to switch to the previous scene
    public void SwitchToPreviousScene()
    {
        int prevSceneIndex = (SceneManager.GetActiveScene().buildIndex - 1 + SceneManager.sceneCountInBuildSettings) % SceneManager.sceneCountInBuildSettings;
        LoadScene(prevSceneIndex);
    }

    // Function to switch to a specific scene
    public void SwitchToScene(int sceneIndex)
    {
        Debug.Log($"Switching to scene: {sceneIndex}, total scenes: {SceneManager.sceneCountInBuildSettings}");
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            //LoadScene(sceneIndex);
            SceneManager.LoadScene(sceneIndex);
            Debug.Log("loading scene name: " + SceneManager.GetSceneAt(sceneIndex).name);
        }
        else
        {
            Debug.LogError("Scene index out of range: " + sceneIndex);
        }
    }

    private void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneAsync(sceneIndex));
    }

    IEnumerator LoadSceneAsync(int sceneIndex)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        if (loadingSlider == null)
        {
            loadingSlider = GetComponentInChildren<Slider>();
        }
        
        while (!asyncLoad.isDone)
        {
            // Update the slider's value based on the progress
            if (loadingSlider != null)
            {
                loadingSlider.value = asyncLoad.progress;
            }

            // Check if the load has finished
            if (asyncLoad.progress >= 0.9f)
            {
                // Loading completed
                loadingSlider.value = 1f;
            }

            yield return null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSwap : MonoBehaviour
{
    // Singleton instance
    public static SceneSwap Instance { get; private set; }

    // List of scene indexes from build settings
    private List<int> sceneIndexes = new List<int>();

    // UI Slider to display loading progress
    public Slider loadingSlider;

    private void Awake()
    {
        // Ensure there's only one instance of the SceneSwap class
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSceneIndexes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Function to initialize the scene indexes list from build settings
    private void InitializeSceneIndexes()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            Debug.Log("Scene " + i + ": " + SceneUtility.GetScenePathByBuildIndex(i));
            sceneIndexes.Add(i);
        }
    }

    // Function to switch to the next scene
    public void SwitchToNextScene()
    {
        int nextSceneIndex = (SceneManager.GetActiveScene().buildIndex + 1) % sceneIndexes.Count;
        LoadScene(nextSceneIndex);
    }

    // Function to switch to the previous scene
    public void SwitchToPreviousScene()
    {
        int prevSceneIndex = (SceneManager.GetActiveScene().buildIndex - 1 + sceneIndexes.Count) % sceneIndexes.Count;
        LoadScene(prevSceneIndex);
    }

    // Function to switch to a specific scene
    public void SwitchToScene(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < sceneIndexes.Count)
        {
            LoadScene(sceneIndex);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseSceneSwapScript : MonoBehaviour
{
    // List of scenes to be assigned in the editor
    [SerializeField]
    private string[] scenes;

    // Index of the current scene
    private int currentSceneIndex = 0;

    // Function to switch to the next scene
    public void SwitchToNextScene()
    {
        if (scenes.Length == 0)
        {
            Debug.LogWarning("No scenes assigned in the SceneSwitcher script.");
            return;
        }

        // Increment the scene index and loop back if necessary
        currentSceneIndex = (currentSceneIndex + 1) % scenes.Length;

        // Load the next scene
        SceneManager.LoadScene(scenes[currentSceneIndex]);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // This function will be called to move to the next scene
    public void LoadNextScene()
    {
        // Get the current scene index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Calculate the index for the next scene
        int nextSceneIndex = currentSceneIndex + 1;

        // Check if the next scene index exceeds the total number of scenes
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // If not, load the next scene
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // If it does, either loop back to the start or handle it as needed
            Debug.Log("You are at the last scene.");
            // Example: Loop back to the first scene (optional)
            // SceneManager.LoadScene(0);
        }
    }
}

using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class DisableDetectedPlanes : MonoBehaviour
{
    // Reference to the ARPlaneManager component
    private ARPlaneManager arPlaneManager;

    void Awake()
    {
        // Get the ARPlaneManager component from the same GameObject
        arPlaneManager = GetComponent<ARPlaneManager>();
    }

    // Method to disable all detected planes and stop plane detection
    public void DisableAllPlanesAndStopDetection()
    {
        // Stop detecting new planes
        arPlaneManager.enabled = false;

        // Loop through each detected plane and disable it
        foreach (ARPlane plane in arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }

        // Optionally, you can log the action
        Debug.Log("All detected planes have been disabled and plane detection stopped.");
    }
}
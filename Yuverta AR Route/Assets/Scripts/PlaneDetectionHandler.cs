using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

public class PlaneDetectionHandler : MonoBehaviour
{
    // Reference to the ARPlaneManager component
    private ARPlaneManager arPlaneManager;

    // Unity Event that will be invoked when a plane is detected for the first time
    [SerializeField]
    private UnityEvent onFirstPlaneDetected;

    // Boolean flag to ensure the event is only invoked once
    private bool planeDetected = false;

    void Awake()
    {
        // Get the ARPlaneManager component from the same GameObject
        arPlaneManager = GetComponent<ARPlaneManager>();
    }

    void OnEnable()
    {
        // Subscribe to the planesChanged event
        arPlaneManager.planesChanged += OnPlanesChanged;
    }

    void OnDisable()
    {
        // Unsubscribe from the planesChanged event
        arPlaneManager.planesChanged -= OnPlanesChanged;
    }

    // Method that is called whenever the planesChanged event is triggered
    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Check if any new planes have been added
        if (!planeDetected && args.added != null && args.added.Count > 0)
        {
            // Set the flag to true to prevent the event from being invoked again
            planeDetected = true;

            // Invoke the Unity Event
            onFirstPlaneDetected.Invoke();

            // Optionally, you can log or perform additional actions here
            Debug.Log("First plane detected and event invoked.");
        }
    }
}
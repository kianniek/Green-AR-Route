using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMainCamera : MonoBehaviour
{
    // Offset from the camera
    public Vector3 offset = new Vector3(0, 0, 0);
    
    private Camera mainCamera;
    
    void Awake()
    {
        // Get the main camera reference
        mainCamera = Camera.main;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // Optionally, you could initialize or log something here
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Keep the position of the object in front of the camera even when the camera rotates
        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, 0.1f);
        
        // Follow camera forward
        transform.forward = mainCamera.transform.forward;
    }
}
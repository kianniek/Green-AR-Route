using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARRecenter : MonoBehaviour
{
    //store relative position and rotation
    private Vector3 relativePosition;

    private void Awake()
    {
        //store relative position and rotation to the camera
        relativePosition = transform.position - Camera.main.transform.position;
    }


    private void OnEnable()
    {
        ARRecenterManager.Instance.AddRecenter(this);
    }
    
    private void OnDisable()
    {
        ARRecenterManager.Instance.RemoveRecenter(this);
    }

    public void Recenter(Vector3 newPosition)
    {
        //center the object horizontally using the collective bounding box off all the child objects
        var bounds = new Bounds();
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }
        
        //calculate the center of the bounding box
        var center = bounds.center;
        
        //calculate the relative position of the object to the center of the bounding box
        var relativePosition = transform.position - center;
        
        //recenter the object
        transform.position = new Vector3(newPosition.x + relativePosition.x, transform.position.y, newPosition.z + relativePosition.z);
    }
}

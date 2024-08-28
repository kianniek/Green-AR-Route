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
        var position = new Vector3(newPosition.x, transform.position.y, newPosition.z);

        StartCoroutine(LerpPosition(position, 0.3f));
    }

    private IEnumerator LerpPosition(Vector3 targetPosition, float duration)
    {
        var startPosition = transform.position;
        var time = 0f;
        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
    }
}
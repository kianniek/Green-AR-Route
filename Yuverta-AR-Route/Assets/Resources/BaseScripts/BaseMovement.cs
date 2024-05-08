using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is to be inherited by all movement scripts for objects in the AR world
public class BaseMovement : MonoBehaviour
{
    //The manager for that scene
    protected virtual BaseManager currentManager { get; set; }

    //This function is to be called when the object is being moved
    protected virtual IEnumerator TrackTouchPosition()
    {
        //While the object is being moved its position (x and z) is being updated
        while (Input.GetMouseButton(0) || Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Ended)
        {
            SwipeDetection.Instance.trackingObject = true;
            Vector3 newPosition = SharedFunctionality.GetTouchWorldPosition();
            newPosition.y = gameObject.transform.position.y;
            gameObject.transform.position = newPosition;
            
            yield return new WaitForFixedUpdate();
        }
        
        SwipeDetection.Instance.trackingObject = false;
    }
}

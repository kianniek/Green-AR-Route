using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

//This class is to be inherited by all movement scripts for objects in the AR world
public class BaseMovement : MonoBehaviour
{
    [Description("The manager that is active in that scene.")]
    protected virtual BaseManager currentManager { get; set; }

    /// <summary>
    /// This function makes the object follow the touch position.
    /// </summary>
    protected virtual IEnumerator TrackTouchPosition()
    {
        //While the object is being moved its position (x and z) is being updated
        while (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            SwipeDetection.Instance.trackingObject = true;
            Vector3 newPosition = SharedFunctionality.Instance.GetTouchWorldPosition();
            newPosition.y = gameObject.transform.position.y;
            gameObject.transform.position = newPosition;
            
            yield return new WaitForFixedUpdate();
        }
        
        SwipeDetection.Instance.trackingObject = false;
    }
}

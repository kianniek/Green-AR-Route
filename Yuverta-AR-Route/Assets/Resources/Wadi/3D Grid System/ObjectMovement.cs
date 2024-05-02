using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ObjectMovement : MonoBehaviour
{
    private ARRaycastManager arRaycastManager;
    
    private void Start()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
    }
    
    public Vector3 GetTouchWorldPosition()
    {
        if (Input.touchCount <= 0) return GetMouseWorldPosition();
    
        var touchPosition = Input.GetTouch(0).position;
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        
        var rayCastHit = new List<ARRaycastHit>();
        return arRaycastManager.Raycast(ray, rayCastHit) ? rayCastHit[0].pose.position : Vector3.zero;
    }
    
    //Debug
    public static Vector3 GetMouseWorldPosition()
    {
        var ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out var rayCastHit) ? rayCastHit.point : Vector3.zero;
    }

    public void MoveObject()
    {
        /*Debug.Log("Before if");
        if (TrackTouchPosition() != null) return;
        Debug.Log("Passed if");*/
        StartCoroutine(TrackTouchPosition());
    }

    private IEnumerator TrackTouchPosition()
    {
        while (Input.GetMouseButton(0) /*|| Input.GetTouch(0).phase != TouchPhase.Ended*/)
        {
            Debug.Log("WHooo");
            SwipeDetection.Instance.trackingObject = true;
            Vector3 newPosition = GetTouchWorldPosition();
            newPosition.y = 0;
            gameObject.transform.position = newPosition;
            
            yield return new WaitForFixedUpdate();
        }

        gameObject.transform.position = GridManager.Instance.SnapToGrid(gameObject);
        
        yield return new WaitForSeconds(0.2f);
        SwipeDetection.Instance.trackingObject = false;
    }
}

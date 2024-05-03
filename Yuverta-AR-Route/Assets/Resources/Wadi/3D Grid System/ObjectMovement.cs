using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ObjectMovement : MonoBehaviour
{
    private ARRaycastManager arRaycastManager;
    public bool animationActive = false;
    public ObjectLogic objectLogic;
    
    private void Start()
    {
        objectLogic = gameObject.GetComponent<ObjectLogic>();
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
        while (Input.GetMouseButton(0) || Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Ended)
        {
            SwipeDetection.Instance.trackingObject = true;
            Vector3 newPosition = GetTouchWorldPosition();
            newPosition.y = gameObject.transform.position.y;
            gameObject.transform.position = newPosition;
            
            yield return new WaitForFixedUpdate();
        }

        var closestGridPosition = GridManager.Instance.SnapToGrid(gameObject);

        animationActive = true;
        while (Vector3.Distance(gameObject.transform.position, closestGridPosition) > 0.001 && animationActive)
        {
            Debug.Log("WHooo");
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position,closestGridPosition , 0.05f);
            yield return new WaitForSeconds(0.01f);

            if (!Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) continue;
            
            GameObject collidedObject = null;
            SwipeDetection.Instance.CollideWithObject(out collidedObject);
            if (!collidedObject || collidedObject != gameObject) continue;
            
            GridManager.Instance.SelectedObject(collidedObject);
            animationActive = false;
            StopCoroutine(TrackTouchPosition());
        }
        
        gameObject.transform.position = closestGridPosition;
        
        yield return new WaitForSeconds(0.2f);
        SwipeDetection.Instance.trackingObject = false;
    }
}

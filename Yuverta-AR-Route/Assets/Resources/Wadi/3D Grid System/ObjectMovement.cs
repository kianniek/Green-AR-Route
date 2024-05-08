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
        StartCoroutine(TrackTouchPosition());
    }

    private IEnumerator TrackTouchPosition()
    {
        CheckLayer();
        
        while (Input.GetMouseButton(0) || Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Ended)
        {
            SwipeDetection.Instance.trackingObject = true;
            Vector3 newPosition = GetTouchWorldPosition();
            newPosition.y = gameObject.transform.position.y;
            gameObject.transform.position = newPosition;
            
            yield return new WaitForFixedUpdate();
        }

        var closestGridPosition = GridManager.Instance.SnapToGrid(gameObject);
        
        SwipeDetection.Instance.trackingObject = false;

        animationActive = true;
        while (Vector3.Distance(gameObject.transform.position, closestGridPosition) > 0.002 && animationActive)
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position,closestGridPosition , 0.05f);
            yield return new WaitForSeconds(0.01f);

            if (!Input.GetMouseButtonDown(0) || Input.touchCount == 0) continue;

            SwipeDetection.Instance.CollideWithObject(out var collidedObject);
            if (!collidedObject || collidedObject != gameObject || collidedObject.GetComponent<ObjectMovement>() != this) continue;
            
            GridManager.Instance.SelectedObject(collidedObject);
            animationActive = false;
            StopCoroutine(TrackTouchPosition());
        }
        
        gameObject.transform.position = closestGridPosition;
    }

    public void CheckLayer()
    {
        if (GridManager.Instance.gridCurrentLayer == objectLogic.layerObj) return;
        
        StartCoroutine(ChangeLayer(GridManager.Instance.gridCurrentLayer));
    }

    private IEnumerator ChangeLayer(int newLayer)
    {
        var currentPos = gameObject.transform.position;
        
        var newLayerIndex = newLayer - objectLogic.layerObj;
        currentPos.y += newLayerIndex * GridManager.Instance.distanceLayers;
        
        while (Mathf.Abs(gameObject.transform.position.y - currentPos.y) > 0.005)
        {
            var whilePos = gameObject.transform.position;
            whilePos.y = Mathf.Lerp(gameObject.transform.position.y, currentPos.y, 0.05f);
            gameObject.transform.position = whilePos;
            yield return new WaitForSeconds(0.01f);
        }
        
        objectLogic.SetObjectLayerID(newLayer);
    }
}
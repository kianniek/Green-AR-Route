using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ObjectMovement : BaseMovement
{
    private ARRaycastManager arRaycastManager;
    public ObjectLogic objectLogic;
    
    //This bool is active if the object is snapping to a certain position
    private bool animationActive;
    
    private void Start()
    {
        currentManager = FindObjectOfType<BaseManager>();
        objectLogic = gameObject.GetComponent<ObjectLogic>();
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
    }

    public void MoveObject()
    {
        StartCoroutine(TrackTouchPosition());
    }
    
    protected override IEnumerator TrackTouchPosition()
    {
        CheckLayer();
        
        yield return StartCoroutine(base.TrackTouchPosition());
        
        var closestGridPosition = GridManager.Instance.SnapToGrid(gameObject);

        animationActive = true;
        while (Vector3.Distance(gameObject.transform.position, closestGridPosition) > 0.002 && animationActive)
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position,closestGridPosition , 0.05f);
            yield return new WaitForSeconds(0.01f);

            if (!Input.GetMouseButtonDown(0) || Input.touchCount == 0) continue;

            SwipeDetection.Instance.CollideWithObject(out var collidedObject);
            if (!collidedObject || collidedObject != gameObject || collidedObject.GetComponent<ObjectMovement>() != this) continue;
            
            currentManager.SelectedObject(collidedObject);
            animationActive = false;
            StopCoroutine(TrackTouchPosition());
        }
        
        gameObject.transform.position = closestGridPosition;
    }

    //GridManager only functions
    private void CheckLayer()
    {
        if (!GridManager.Instance || !objectLogic) return;
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
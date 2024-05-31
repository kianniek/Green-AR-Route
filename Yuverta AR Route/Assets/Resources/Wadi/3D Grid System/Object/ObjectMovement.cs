using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ObjectMovement : BaseMovement
{
    public ObjectLogic objectLogic;

    [Tooltip("This bool is active if the object is snapping to a certain position.")]
    public bool animationActive;
    
    [Tooltip("The maximum amount of times a while should repeat itself before stopping.")]
    [SerializeField] private int maxWhileRepeatTime = 0;

    [Tooltip("The time that the while loop needs to pause before running again.")]
    [SerializeField] private float timeWaitWhile = 0;
    
    [Tooltip("The speed at which the object should lerp to the new position.")]
    [SerializeField] private float lerpSpeed = 0;
    
    [Tooltip("The distance the object should be from the new position until it snaps to the grid-position.")]
    [SerializeField] private float snapDistanceWhile;
    
    [Tooltip("AR Ray-cast Manager")]
    private ARRaycastManager raycastManager;
    private void Start()
    {
        
        //Setting the values to a default if they were not set yet
        if (maxWhileRepeatTime == 0) maxWhileRepeatTime = 150;
        if (timeWaitWhile == 0) timeWaitWhile = 0.01f;
        if (lerpSpeed == 0) lerpSpeed = 0.05f;
        
        //Finding objects
        currentManager = FindObjectOfType<BaseManager>();
        objectLogic = gameObject.GetComponent<ObjectLogic>();
        raycastManager = FindObjectOfType<ARRaycastManager>();
    }

    /// <summary>
    /// This function makes the object follow the touch position.
    /// </summary>
    public void MoveObject()
    {
        StartCoroutine(TrackTouchPosition());
    }

    protected override IEnumerator TrackTouchPosition()
    {
        if (animationActive) yield break;
        CheckLayer();
        bool editor = Application.isEditor;
        //While the object is being moved its position (x and z) is being updated
        while (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            SwipeDetection.Instance.trackingObject = true;

            gameObject.transform.position = SharedFunctionality.Instance.ObjectMovement(raycastManager, gameObject);
            
            yield return new WaitForFixedUpdate();
        }

        SwipeDetection.Instance.trackingObject = false;

        var closestGridPosition = GridManager.Instance.SnapToGrid(gameObject);
        DebugLayer();
        animationActive = true;
        int whileTimeCalled = 0;
        while (Vector3.Distance(gameObject.transform.position, closestGridPosition) > snapDistanceWhile && animationActive)
        {
            whileTimeCalled++;
            if (whileTimeCalled * timeWaitWhile > maxWhileRepeatTime) animationActive = false;
            
            yield return new WaitForSeconds(timeWaitWhile);
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, closestGridPosition, lerpSpeed);

            if (!Input.GetMouseButtonDown(0) && Input.touchCount == 0) continue;

            SwipeDetection.Instance.CollideWithObject(out var collidedObject);
            if (!collidedObject || collidedObject != gameObject ||
                collidedObject.GetComponent<ObjectMovement>() != this) continue;

            animationActive = false;
        }

        if (!animationActive)
        {
            currentManager.SelectedObject(this.gameObject);
            yield break;
        }

        animationActive = false;
        gameObject.transform.position = closestGridPosition;
    }

    //GridManager only functions
    /// <summary>
    /// Checking if the object is on the right layer.
    /// </summary>
    private void CheckLayer()
    {
        if (!GridManager.Instance || !objectLogic) return;
        if (GridManager.Instance.gridCurrentLayer == objectLogic.layerObj) return;

        StartCoroutine(ChangeLayer(GridManager.Instance.gridCurrentLayer));
    }

    /// <summary>
    /// Forcing the object to snap to the right layer.
    /// </summary>
    private void DebugLayer()
    {
        if (!GridManager.Instance || !objectLogic) return;
        var gridPointPos = GridManager.Instance.SnapToGrid(gameObject);
        GridManager.Instance.distanceLayers = GridManager.Instance.gridPoints.Keys.ToList()[^1].transform.position.y - GridManager.Instance.gridPoints.Keys.ToList()[0].transform.position.y;
        var currentPos = gameObject.transform.position;
        if (Vector3.Distance(gridPointPos, currentPos) > snapDistanceWhile) StartCoroutine(ChangeLayer(GridManager.Instance.gridCurrentLayer, true, gridPointPos));
    }

    /// <summary>
    /// Changing the objects layers
    /// </summary>
    private IEnumerator ChangeLayer(int newLayer, bool debug = false, Vector3? debugPos = null)
    {
        var currentPos = gameObject.transform.position;

        float newLayerIndex = debug switch
        {
            true => newLayer,
            _ => newLayer - objectLogic.layerObj
        };

        switch (debugPos)
        {
            case null:
                currentPos.y += GridManager.Instance.distanceLayers * newLayerIndex;
                break;
            default:
                currentPos = (Vector3) debugPos;
                break;
        }
        
        int whileTimeCalled = 0;
        while (Vector3.Distance(gameObject.transform.position, currentPos) > snapDistanceWhile)
        {
            whileTimeCalled++;
            if (whileTimeCalled > maxWhileRepeatTime) break;
            var whilePos = gameObject.transform.position;
            whilePos.y = Mathf.Lerp(gameObject.transform.position.y, currentPos.y, lerpSpeed);
            gameObject.transform.position = whilePos;
            yield return new WaitForSeconds(timeWaitWhile);
        }
        gameObject.transform.position = currentPos;
        objectLogic.SetObjectLayerID(newLayer);
    }

    private void Update()
    {
        KeepObjectOnSnappedPosition();
    }

    public void KeepObjectOnSnappedPosition()
    {
        if (!objectLogic.SnappedObject) return;
        var closestGridPosition = objectLogic.SnappedObject.transform.position;
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, closestGridPosition, lerpSpeed);
    }
}
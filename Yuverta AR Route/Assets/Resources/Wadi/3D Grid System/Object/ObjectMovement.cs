using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ObjectMovement : BaseMovement
{
    public ObjectLogic objectLogic;

    //This bool is active if the object is snapping to a certain position
    public bool animationActive;
    
    [Tooltip("The maximum amount of times a while should repeat itself before stopping.")]
    [SerializeField] private int maxWhileRepeatTime = 0;

    [Tooltip("The time that the while loop needs to pause before running again")]
    [SerializeField] private float timeWaitWhile = 0;
    
    [Tooltip("The speed at which the object should lerp to the new position.")]
    [SerializeField] private float lerpSpeed = 0;
    
    [Tooltip("The distance the object should be from the new position until it snaps to the grid-position.")]
    [SerializeField] private float snapDistanceWhile;
    
    private ARRaycastManager raycastManager;
    private void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        //Temporary fixing of the values
        if (maxWhileRepeatTime == 0) maxWhileRepeatTime = 150;
        if (timeWaitWhile == 0) timeWaitWhile = 0.01f;
        if (lerpSpeed == 0) lerpSpeed = 0.05f;
        
        currentManager = FindObjectOfType<BaseManager>();
        objectLogic = gameObject.GetComponent<ObjectLogic>();
    }

    public void MoveObject()
    {
        StartCoroutine(TrackTouchPosition());
    }

    protected override IEnumerator TrackTouchPosition()
    {
        if (animationActive) yield break;
        CheckLayer();
        
        //While the object is being moved its position (x and z) is being updated
        while (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            SwipeDetection.Instance.trackingObject = true;
            
            /*Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {*/
                // Create a list to hold the hit results
                List<ARRaycastHit> hits = new List<ARRaycastHit>();

                // Raycast from the touch position
                if (raycastManager.Raycast(Input.mousePosition/*touch.position*/, hits, TrackableType.Planes))
                {
                    // Get the hit position
                    Pose hitPose = hits[0].pose;

                    // Calculate the adjusted position at the fixed y-level
                    #if UNITY_EDITOR
                    Vector3 adjustedPosition = GetPointOnYPlane(hitPose.position, gameObject.transform.position.y, true);
                    #else
                    Vector3 adjustedPosition = GetPointOnYPlane(hitPose.position, gameObject.transform.position.y);
                    #endif

                    // Move the object to the adjusted position
                    gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, adjustedPosition, Time.deltaTime * 10);
                }
            //}
            
            yield return new WaitForFixedUpdate();
            
            /*var cameraPos = Camera.main!.transform.position;
            
            float distanceToCamera = Vector3.Distance(cameraPos, gameObject.transform.position);
            
            
            if (distanceToCamera > 1f)
            {
                distanceToCamera = 1f;
            }

            // Get the position of the touch in screen coordinates
            Vector3 touchPosition = Input.touchCount switch
            {
                0 => Input.mousePosition,
                _ => Input.GetTouch(0).position
            };

            // Convert the screen position to world position
            Vector3 newPosition = Camera.main!.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, distanceToCamera));
            newPosition.y = gameObject.transform.position.y;
            gameObject.transform.position = newPosition;*/

            yield return new WaitForFixedUpdate();
            
            /*SwipeDetection.Instance.trackingObject = true;
            Vector3 newPosition = SharedFunctionality.Instance.GetTouchWorldPosition();
            if (newPosition == Vector3.zero)
            {
                yield return new WaitForFixedUpdate();
                continue;
            }

            var yDiff = Mathf.Abs(newPosition.y - gameObject.transform.position.y);
            yDiff *= -Camera.main!.transform.forward.magnitude;

            newPosition.y = gameObject.transform.position.y;
            gameObject.transform.position = newPosition;

            yield return new WaitForFixedUpdate();*/
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
    
    Vector3 GetPointOnYPlane(Vector3 hitPosition, float yLevel, bool debug = false)
    {
        // Create a ray from the camera through the touch position
        Vector3 touchPosition = debug ? Input.mousePosition : Input.GetTouch(0).position;
        Ray ray = Camera.main!.ScreenPointToRay(touchPosition);

        // Calculate the distance to the yLevel plane from the camera
        float distanceToYLevel = (yLevel - ray.origin.y) / ray.direction.y;

        // Get the point of intersection
        Vector3 pointOnYPlane = ray.origin + ray.direction * distanceToYLevel;

        return new Vector3(pointOnYPlane.x, yLevel, pointOnYPlane.z);
    }

    //GridManager only functions
    private void CheckLayer()
    {
        if (!GridManager.Instance || !objectLogic) return;
        if (GridManager.Instance.gridCurrentLayer == objectLogic.layerObj) return;

        StartCoroutine(ChangeLayer(GridManager.Instance.gridCurrentLayer));
    }

    private void DebugLayer()
    {
        if (!GridManager.Instance || !objectLogic) return;
        var gridPointPos = GridManager.Instance.SnapToGrid(gameObject);
        GridManager.Instance.distanceLayers = GridManager.Instance.gridPoints.Keys.ToList()[^1].transform.position.y - GridManager.Instance.gridPoints.Keys.ToList()[0].transform.position.y;
        var currentPos = gameObject.transform.position;
        if (Vector3.Distance(gridPointPos, currentPos) > snapDistanceWhile) StartCoroutine(ChangeLayer(GridManager.Instance.gridCurrentLayer, true, gridPointPos));
    }

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
}
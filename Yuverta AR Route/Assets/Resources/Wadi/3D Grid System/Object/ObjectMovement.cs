using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ObjectMovement : BaseMovement
{
    public ObjectLogic objectLogic;

    //This bool is active if the object is snapping to a certain position
    public bool animationActive;
    
    [Tooltip("The maximum amount of times a while should repeat itself before stopping.")]
    [SerializeField] private int maxWhileRepeatTime;

    [Tooltip("The time that the while loop needs to pause before running again")]
    [SerializeField] private float timeWaitWhile;
    
    [Tooltip("The speed at which the object should lerp to the new position.")]
    [SerializeField] private float lerpSpeed;
    
    [Tooltip("The distance the object should be from the new position until it snaps to the grid-position.")]
    [SerializeField] private float snapDistanceWhile;
    private void Start()
    {
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
            
            var cameraPos = Camera.main!.transform.position;
            
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
            gameObject.transform.position = newPosition;

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
            Debug.Log(whileTimeCalled);
        }
        gameObject.transform.position = currentPos;
        objectLogic.SetObjectLayerID(newLayer);
    }
}
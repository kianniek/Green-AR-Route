using UnityEngine;
using UnityEngine.Events;

public class ObjectLogic : MonoBehaviour
{
    public GridManager.ObjectGridLocation objectGridLocation;

    private GameObject snappedGridPoint;

    private DOTweenAnimations _dotweenAnimations;

    public DOTweenAnimations DotweenAnimations => _dotweenAnimations;
    public GameObject SnappedGridPoint
    {
        get => snappedGridPoint;
        private set
        {
            snappedGridPoint = value;
            currentGridPoint = SnappedGridPoint.GetComponent<GridPointScript>();
        }
    }

    private GridPointScript currentGridPoint;

    private GridManager gridManager;

    [SerializeField] private UnityEvent onShake = new();

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        _dotweenAnimations = FindObjectOfType<DOTweenAnimations>();
        gameObject.transform.rotation = Quaternion.Euler(0, -90, 0);

        gameObject.name = objectGridLocation.ToString();

        if (gridManager != null)
            SnappedGridPoint = gridManager.SnapToGridPoint(gameObject);
    }

    public bool IsCorrectlyPlaced()
    {
        var isCorrectlyPlaced = objectGridLocation == currentGridPoint.objectGridLocation;
        Debug.Log("IsCorrectlyPlaced: " + isCorrectlyPlaced);
        if (!isCorrectlyPlaced)
        {
            Debug.Log("Incorrectly placed");
        }

        return isCorrectlyPlaced;
    }

    public void SnapToNewGridPoint()
    {
        Debug.Log("Snapping to new grid point");
        SnappedGridPoint = gridManager.MoveObjectToNewGridPoint(gameObject, SnappedGridPoint);
        
        //if the object is shaking, stop the shaking
        StopShaking();
    }

    public void RemoveObjectFromGrid()
    {
        Debug.Log("Removing object from grid");
        gridManager.RemoveObjectFromGrid(gameObject, SnappedGridPoint);
        
        //if the object is shaking, stop the shaking
        StopShaking();
    }

    public void ShakeObject()
    {
        onShake.Invoke();
        _dotweenAnimations.InfiniteShake(transform);
    }
    
    public void StopShaking()
    {
        _dotweenAnimations.StopInfiniteShake(transform);
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ObjectLogic : MonoBehaviour
{
    public GridManager.ObjectGridLocation objectGridLocation;

    private GameObject snappedGridPoint;

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

    private static readonly Vector3 newScale = new Vector3(0.2f, 0.2f, 0.2f);
    

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        gameObject.transform.localScale = newScale;
        gameObject.transform.rotation = Quaternion.Euler(0, -90, 0);
        
        gameObject.name = objectGridLocation.ToString();

        if (gridManager != null)
            SnappedGridPoint = gridManager.SnapToGridPoint(gameObject);
    }

    public bool IsCorrectlyPlaced()
    {
        var isCorrectlyPlaced = objectGridLocation == currentGridPoint.objectGridLocation;
        Debug.Log("IsCorrectlyPlaced: " + isCorrectlyPlaced);
        if(!isCorrectlyPlaced)
        {
            Debug.Log("Incorrectly placed");
        }
        
        return isCorrectlyPlaced;
    }

    public void SnapToNewGridPoint()
    {
        Debug.Log("Snapping to new grid point");
        SnappedGridPoint = gridManager.MoveObjectToNewGridPoint(gameObject, SnappedGridPoint);
    }

    public void RemoveObjectFromGrid()
    {
        Debug.Log("Removing object from grid");
        gridManager.RemoveObjectFromGrid(gameObject, SnappedGridPoint);
    }
}
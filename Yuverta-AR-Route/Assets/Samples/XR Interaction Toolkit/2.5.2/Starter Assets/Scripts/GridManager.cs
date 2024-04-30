using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private float gridSize = 1.0f;
    public float GridSize
    {
        get => gridSize;
        set => gridSize = value;
    }

    public List<GameObject> gridPoints = new();
    private Dictionary<bool, GameObject> occupiedPositions = new Dictionary<bool, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    public void SnapToGrid(GameObject objToSnap)
    {
        float minDistance = Mathf.Infinity;
        GameObject closestGridPoint = null;

        foreach (var gridPoint in gridPoints)
        {
            float distance = Vector3.Distance(objToSnap.transform.position, gridPoint.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestGridPoint = gridPoint;
            }
        }

        objToSnap.transform.position = closestGridPoint.transform.position;
        occupiedPositions.Add(true, closestGridPoint);
    }

}

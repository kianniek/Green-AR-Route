using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private float gridSize = 1.0f;
    
    [SerializeField] private TextMeshProUGUI gridText;
    public float GridSize
    {
        get => gridSize;
        set => gridSize = value;
    }

    public List<GameObject> gridPoints = new();
    public Dictionary<GameObject, int> gridSortedLayer = new Dictionary<GameObject, int>();
    private Dictionary<bool, GameObject> occupiedPositions = new Dictionary<bool, GameObject>();
    private int gridCurrentLayer = 0;
    public Vector2Int gridDimensions = new Vector2Int(0, 0);
    
    public List<GameObject> placedObjects = new List<GameObject>();

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

    public void LayerSwap()
    {
        if (Input.touchCount == 0 || Input.GetTouch(0).phase != TouchPhase.Ended) return;

        var touchDirection = Input.GetTouch(0).deltaPosition.normalized;
        float direction = Mathf.Abs(touchDirection.x) > Mathf.Abs(touchDirection.y) ? touchDirection.x : touchDirection.y;
        gridCurrentLayer += Mathf.RoundToInt(direction);
        Debug.Log(gridCurrentLayer);
        gridCurrentLayer = Mathf.Clamp(gridCurrentLayer, 0, gridDimensions.y);
        gridText.text = gridCurrentLayer.ToString();
    }

}

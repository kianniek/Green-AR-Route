using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class GridManager : BaseManager
{
    public static GridManager Instance { get; private set; }
    
    public GridBuilder gridBuilder;
    public GridLayering gridLayering;
    public ObjectMovement objectMovement;
    public UIMenuLogic uiMenu;
    public ObjectSpawner objectSpawner;
    public int gridCurrentLayer;

    public SerializableDictionary<GameObject, int> objsToSpawnAmount = new();

    [SerializeField] private float gridSize;
    
    public float GridSize
    {
        get => gridSize;
        set => gridSize = value;
    }

    //public List<GameObject> gridPoints = new();
    public Dictionary<GameObject, int> gridPoints = new();
    public Dictionary<GameObject, bool> occupiedPositions = new Dictionary<GameObject, bool>();
    
    public List<GameObject> placedObjects = new List<GameObject>();
    public int selectedObjectIndex = 0;

    public float distanceLayers;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        gridBuilder = gameObject.GetComponent<GridBuilder>();
        gridLayering = gameObject.GetComponent<GridLayering>();
        uiMenu = FindObjectOfType<UIMenuLogic>();
        objectSpawner = FindObjectOfType<ObjectSpawner>();
        Destroy(FindObjectOfType<ARInteractorSpawnTrigger>());
        
        uiMenu.StartUp(objsToSpawnAmount.keys.ToList());
        
        objectSpawner.ObjectSpawned.AddListener(NewObjectPlaced);
        objectSpawner.m_ObjectPrefabs = objsToSpawnAmount.keys.ToList();
    }

    public Vector3 SnapToGrid(GameObject objToSnap)
    {
        ObjectLogic objectLogic = objToSnap.GetComponent<ObjectLogic>();
        Vector3 position = objToSnap.transform.position;

        if (objectLogic.isPlaced)
        {
            var previousSnappedGridPoint = ClosestGridPoint(objectLogic.previousSnappedPosition, findLastPoint: true);
            occupiedPositions.Remove(previousSnappedGridPoint);
            objectLogic.isPlaced = false;
            objectLogic.previousSnappedPosition = Vector3.negativeInfinity;
        }
        
        var closestGridPoint = ClosestGridPoint(position, occupiedPositions.Keys.ToList());
        if (closestGridPoint == null)
        {
            closestGridPoint = ClosestGridPoint(position, occupiedPositions.Keys.ToList(), ignoreLayer: true);
        }
        occupiedPositions.Add(closestGridPoint, true);
        objectLogic.isPlaced = true;
        objectLogic.previousSnappedPosition = closestGridPoint.transform.position;
        return closestGridPoint.transform.position;
    }

    private GameObject ClosestGridPoint(Vector3 position, List<GameObject> occupiedPositions = null, bool findLastPoint = false, bool ignoreLayer = false)
    {
        float minDistance = Mathf.Infinity;
        GameObject closestGridPoint = null;

        foreach (var gridPoint in gridPoints.Keys)
        {
            if (!ignoreLayer && !findLastPoint && gridPoints[gridPoint] != gridCurrentLayer) continue;
            if (occupiedPositions != null && occupiedPositions.Contains(gridPoint)) continue;
            
            float distance = Vector3.Distance(position, gridPoint.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestGridPoint = gridPoint;
            }
        }
        
        return closestGridPoint;
    }

    public override void NewObjectPlaced()
    {
        //Getting the last object spawned by the object spawner
        GameObject newObject = objectSpawner.lastSpawnedObject;
        
        //Adding the object to the list of placed objects
        placedObjects.Add(newObject); 
        objectMovement = newObject.GetComponent<ObjectMovement>();
        
        //Updating the UI
        uiMenu.Remove(newObject);
        
        //Setting the values
        var objectLogic = newObject.GetComponent<ObjectLogic>();
        objectLogic.objectIndex = placedObjects.Count - 1;
        objectLogic.objectPrefabIndex = objectSpawner.m_SpawnOptionIndex; //TODO: Implement objectPrefabIndex
        objectLogic.SetObjectLayerID(gridCurrentLayer);
        
        //Snapping the object to the grid
        objectMovement.MoveObject();
    }

    public override void SelectedObject(GameObject selectedObject)
    {
        objectMovement = selectedObject.GetComponent<ObjectMovement>();
        selectedObjectIndex = objectMovement.objectLogic.objectIndex;
        objectMovement.MoveObject();
    }

    public override void DestroyObject()
    {
        //Checking if the list is empty
        if (placedObjects.Count <= 0) return;
        
        //Getting the selected object
        var focusedObject = placedObjects[selectedObjectIndex];
        
        //Removing the object from the lists
        placedObjects.Remove(focusedObject);
        occupiedPositions.Remove(ClosestGridPoint(focusedObject.transform.position, findLastPoint: true));
        
        //Destroying the object
        Destroy(focusedObject.gameObject);
        
        //Updating the object indexes
        foreach (var obj in placedObjects)
        {
            obj.GetComponent<ObjectLogic>().objectIndex = placedObjects.IndexOf(obj);
        }
        
        //Setting the objectMovement to the first object in the list
        if (placedObjects.Count > 0)
        {
            objectMovement = placedObjects[0].GetComponent<ObjectMovement>();
        }
        //Or updating the UI
        //else uiMenu.DeleteButtonVisibility();

        //Resetting the swipe detection
        SwipeDetection.Instance.trackingObject = false;
    }
}

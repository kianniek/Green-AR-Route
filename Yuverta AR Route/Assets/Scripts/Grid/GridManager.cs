using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;

[RequireComponent(typeof(GridBuilder))]
public class GridManager : MonoBehaviour
{
    [SerializeField] private bool _wadiCompleted;

    [SerializeField] private GridBuilder gridBuilder;
    [SerializeField] private UIMenuLogic uiMenuLogic;

    [SerializeField] private SerializableDictionary<GameObject, ObjectGridLocation> objsToSpawn = new();

    [SerializeField] private List<GameObject> placedObjects = new();
    [SerializeField] private int selectedObjectIndex;

    private Dictionary<GameObject, bool> occupiedPositions = new(); // Changed value type to bool

    public enum ObjectGridLocation
    {
        BottomLeft,
        BottomMiddle,
        BottomRight,
        MiddleLeft,
        Middle,
        MiddleRight,
        UpperLeft,
        UpperMiddle,
        UpperRight,
        BottomLeft2,
        BottomMiddle2,
        BottomRight2,
        MiddleLeft2,
        Middle2,
        MiddleRight2,
        UpperLeft2,
        UpperMiddle2,
        UpperRight2
    }

    public bool WadiCompleted => _wadiCompleted;

    public SerializableDictionary<GameObject, ObjectGridLocation> ObjsToSpawn => objsToSpawn;

    public GridBuilder GridBuilder => gridBuilder;

    private void Awake()
    {
        gridBuilder = gameObject.GetComponent<GridBuilder>(); // Ensure GridBuilder is initialized
    }

    private void Start()
    {
        // Find and enable UI Menu Logic
        uiMenuLogic = FindObjectOfType<UIMenuLogic>();
        if (uiMenuLogic != null)
        {
            uiMenuLogic.EnableCanvas(true);
        }
        else
        {
            Debug.LogError("UIMenuLogic not found");
        }

        // Build the grid and initialize occupied positions
        var gameObjectList = gridBuilder.BuildGrid();
        foreach (var obj in gameObjectList)
        {
            occupiedPositions.Add(obj, false);
        }

        // Ensure ARInteractorSpawnTrigger is destroyed if found
        var arInteractor = FindObjectOfType<ARInteractorSpawnTrigger>();
        if (arInteractor != null)
        {
            Destroy(arInteractor.gameObject);
        }

        // Deserialize the dictionary to ensure correct handling of data
        objsToSpawn.OnAfterDeserialize();
    }

    public GameObject SnapToGridPoint(GameObject objToSnap)
    {
        var closestGridPoint = ClosestGridPoint(objToSnap);
        if (closestGridPoint != null)
        {
            Debug.Log($"Closest grid point is {closestGridPoint.name}");
        }
        else
        {
            Debug.LogWarning("No available grid point found");
        }

        if (!placedObjects.Contains(objToSnap))
        {
            placedObjects.Add(objToSnap);
        }

        return closestGridPoint;
    }

    private GameObject ClosestGridPoint(GameObject objToSnap)
    {
        var availablePositions = new Dictionary<GameObject, bool>();

        foreach (var VARIABLE in occupiedPositions)
        {
            if (VARIABLE.Value == false)
            {
                availablePositions.Add(VARIABLE.Key, VARIABLE.Value);
            }
        }

        if (availablePositions.Count == 0)
        {
            Debug.LogWarning("No available positions found");
            return null;
        }

        var closestGridPoint = availablePositions
            .OrderBy(x => Vector3.Distance(x.Key.transform.position, objToSnap.transform.position))
            .FirstOrDefault().Key;

        // Mark the closest grid point as occupied
        occupiedPositions[closestGridPoint] = true;

        return closestGridPoint;
    }

    public GameObject MoveObjectToNewGridPoint(GameObject objToMove, GameObject objGridPoint)
    {
        // Find the current grid point occupied by the object
        var currentGridPoint = occupiedPositions.FirstOrDefault(x => x.Key == objGridPoint).Key;

        Debug.Log($"Current grid point is {currentGridPoint.name}");

        // If the object is already occupying a grid point, mark that position as available
        if (currentGridPoint != null)
        {
            occupiedPositions[currentGridPoint] = false;
        }

        // Find the closest new grid point
        var newGridPoint = ClosestGridPoint(objToMove);

        // If a new grid point is found, snap the object to that point and update the dictionary
        if (newGridPoint != null)
        {
            occupiedPositions[newGridPoint] = true;

            return newGridPoint;
        }
        else
        {
            Debug.LogWarning("No available grid point found for moving the object");

            return null;
        }
    }

    public bool CheckPosition(out List<GameObject> wrongPlaces)
    {
        wrongPlaces = new List<GameObject>();
        foreach (var obj in placedObjects)
        {
            var script = obj.GetComponent<ObjectLogic>();
            if (script != null && script.IsCorrectlyPlaced())
            {
                continue;
            }

            wrongPlaces.Add(obj);
            Debug.Log($"Object {obj.name} is not correctly placed.");
        }

        return wrongPlaces.Count == 0;
    }

    public void RemoveObjectFromGrid(GameObject obj, GameObject gridPoint)
    {
        if (!occupiedPositions[gridPoint])
            return;

        occupiedPositions[gridPoint] = false;

        //reenable the object in the UI
        if (uiMenuLogic.OnObjectDelete(obj))
        {
            Destroy(obj);
            Debug.Log($"Object {obj.name} removed from grid point {gridPoint.name}");
        }
    }

    private bool CheckIfAllPlaced()
    {
        return placedObjects.Count == objsToSpawn.keys.Count;
    }
}
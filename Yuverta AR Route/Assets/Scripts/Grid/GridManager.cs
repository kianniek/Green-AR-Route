using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using Object = UnityEngine.Object;

[RequireComponent(typeof(GridBuilder))]
public class GridManager : MonoBehaviour
{
    [SerializeField] private bool _wadiCompleted;

    [SerializeField] private GridBuilder gridBuilder;
    [SerializeField] private UIMenuLogic uiMenuLogic;

    [SerializeField] private SerializableDictionary<GameObject, ObjectGridLocation> objsToSpawn = new();

    [SerializeField] private List<GameObject> placedObjects = new();
    [SerializeField] private int selectedObjectIndex;

    private Dictionary<GameObject, int> gridPoints = new();
    private Dictionary<GameObject, bool> occupiedPositions = new();

    public enum ObjectGridLocation
    {
        BottomLeft, //0 0 0
        BottomMiddle, //0 0 1
        BottomRight, //0 0 2
        MiddleLeft, //1 0 0
        Middle, //1 0 1
        MiddleRight, //1 0 2
        UpperLeft, //2 0 0
        UpperMiddle, //2 0 1
        UpperRight, //2 0 2
        BottomLeft2, //0 1 0
        BottomMiddle2, //0 1 1
        BottomRight2, //0 1 2
        MiddleLeft2, //1 1 0
        Middle2, //1 1 1
        MiddleRight2, //1 1 2
        UpperLeft2, //2 1 0
        UpperMiddle2, //2 1 1
        UpperRight2 //2 1 2
    }

    public bool WadiCompleted
    {
        get => _wadiCompleted;
        //set => DisplayWeather(value);
    }
    
    public SerializableDictionary<GameObject, ObjectGridLocation> ObjsToSpawn => objsToSpawn;

    private void Start()
    {
        //find the UI Menu Logic
        uiMenuLogic = FindObjectOfType<UIMenuLogic>();
        //enable the canvas
        uiMenuLogic.EnableCanvas(true);
        
        gridBuilder = gameObject.GetComponent<GridBuilder>();
        Destroy(FindObjectOfType<ARInteractorSpawnTrigger>());

        objsToSpawn.OnAfterDeserialize();

        gridBuilder.BuildGrid();
    }

    public Vector3 SnapToGridPoint(GameObject objToSnap)
    {
        //Getting the closest grid point
        var closestGridPoint = ClosestGridPoint(objToSnap.transform.position);
        
        //Setting the object position to the grid point
        return closestGridPoint.transform.position;
    }

    private GameObject ClosestGridPoint(Vector3 startPosition)
    {
        //check for every occupied position and filter out the available ones
        var availablePositions = occupiedPositions.Where(x => !x.Value).ToList();
        
        //Getting the closest grid point
        var closestGridPoint = availablePositions.OrderBy(x => Vector3.Distance(x.Key.transform.position, startPosition)).FirstOrDefault().Key;
        
        return closestGridPoint;
    }

    public bool CheckPosition(out List<GameObject> wrongPlaces)
    {
        wrongPlaces = new List<GameObject>();
        foreach (var obj in placedObjects)
        {
            var script = obj.GetComponent<ObjectLogic>();
            if (script.IsCorrectlyPlaced())
            {
                continue;
            }

            wrongPlaces.Add(obj);
            Debug.Log($"Object {obj.name} is not correctly placed.");
        }

        return wrongPlaces.Count switch
        {
            0 => true,
            _ => false
        };
    }

    public bool CheckIfAllPlaced()
    {
        return placedObjects.Count == objsToSpawn.keys.Count;
    }
}
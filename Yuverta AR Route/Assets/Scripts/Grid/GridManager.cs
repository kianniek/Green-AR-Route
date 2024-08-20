using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;

[RequireComponent(typeof(GridBuilder))]
public class GridManager : MonoBehaviour
{
    [SerializeField] private GridBuilder gridBuilder;
    [SerializeField] private SerializableDictionary<GameObject, ObjectGridLocation> objsToSpawn = new();
    [SerializeField] private GameObject wadiTopLayerPrefab;
    [SerializeField] private GameObject wadiBottomLayerPrefab;
    [SerializeField] private GameObject wadiCompleteParticlesPrefab;
    [SerializeField] private GameObject wadiWeatherUIPrefab;

    [Header("Events")] [Space(10)] [SerializeField]
    private UnityEvent onWadiCompleted = new();
    [SerializeField]
    private UnityEvent onBlockPlaced = new();

    private GameObject _wadiTopLayer;
    private GameObject _wadiBottomLayer;
    private GameObject _wadiWeatherUI;

    private List<GameObject> _placedObjects = new();
    private int _selectedObjectIndex;
    private Dictionary<GameObject, bool> _occupiedPositions = new(); // Changed value type to bool
    private UIMenuLogic _uiMenuLogic;
    private bool _wadiCompleted;

    public enum ObjectGridLocation
    {
        BottomLeft,
        MiddleLeft,
        UpperLeft,
        BottomMiddle,
        Middle,
        UpperMiddle,
        BottomRight,
        MiddleRight,
        UpperRight,
        BottomLeft2,
        MiddleLeft2,
        UpperLeft2,
        BottomMiddle2,
        Middle2,
        UpperMiddle2,
        BottomRight2,
        MiddleRight2,
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
        _uiMenuLogic = FindObjectOfType<UIMenuLogic>();
        if (_uiMenuLogic != null)
        {
            _uiMenuLogic.EnableCanvas(true);
        }
        else
        {
            Debug.LogError("UIMenuLogic not found");
        }

        // Build the grid and initialize occupied positions
        var gameObjectList = gridBuilder.BuildGrid();
        foreach (var obj in gameObjectList)
        {
            _occupiedPositions.Add(obj, false);
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
        onBlockPlaced.Invoke();
        
        var closestGridPoint = ClosestGridPoint(objToSnap);
        if (closestGridPoint != null)
        {
            //Debug.Log($"Closest grid point is {closestGridPoint.name}");
        }
        else
        {
            Debug.LogWarning("No available grid point found");
        }

        if (!_placedObjects.Contains(objToSnap))
        {
            _placedObjects.Add(objToSnap);
        }
        
        HideGridPointVisualIfOccupied();

        return closestGridPoint;
    }

    private GameObject ClosestGridPoint(GameObject objToSnap)
    {
        var availablePositions = new Dictionary<GameObject, bool>();

        foreach (var occupiedPosition in _occupiedPositions)
        {
            if (occupiedPosition.Value == false)
            {
                availablePositions.Add(occupiedPosition.Key, occupiedPosition.Value);
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
        _occupiedPositions[closestGridPoint] = true;
        
        HideGridPointVisualIfOccupied();


        return closestGridPoint;
    }

    public GameObject MoveObjectToNewGridPoint(GameObject objToMove, GameObject objGridPoint)
    {
        // Find the current grid point occupied by the object
        var currentGridPoint = _occupiedPositions.FirstOrDefault(x => x.Key == objGridPoint).Key;

        //Debug.Log($"Current grid point is {currentGridPoint.name}");

        // If the object is already occupying a grid point, mark that position as available
        if (currentGridPoint != null)
        {
            _occupiedPositions[currentGridPoint] = false;
        }

        // Find the closest new grid point
        var newGridPoint = ClosestGridPoint(objToMove);

        // If a new grid point is found, snap the object to that point and update the dictionary
        if (newGridPoint != null)
        {
            _occupiedPositions[newGridPoint] = true;

            return newGridPoint;
        }
        else
        {
            Debug.LogWarning("No available grid point found for moving the object");

            return null;
        }
        
        HideGridPointVisualIfOccupied();

    }

    public bool CheckPosition(out List<GameObject> wrongPlaces)
    {
        wrongPlaces = new List<GameObject>();
        foreach (var obj in _placedObjects)
        {
            var script = obj.GetComponent<ObjectLogic>();

            if (script != null && script.IsCorrectlyPlaced())
                continue;

            // Add the object to the list of wrongly placed objects
            wrongPlaces.Add(obj);
            Debug.Log($"Object {obj.name} is not correctly placed.");

            //make the wrong objects shake
            script.ShakeObject();
        }
        HideGridPointVisualIfOccupied();

        
        return wrongPlaces.Count == 0 && CheckIfAllPlaced();
    }

    public void RemoveObjectFromGrid(GameObject obj, GameObject gridPoint)
    {
        if (!_occupiedPositions[gridPoint])
            return;

        _occupiedPositions[gridPoint] = false;

        //reenable the object in the UI
        if (_uiMenuLogic.OnObjectDelete(obj))
        {
            //remove the object from the list of placed objects
            _placedObjects.Remove(obj);
            Destroy(obj);
            Debug.Log($"Object {obj.name} removed from grid point {gridPoint.name}");
        }
        
        HideGridPointVisualIfOccupied();

    }

    private bool CheckIfAllPlaced()
    {
        return _placedObjects.Count == objsToSpawn.keys.Count;
    }

    public void OnWadiCompleted()
    {
        HideGridPointVisualIfOccupied();
        //Remove all objects from the grid and don't place them back into UI
        foreach (var obj in _placedObjects)
        {
            Destroy(obj);
        }

        _placedObjects.Clear();

        //Remove wadi gridpoints through GridBuilder
        gridBuilder.ClearGrid();

        //Instantiate the top and bottom layers of the wadi
        _wadiTopLayer = Instantiate(wadiTopLayerPrefab);
        _wadiBottomLayer = Instantiate(wadiBottomLayerPrefab);
        _wadiWeatherUI = Instantiate(wadiWeatherUIPrefab);

        //Get the position constraints in the wadiWeatherUI
        var weatherUIPositionConstraints = _wadiWeatherUI.GetComponentsInChildren<PositionConstraint>();

        foreach (var positionConstraint in weatherUIPositionConstraints)
        {
            positionConstraint.constraintActive = true;

            var constraintSources = new ConstraintSource();
            constraintSources.sourceTransform = _wadiTopLayer.transform;
            constraintSources.weight = 1;

            positionConstraint.AddSource(constraintSources);
        }

        var centerPoint = gridBuilder.GetCenterPoint();

        _wadiTopLayer.transform.position = _wadiBottomLayer.transform.position = centerPoint + transform.position;

        var rotation = transform.rotation * Quaternion.Euler(0, -90, 0);
        _wadiTopLayer.transform.rotation = _wadiBottomLayer.transform.rotation = rotation;

        //Set the wadi completed flag to true
        _wadiCompleted = true;

        //Play the wadi completion particles
        if (wadiCompleteParticlesPrefab)
        {
            var particles = Instantiate(wadiCompleteParticlesPrefab, transform.position, Quaternion.identity);
            var particleSystem = particles.GetComponent<ParticleSystem>();
                
            if (particleSystem != null)
                particleSystem.Play();
        }

        onWadiCompleted.Invoke();
    }
    
    public void HideGridPointVisualIfOccupied()
    {
        foreach (var occupiedPosition in _occupiedPositions)
        {
            occupiedPosition.Key.transform.GetChild(0).gameObject.SetActive(!occupiedPosition.Value);
        }
    }
}
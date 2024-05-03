using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    
    public GridBuilder gridBuilder;
    public GridLayering gridLayering;
    public ObjectMovement objectMovement;
    public UIMenuLogic uiMenu;
    public ObjectSpawner objectSpawner;
    public int gridCurrentLayer = 0;

    [SerializeField] private float gridSize = 1.0f;
    
    public float GridSize
    {
        get => gridSize;
        set => gridSize = value;
    }

    public List<GameObject> gridPoints = new();
    public Dictionary<GameObject, bool> occupiedPositions = new Dictionary<GameObject, bool>();
    
    public List<GameObject> placedObjects = new List<GameObject>();
    public int selectedObjectIndex = 0;

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
        
        uiMenu.StartUp(objectSpawner.objectPrefabs);
        
        objectSpawner.ObjectSpawned.AddListener(NewObjectPlaced);
    }

    public Vector3 SnapToGrid(GameObject objToSnap)
    {
        ObjectLogic objectLogic = objToSnap.GetComponent<ObjectLogic>();
        Vector3 position = objToSnap.transform.position;

        if (objectLogic.isPlaced && objectLogic.previousSnappedPosition != Vector3.negativeInfinity)
        {
            var previousSnappedGridPoint = ClosestGridPoint(objectLogic.previousSnappedPosition);
            occupiedPositions.Remove(previousSnappedGridPoint);
            objectLogic.isPlaced = false;
            objectLogic.previousSnappedPosition = Vector3.negativeInfinity;
        }
        
        var closestGridPoint = ClosestGridPoint(position);
        occupiedPositions.Add(closestGridPoint, true);
        objectLogic.isPlaced = true;
        objectLogic.previousSnappedPosition = closestGridPoint.transform.position;
        return closestGridPoint.transform.position;
    }

    private GameObject ClosestGridPoint(Vector3 position)
    {
        float minDistance = Mathf.Infinity;
        GameObject closestGridPoint = null;

        foreach (var gridPoint in gridPoints)
        {
            float distance = Vector3.Distance(position, gridPoint.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestGridPoint = gridPoint;
            }
        }
        
        return closestGridPoint;
    }

    private void NewObjectPlaced()
    {
        GameObject newObject = objectSpawner.lastSpawnedObject;
        uiMenu.Remove(newObject);
        placedObjects.Add(newObject); 
        var objectLogic = newObject.GetComponent<ObjectLogic>();
        objectLogic.objectIndex = selectedObjectIndex;
        objectLogic.objectPrefabIndex = objectSpawner.m_SpawnOptionIndex; //TODO: Implement objectPrefabIndex
        objectLogic.objectLayer = gridCurrentLayer;
    }

    public void SelectedObject(GameObject selectedObject)
    {
        if (objectMovement) objectMovement.animationActive = false;
        objectMovement = selectedObject.GetComponent<ObjectMovement>();
        selectedObjectIndex = objectMovement.objectLogic.objectIndex;
        objectMovement.MoveObject();
    }

    public void DestroyObject()
    {
        if (placedObjects.Count <= 0) return;
        var focusedObject = placedObjects[selectedObjectIndex];
        placedObjects.Remove(focusedObject);
        occupiedPositions.Remove(focusedObject);
        Destroy(focusedObject.gameObject);
    }
}

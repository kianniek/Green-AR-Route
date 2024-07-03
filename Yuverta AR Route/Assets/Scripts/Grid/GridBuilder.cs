using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(GridManager))]
public class GridBuilder : MonoBehaviour
{
    [Header("Grid Spawn Settings")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(10, 10);
    [SerializeField] [Range(1, 2)] private int gridHeight = 1;
    [SerializeField] private Vector3 gridCellRotationDeg;
    [SerializeField] private Vector3 gridCellPadding = Vector3.one;
    [SerializeField] private Vector3 gridCellPaddingConverged = Vector3.one;
    [SerializeField] private float blockSizeMultiplier = 1;
    [SerializeField] private GameObject gridPointPrefab;
    [SerializeField] private GameObject[] gridPrefabs;
    [SerializeField] private GameObject slopKop;
    [SerializeField] private Vector3 slopKopOffset;

    private List<GameObject> gridPoints = new List<GameObject>();

    private Dictionary<GameObject, Vector3> currentGridPositions = new();
    private Dictionary<GameObject, Vector3> convertedGridPositions = new();

    public List<GameObject> BuildGrid()
    {
        if (gridPointPrefab == null)
        {
            Debug.LogError("Grid Point Prefab is not assigned.");
            return null;
        }

        currentGridPositions.Clear();
        convertedGridPositions.Clear();
        gridPoints.Clear();
        ClearGrid();
        
        var maxSize = Vector3.zero;
        if (gridPrefabs.Length != 0)
        {
            foreach (var obj in gridPrefabs)
            {
                var size = obj.GetComponentInChildren<Renderer>().bounds.size;
                maxSize = Vector3.Max(maxSize, size);
            }
        }
        if (maxSize == Vector3.zero)
            maxSize = gridPointPrefab.GetComponentInChildren<Renderer>().bounds.size * blockSizeMultiplier;
        
        maxSize *= blockSizeMultiplier;
        gridPoints = new List<GameObject>();

        gridHeight = Mathf.Max(gridHeight, 1);

        var indexOfGridPoint = 0;

        // Create a grid of points
        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridSize.x; x++)
            {
                for (var z = 0; z < gridSize.y; z++)
                {
                    Vector3 position = CalculateGridPosition(x, y, z);
                    Vector3 positionConverged = CalculateGridpointConvergePositions(x, y, z);

                    var rotation = Quaternion.Euler(gridCellRotationDeg);
                    
                    // Instantiate the grid point
                    var gridPoint = Instantiate(gridPointPrefab, position, rotation, transform);
                    gridPoint.transform.localScale = maxSize;

                    var gridPointScript = gridPoint.GetComponent<GridPointScript>();
                    if (gridPointScript != null)
                    {
                        // Set the object position of the grid point based on the indexOfGridPoint
                        gridPointScript.objectGridLocation = (GridManager.ObjectGridLocation)indexOfGridPoint;
                    }
                    
                    
                    if(gridPointScript.objectGridLocation == GridManager.ObjectGridLocation.UpperMiddle)
                    {
                        // Instantiate the slopKop
                        if (slopKop != null)
                        {
                            var slopKopPosition = gridPoint.transform.position + slopKopOffset;
                            var slopKopRotation = Quaternion.Euler(gridCellRotationDeg);
                            var slopKopObject = Instantiate(slopKop, slopKopPosition, slopKopRotation, transform);
            
                            gridPoints.Add(slopKopObject);
                            convertedGridPositions.Add(slopKopObject, positionConverged + slopKopOffset);

                        }
                    }

                    // Inputting rotation here later
                    gridPoint.name = $"{gridPointScript.objectGridLocation} {x} {y} {z}";

                    // Add the grid point to the list of grid points
                    gridPoints.Add(gridPoint);

                    convertedGridPositions.Add(gridPoint, positionConverged);

                    // Keep track of the index of the grid point
                    ++indexOfGridPoint;
                }
            }
        }
        
        


        // Add all grid points to the list of current grid positions
        foreach (var gridPoint in gridPoints)
        {
            currentGridPositions.Add(gridPoint, gridPoint.transform.position);
        }

        return gridPoints;
    }

    public void ClearGrid()
    {
        // Destroy all children of the grid in forloop
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private Vector3 CalculateGridpointConvergePositions(float x, float y, float z)
    {
        var position = new Vector3(x, y, z);
        position.x += gridCellPaddingConverged.x;
        position.y += gridCellPaddingConverged.y;
        position.z += gridCellPaddingConverged.z;

        // Center the grid
        var centerOffset = new Vector3(
            (gridSize.x - 1) * 0.5f + (x * (gridCellPaddingConverged.x)),
            (gridHeight - 1) * 0.5f + (y * (gridCellPaddingConverged.y) * gridHeight),
            (gridSize.y - 1) * 0.5f + (z * (gridCellPaddingConverged.z))
        );
        position -= centerOffset;

        // Adjust for the GridBuilder's position
        position += transform.position;

        return position;
    }

    private Vector3 CalculateGridPosition(int x, int y, int z)
    {
        Vector3 position = new Vector3(x, y, z);
        position.x += gridCellPadding.x;
        position.y += gridCellPadding.y;
        position.z += gridCellPadding.z;

        // Center the grid
        var centerOffset = new Vector3(
            (gridSize.x - 1) * 0.5f + (x * (gridCellPadding.x)),
            (gridHeight - 1) * 0.5f + (y * (gridCellPadding.y) * gridHeight),
            (gridSize.y - 1) * 0.5f + (z * (gridCellPadding.z))
        );
        position -= centerOffset;

        // Adjust for the GridBuilder's position
        position += transform.position;

        return position;
    }
    
    public void MoveGridPointsToConvergedPosition()
    {
        foreach (var gridPoint in gridPoints)
        {
            gridPoint.transform.position = convertedGridPositions[gridPoint];
        }
    }
    
    public void MoveGridPointsToOriginalPosition()
    {
        foreach (var gridPoint in gridPoints)
        {
            gridPoint.transform.position = currentGridPositions[gridPoint];
        }
    }
    
    public Vector3 GetCenterPoint()
    {
        var center = Vector3.zero;
        foreach (var gridPoint in gridPoints)
        {
            center += gridPoint.transform.position;
        }

        center /= gridPoints.Count;
        Debug.DrawLine(center, center + Vector3.up * 10, Color.red, 10);
        return center;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying || gridPointPrefab == null) return;

        var maxSize = Vector3.zero;

        if (gridPrefabs.Length != 0)
        {
            foreach (var obj in gridPrefabs)
            {
                var size = obj.GetComponentInChildren<Renderer>().bounds.size;
                maxSize = Vector3.Max(maxSize, size);
            }
        }

        if (maxSize == Vector3.zero)
            maxSize = gridPointPrefab.GetComponentInChildren<Renderer>().bounds.size * blockSizeMultiplier;

        maxSize *= blockSizeMultiplier;
        
        gridHeight = Mathf.Max(gridHeight, 1);

        var list = new List<Vector3>();

        Gizmos.color = Color.green;
        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridSize.x; x++)
            {
                for (var z = 0; z < gridSize.y; z++)
                {
                    Vector3 position = CalculateGridPosition(x, y, z);
                    list.Add(position);

                    Gizmos.DrawWireCube(position, maxSize);
                }
            }
        }

        Gizmos.color = Color.red;
        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridSize.x; x++)
            {
                for (var z = 0; z < gridSize.y; z++)
                {
                    var position = CalculateGridpointConvergePositions(x, y, z);
                    list.Add(position);

                    Gizmos.DrawWireCube(position, maxSize);
                }
            }
        }
    }
}
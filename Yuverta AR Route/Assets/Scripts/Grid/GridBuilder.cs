using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(GridManager))]
public class GridBuilder : MonoBehaviour
{
    [SerializeField] private Vector2Int gridSize;

    [SerializeField] [Range(1, 2)] private int gridHeight;
    [SerializeField] private Vector3 gridCellPadding;
    [SerializeField] private float blockSizeMultiplier = 1;
    [SerializeField] private GameObject gridPointPrefab;
    [SerializeField] private Vector3 stoppingDistance = new Vector3(0.47f, 1, 0.725f);
    [SerializeField] private float blockSize;

    public void BuildGrid()
    {
        var size = gridPointPrefab.GetComponentInChildren<Renderer>().bounds.size * blockSizeMultiplier;

        //Making sure gridHeight does not equal 0 to prevent no grid being created
        gridHeight = gridHeight > 0 ? gridHeight : 1;

        var indexOfGridPoint = 0;

        // Create a grid of points
        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridSize.x; x++)
            {
                for (var z = 0; z < gridSize.y; z++)
                {
                    Vector3 position = new(x, y, z);
                    position.x += gridCellPadding.x;
                    position.y += gridCellPadding.y;
                    position.z += gridCellPadding.z;

                    //center the grid
                    var centerOffset = new Vector3(
                        (gridSize.x - 1) * 0.5f + (x * (gridCellPadding.x)),
                        (gridHeight - 1) * 0.5f + (y * (gridCellPadding.y) * gridHeight),
                        (gridSize.y - 1) * 0.5f + (z * (gridCellPadding.z))
                    );
                    position -= centerOffset;

                    // Set the position of the grid point
                    position += transform.position;

                    // Instantiate the grid point
                    var gridPoint = Instantiate(gridPointPrefab, position, Quaternion.identity, transform);
                    gridPoint.transform.localScale = size;
                    
                    var gridPointScript = gridPoint.GetComponent<GridPointScript>();

                    // Set the object position of the grid point based on the indexOfGridPoint
                    gridPointScript.objectPosition = (GridManager.ObjectPosition)indexOfGridPoint;

                    // Inputting rotation here later
                    gridPoint.name = $"{gridPointScript.objectPosition} {x} {y} {z}";

                    // Keep track of the index of the grid point
                    ++indexOfGridPoint;
                }
            }
        }
    }

    public void ClearGrid()
    {
        
        // Destroy all children of the grid in forloop
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            if(Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        var size = gridPointPrefab.GetComponentInChildren<Renderer>().bounds.size * blockSizeMultiplier;

        //Making sure gridHeight does not equal 0 to prevent no grid being created
        gridHeight = gridHeight > 0 ? gridHeight : 1;

        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridSize.x; x++)
            {
                for (var z = 0; z < gridSize.y; z++)
                {
                    Vector3 position = new(x, y, z);
                    position.x += gridCellPadding.x;
                    position.y += gridCellPadding.y;
                    position.z += gridCellPadding.z;

                    //center the grid
                    var centerOffset = new Vector3(
                        (gridSize.x - 1) * 0.5f + (x * (gridCellPadding.x)),
                        (gridHeight - 1) * 0.5f + (y * (gridCellPadding.y) * gridHeight),
                        (gridSize.y - 1) * 0.5f + (z * (gridCellPadding.z))
                    );
                    position -= centerOffset;


                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(position, size);
                }
            }
        }
    }
}
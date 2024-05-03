using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(GridManager))]
public class GridBuilder : MonoBehaviour
{
    [SerializeField]
    private Vector2 gridsize = new Vector2(10, 10);
    [SerializeField]
    [Min(1)]
    private int gridHeight = 0;
    [SerializeField]
    private float gridCellPadding = 0f;
    [SerializeField]
    private GameObject gridPointPrefab;

    private GridManager gridManager;
    // Start is called before the first frame update
    void Start()
    {
        if (gridManager == null)
        {
            gridManager = GetComponent<GridManager>();
        }

        BuildGrid();
    }

    public void BuildGrid()
    {
        if (gridManager == null)
        {
            gridManager = GetComponent<GridManager>();
        }
        gridCellPadding = gridCellPadding > 0 ? gridCellPadding : gridPointPrefab.GetComponent<Renderer>().bounds.size.x * 2;
        // Create a grid of points
        for (int y = 0; y < gridHeight; y++)
        {
            GameObject layerParent = new GameObject($"Layer {y}");
            layerParent.transform.position = gameObject.transform.position;
            layerParent.transform.SetParent(transform);
            for (int x = 0; x < gridsize.x; x++)
            {
                for (int z = 0; z < gridsize.y; z++)
                {
                    Vector3 position = new(x, y, z);
                    position *= gridCellPadding;
                    //center the grid horizontally on the x and z axis
                    Vector3 center = new Vector3(gridsize.x / 2, 0, gridsize.y / 2) * gridCellPadding;
                    center -= position + Vector3.one * gridCellPadding * 0.5f;
                    position = new Vector3(center.x, position.y, center.z);
                    position += transform.position;
                    GameObject gridPoint = Instantiate(gridPointPrefab, position, Quaternion.identity, layerParent.transform);
                    gridPoint.name = $"GridPoint ({x}, {y}, {z})";
                    gridManager.gridPoints.Add(gridPoint);
                    if (GridLayering.Instance != null)
                    {
                        GridLayering.Instance.gridSortedLayer.Add(gridPoint, y);
                    }
                }
            }
        }
        if (GridLayering.Instance != null)
        {
            GridLayering.Instance.gridDimensions = new Vector2Int(0, gridHeight - 1);
        }
    }

    public void ClearGrid()
    {
#if UNITY_EDITOR

        if (UnityEditor.EditorUtility.DisplayDialog("Clear Grid", "Are you sure you want to clear the grid?", "Yes", "No"))
        {
            foreach (var gridPoint in gridManager.gridPoints)
            {
                DestroyImmediate(gridPoint);
            }
            gridManager.gridPoints.Clear();

            return;
        }

        
#endif
        foreach (var gridPoint in gridManager.gridPoints)
        {
            Destroy(gridPoint);
        }
        gridManager.gridPoints.Clear();
    }

    private void OnDrawGizmos()
    {
        //Making sure gridCellPadding does not equal 0 to prevent all positions being 0
        var localPadding = gridCellPadding = gridCellPadding > 0 ? gridCellPadding : gridPointPrefab.GetComponent<Renderer>().bounds.size.x * 2;

        //Making sure gridHeight does not equal 0 to prevent no grid being created
        gridHeight = gridHeight > 0 ? gridHeight : 1;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridsize.x; x++)
            {
                for (int z = 0; z < gridsize.y; z++)
                {
                    Vector3 position = new(x, y, z);
                    position *= localPadding;
                    //center the grid horizontally on the x and z axis
                    Vector3 center = new Vector3(gridsize.x / 2, 0, gridsize.y / 2) * localPadding;
                    center -= position + Vector3.one * localPadding * 0.5f;
                    position = new Vector3(center.x, position.y, center.z);
                    position += transform.position;
                    Gizmos.color = Color.green;

                    Gizmos.DrawWireCube(position, Vector3.one * gridCellPadding);
                    Gizmos.color = Color.cyan;

                    Gizmos.DrawWireSphere(position, localPadding * 0.25f);
                }
            }
        }
    }
}

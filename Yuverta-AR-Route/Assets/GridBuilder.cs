using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(GridManager))]
public class GridBuilder : MonoBehaviour
{
    [SerializeField]
    private Vector2 gridsize = new Vector2(10, 10);
    [SerializeField]
    private float gridCellPadding = 0f;
    [SerializeField]
    private GameObject gridPointPrefab;

    private GridManager gridManager;
    // Start is called before the first frame update
    void Start()
    {
        if(gridManager == null)
        {
            gridManager = GetComponent<GridManager>();
        }
        
        BuildGrid();
    }

    public void BuildGrid()
    {
        //Making sure gridCellPadding does not equal 0 to prevent all positions being 0
        gridCellPadding = gridCellPadding > 0 ? gridCellPadding : gridPointPrefab.GetComponent<Renderer>().bounds.size.x * 2;
        
        // Create a grid of points
        for (int x = 0; x < gridsize.x; x++)
        {
            for (int z = 0; z < gridsize.y; z++)
            {
                Vector3 position = new (x, 0, z);
                position *= gridCellPadding;
                GameObject gridPoint = Instantiate(gridPointPrefab, position, Quaternion.identity, transform);
                gridPoint.name = $"GridPoint ({x}, {z})";
                gridManager.gridPoints.Add(gridPoint);
            }
        }
    }

    public void ClearGrid()
    {
        foreach (var gridPoint in gridManager.gridPoints)
        {
            Destroy(gridPoint);
        }
        gridManager.gridPoints.Clear();
    }

    private void OnDrawGizmos()
    {
        var localPadding = gridCellPadding = gridCellPadding > 0 ? gridCellPadding : gridPointPrefab.GetComponent<Renderer>().bounds.size.x * 2;
        
        for (int x = 0; x < gridsize.x; x++)
        {
            for (int z = 0; z < gridsize.y; z++)
            {
                Vector3 position = new (x, 0, z);
                position *= localPadding;
                Gizmos.color = Color.green;

                Gizmos.DrawWireCube(position, Vector3.one * gridCellPadding);
                Gizmos.color = Color.cyan;

                Gizmos.DrawWireSphere(position, localPadding * 0.25f);
            }
        }
    }
}

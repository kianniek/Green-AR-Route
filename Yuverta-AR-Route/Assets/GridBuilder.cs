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

    private GridManager gridManager;
    // Start is called before the first frame update
    void Start()
    {
        if(gridManager == null)
        {
            gridManager = GetComponent<GridManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BuildGrid()
    {
        // Create a grid of points
        for (int x = 0; x < gridsize.x; x++)
        {
            for (int z = 0; z < gridsize.y; z++)
            {
                Vector3 position = new (x, 0, z);
                position *= gridCellPadding;
                GameObject gridPoint = new ($"Grid Point {x}-{z}");
                gridPoint.transform.position = position;
                gridPoint.transform.SetParent(transform);
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
        for (int x = 0; x < gridsize.x; x++)
        {
            for (int z = 0; z < gridsize.y; z++)
            {
                Vector3 position = new (x, 0, z);
                position *= gridCellPadding;
                Gizmos.color = Color.green;

                Gizmos.DrawWireCube(position, Vector3.one * gridCellPadding);
                Gizmos.color = Color.cyan;

                Gizmos.DrawWireSphere(position, 0.1f);
            }
        }
    }
}

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
    [SerializeField] 
    private GameObject gridParent;
    
    public List<GameObject> layerParents = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        BuildGrid();
    }

    public void BuildGrid()
    {
        // Create a grid of points
        for (int y = 0; y < gridHeight; y++)
        {
            var layerParent = Instantiate(gridParent, gameObject.transform.position, Quaternion.identity, transform);
            layerParent.name = $"Layer {y}";
            
            layerParents.Add(layerParent);
            for (int x = 0; x < gridsize.x; x++)
            {
                for (int z = 0; z < gridsize.y; z++)
                {
                    Vector3 position = new (x, y, z);
                    position *= gridCellPadding;
                    GameObject gridPoint = Instantiate(gridPointPrefab, position, Quaternion.identity, layerParent.transform);
                    gridPoint.name = $"GridPoint ({x}, {y}, {z})";
                    GridManager.Instance.gridPoints.Add(gridPoint);
                    GridManager.Instance.gridLayering.gridSortedLayer.Add(gridPoint, y);
                }
            }

            Transform firstChild = layerParent.transform.GetChild(0);
            Transform lastChild = layerParent.transform.GetChild(layerParent.transform.childCount - 1);
            
            var sizeChild = firstChild.GetComponent<Renderer>().bounds.size;
            
            var distanceX = lastChild.position.x - firstChild.position.x;
            var distanceZ = lastChild.position.z - firstChild.position.z;
            
            layerParent.GetComponent<BoxCollider>().center = new Vector3(distanceX / 2, 0, distanceZ / 2);
            
            distanceX += sizeChild.x;
            distanceZ += sizeChild.z;
            layerParent.GetComponent<BoxCollider>().size = new Vector3(distanceX, 0.001f, distanceZ);
        }
        
        GridManager.Instance.gridLayering.gridDimensions = new Vector2Int(0, gridHeight - 1);
    }

    public void ClearGrid()
    {
        foreach (var gridPoint in GridManager.Instance.gridPoints)
        {
            Destroy(gridPoint);
        }
        GridManager.Instance.gridPoints.Clear();
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
                    Vector3 position = new (x, y, z);
                    position *= localPadding;
                    Gizmos.color = Color.green;

                    Gizmos.DrawWireCube(position, Vector3.one * gridCellPadding);
                    Gizmos.color = Color.cyan;

                    Gizmos.DrawWireSphere(position, localPadding * 0.25f);
                }
            }
        }
    }
}

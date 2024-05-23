using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(GridManager))]
public class GridBuilder : MonoBehaviour
{
    [SerializeField]
    private Vector2 gridSize;

    [SerializeField] 
    [Range(1, 2)] 
    private int gridHeight;
    [SerializeField]
    private float gridCellPadding;
    [SerializeField]
    private GameObject gridPointPrefab;
    [SerializeField] 
    private GameObject gridParent;
    
    public List<GameObject> layerParents = new List<GameObject>();
    
    private float blockSize;
    // Start is called before the first frame update
    void Start()
    {
        var obj = GridManager.Instance.objsToSpawnAmount.keys[0];
        blockSize = obj.GetComponent<MeshRenderer>().bounds.size.y / 4;
        BuildGrid();
    }

    public void BuildGrid()
    {
        gridCellPadding = gridCellPadding > 0 ? gridCellPadding : gridPointPrefab.GetComponent<Renderer>().bounds.size.x * 2;
        gridCellPadding += blockSize;
        // Create a grid of points
        for (int y = 0; y < gridHeight; y++)
        {
            var layerParentPos = gameObject.transform.position;
            layerParentPos.y += y * gridCellPadding;
            var layerParent = Instantiate(gridParent, layerParentPos, Quaternion.identity, transform);
            layerParent.name = $"Layer {y}";
            
            layerParents.Add(layerParent);
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int z = 0; z < gridSize.y; z++)
                {
                    Vector3 position = new(x, y, z);
                    position *= gridCellPadding;
                    //center the grid horizontally on the x and z axis
                    Vector3 center = new Vector3(gridSize.x / 2, 0, gridSize.y / 2) * gridCellPadding;
                    center -= position + Vector3.one * gridCellPadding * 0.5f;
                    position = new Vector3(center.x, position.y, center.z);
                    position += transform.position;
                    GameObject gridPoint = Instantiate(gridPointPrefab, position, Quaternion.identity, layerParent.transform);
                    var script = gridPoint.GetComponent<GridPointScript>();
                    script.objectPosition =
                        GridManager.Instance.ObjectPositionName[GridManager.Instance.gridPoints.Count];
                    //Inputting rotation here later
                    gridPoint.name =
                        $"{script.objectPosition} {x} {y} {z}";
                    GridManager.Instance.gridPoints.Add(gridPoint, y);
                    GridManager.Instance.gridLayering.gridSortedLayer.Add(gridPoint, y);
                }
            }

            Transform firstChild = layerParent.transform.GetChild(0);
            Transform lastChild = layerParent.transform.GetChild(layerParent.transform.childCount - 1);
            
            var sizeChild = firstChild.GetComponentInChildren<Renderer>().bounds.size;
            
            var distanceX = (lastChild.position.x - firstChild.position.x) * 4;
            var distanceZ = (lastChild.position.z - firstChild.position.z) * 4;
            
            distanceX += sizeChild.x;
            distanceZ += sizeChild.z;
            layerParent.GetComponent<BoxCollider>().size = new Vector3(Mathf.Abs(distanceX), 0.001f, Mathf.Abs(distanceZ));
        }
        
        GridManager.Instance.distanceLayers = gridCellPadding;
        GridManager.Instance.gridLayering.gridDimensions = new Vector2Int(0, gridHeight - 1);
    }

    public void ClearGrid()
    {
#if UNITY_EDITOR

        if (UnityEditor.EditorUtility.DisplayDialog("Clear Grid", "Are you sure you want to clear the grid?", "Yes", "No"))
        {
            foreach (var gridPoint in GridManager.Instance.gridPoints)
            {
                DestroyImmediate(gridPoint.Key);
            }
            GridManager.Instance.gridPoints.Clear();

            return;
        }

        
#endif
        foreach (var gridPoint in GridManager.Instance.gridPoints)
        {
            Destroy(gridPoint.Key);
        }
        GridManager.Instance.gridPoints.Clear();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        
        //Making sure gridCellPadding does not equal 0 to prevent all positions being 0
        var localPadding = gridCellPadding = gridCellPadding > 0 ? gridCellPadding : gridPointPrefab.GetComponent<Renderer>().bounds.size.x * 2;

        //Making sure gridHeight does not equal 0 to prevent no grid being created
        gridHeight = gridHeight > 0 ? gridHeight : 1;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int z = 0; z < gridSize.y; z++)
                {
                    Vector3 position = new(x, y, z);
                    position *= localPadding;
                    //center the grid horizontally on the x and z axis
                    Vector3 center = new Vector3(gridSize.x / 2, 0, gridSize.y / 2) * localPadding;
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

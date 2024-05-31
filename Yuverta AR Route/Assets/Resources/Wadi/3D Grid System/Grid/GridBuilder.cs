using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(GridManager))]
public class GridBuilder : MonoBehaviour
{
    [SerializeField] private Vector2 gridSize;

    [SerializeField] [Range(1, 2)] private int gridHeight;
    [SerializeField] private float gridCellPadding;
    [SerializeField] private GameObject gridPointPrefab;
    [SerializeField] private GameObject gridParent;

    public List<GameObject> layerParents = new List<GameObject>();

    [SerializeField] private Vector3 stoppingDistance = new Vector3(-0.1f, 1, 0.4f);
    private float blockSize;

    // Start is called before the first frame update
    void Start()
    {
        stoppingDistance = new Vector3(-0.1f, 1, 0.4f);
        var obj = GridManager.Instance.objsToSpawnAmount.keys[0];
        blockSize = obj.GetComponent<MeshRenderer>().bounds.size.y / 4;
        BuildGrid();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void BuildGrid()
    {
        gridCellPadding = gridCellPadding > 0
            ? gridCellPadding
            : gridPointPrefab.GetComponent<Renderer>().bounds.size.x * 2;
        gridCellPadding += blockSize;

        var indexOfGridPoint = 0;
        // Create a grid of points
        for (int y = 0; y < gridHeight; y++)
        {
            var layerParentPos = gameObject.transform.position;
            layerParentPos.y += y * gridCellPadding;
            
            var layerParent = Instantiate(gridParent, layerParentPos, Quaternion.identity, transform);
            layerParent.name = $"Layer {y}";
            
            var centerObjects = layerParent.AddComponent<CenterObjects>();
            centerObjects.limitAxis = CenterObjects.Axis.None;
            centerObjects.stoppingDistance = stoppingDistance;

            layerParents.Add(layerParent);
            for (int z = 0; z < gridSize.y; z++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    // Calculate the position of the grid point
                    Vector3 position = new(x, y, z);
                    position *= gridCellPadding;

                    // Center the grid horizontally on the x and z axis
                    Vector3 centerOffset = new Vector3(gridSize.x - 1, 0, gridSize.y - 1) * gridCellPadding * 0.5f;
                    position -= centerOffset;

                    // Set the position of the grid point
                    position += transform.position;

                    // Instantiate the grid point
                    var gridPoint = Instantiate(gridPointPrefab, position, Quaternion.identity, layerParent.transform);
                    var gridPointScript = gridPoint.GetComponent<GridPointScript>();

                    // Set the object position of the grid point based on the indexOfGridPoint
                    gridPointScript.objectPosition = (GridManager.ObjectPosition)indexOfGridPoint;

                    // Inputting rotation here later
                    gridPoint.name = $"{gridPointScript.objectPosition} {x} {y} {z}";
                    GridManager.Instance.gridPoints.Add(gridPoint, y);
                    GridManager.Instance.gridLayering.gridSortedLayer.Add(gridPoint, y);

                    // Keep track of the index of the grid point
                    ++indexOfGridPoint;
                }
            }

            //Add all children to the center objects script
            centerObjects.objects = new List<GameObject>();
            for (int i = 0; i < layerParent.transform.childCount; i++)
            {
                centerObjects.objects.Add(layerParent.transform.GetChild(i).gameObject);
            }

            var firstChild = layerParent.transform.GetChild(0);
            var lastChild = layerParent.transform.GetChild(layerParent.transform.childCount - 1);

            var sizeChild = firstChild.GetComponentInChildren<Renderer>().bounds.size;

            var distanceX = (lastChild.position.x - firstChild.position.x) * 4;
            var distanceZ = (lastChild.position.z - firstChild.position.z) * 4;

            distanceX += sizeChild.x;
            distanceZ += sizeChild.z;
            layerParent.GetComponent<BoxCollider>().size =
                new Vector3(Mathf.Abs(distanceX), 0.001f, Mathf.Abs(distanceZ));
            
            GridManager.Instance.CenterObjectsList.Add(centerObjects);
        }
        GridManager.Instance.distanceLayers = gridCellPadding;
        GridManager.Instance.gridLayering.gridDimensions = new Vector2Int(0, gridHeight - 1);
    }

    public void ClearGrid()
    {
#if UNITY_EDITOR

        if (UnityEditor.EditorUtility.DisplayDialog("Clear Grid", "Are you sure you want to clear the grid?", "Yes",
                "No"))
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
        var localPadding = gridCellPadding = gridCellPadding > 0
            ? gridCellPadding
            : gridPointPrefab.GetComponent<Renderer>().bounds.size.x * 2;

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
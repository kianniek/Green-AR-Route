using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject cubePrefab; // Assign your cube prefab in the inspector
    public int width = 2; // Grid width
    public int height = 6; // Grid height
    public int depth = 2; // Grid depth
    public float spacingMultiplier = 1.1f; // Multiplier to adjust spacing slightly larger than the cube dimensions

    private Vector3 cubeSize;

    void Start()
    {
        if (cubePrefab != null)
        {
            cubeSize = cubePrefab.GetComponent<Renderer>().bounds.size;
            GenerateGrid();
        }
        else
        {
            Debug.LogError("Cube prefab is not assigned!");
        }
    }

    void GenerateGrid()
    {
        // Calculate the total grid dimensions
        Vector3 gridDimensions = new Vector3(
            width * (cubeSize.x * spacingMultiplier),
            height * (cubeSize.y * spacingMultiplier),
            depth * (cubeSize.z * spacingMultiplier)
        );

        // Calculate the starting position to center the grid
        Vector3 startPosition = new Vector3(
            transform.position.x - gridDimensions.x / 2 + cubeSize.x / 2,
            transform.position.y - gridDimensions.y / 2 + cubeSize.y / 2,
            transform.position.z - gridDimensions.z / 2 + cubeSize.z / 2
        );

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    Vector3 position = new Vector3(
                        startPosition.x + x * (cubeSize.x * spacingMultiplier),
                        startPosition.y + y * (cubeSize.y * spacingMultiplier),
                        startPosition.z + z * (cubeSize.z * spacingMultiplier)
                    );
                    GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
                    cube.transform.parent = this.transform; // Organize under the grid parent

                    // Make sure each cell has a collider and is tagged for raycasting
                    cube.tag = "GridCell";
                    if (!cube.GetComponent<Collider>())
                    {
                        cube.AddComponent<BoxCollider>().isTrigger = true; // Add a trigger collider
                    }

                    cube.name = "GridCell_" + x + "_" + y + "_" + z;
                }
            }
        }
    }
}

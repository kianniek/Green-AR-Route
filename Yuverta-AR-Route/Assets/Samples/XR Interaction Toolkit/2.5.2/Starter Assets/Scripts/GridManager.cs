using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private float gridSize = 1.0f;
    public float GridSize
    {
        get => gridSize;
        set => gridSize = value;
    }

    // Dictionary to hold the status of grid positions
    private Dictionary<Vector3, bool> occupiedPositions = new Dictionary<Vector3, bool>();

    //display the occupied positions in the editor


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Function to check if a grid position is occupied
    public bool IsPositionOccupied(Vector3 position)
    {
        return occupiedPositions.TryGetValue(position, out bool isOccupied) && isOccupied;
    }

    // Function to mark a grid position as occupied
    public void SetPositionOccupied(Vector3 position, bool occupied)
    {
        occupiedPositions[position] = occupied;
    }

    public void RemovePositionOccupied(Vector3 position)
    {
        if (occupiedPositions.ContainsKey(position))
        {
            occupiedPositions.Remove(position);
        }
    }

    public Dictionary<Vector3, bool> OccupiedPositions
    {
        get
        {
            return occupiedPositions;
        }
    }
}

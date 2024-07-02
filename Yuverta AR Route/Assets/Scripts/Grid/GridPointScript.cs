using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GridPointScript : MonoBehaviour
{
    public GridManager.ObjectGridLocation objectGridLocation;
    
    private void Start()
    {
        gameObject.name = objectGridLocation.ToString();
    }
}

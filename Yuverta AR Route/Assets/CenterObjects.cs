using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CenterObjects : MonoBehaviour
{
    public List<GameObject> objects;

    public Vector3 stoppingDistance = new Vector3(0.1f, 0.1f, 0.1f);

    private Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Vector3> centerPositions = new Dictionary<GameObject, Vector3>();


    private Vector3 globalCenter;
    private void Start()
    {
        initialPositions = new Dictionary<GameObject, Vector3>();
        centerPositions = new Dictionary<GameObject, Vector3>();
        CalculateCenterPositions();
    }

    public void CalculateCenterPositions()
    {
        if (objects == null || objects.Count == 0)
        {
            Debug.LogError("No objects assigned.");
            return;
        }
        
        // Store initial positions
        foreach (var obj in objects)
        {
            initialPositions.Add(obj, obj.transform.localPosition);
        }
        
        globalCenter = CalculateCenterPosition(initialPositions.Values.ToList());

        // Store center positions
        foreach (var initialPosition in initialPositions)
        {
            centerPositions.Add(initialPosition.Key, CalculateObjectsCenteredPosition(initialPosition.Key, globalCenter));
        }
    }

    public void MoveObjectsToCenter()
    {
        if (objects == null || objects.Count == 0)
        {
            Debug.LogError("No objects assigned.");
            return;
        }

        foreach (var t in objects)
        {
            //check if t is in centerPositions and if not calculate it
            if (!centerPositions.ContainsKey(t))
            {
                centerPositions.Add(t, CalculateObjectsCenteredPosition(t, globalCenter));
            }
            
            t.transform.localPosition = centerPositions[t];
        }
    }
    
    public void ResetPositions()
    {
        if (objects == null || objects.Count == 0)
        {
            Debug.LogError("No objects assigned.");
            return;
        }

        foreach (var t in objects)
        {
            t.transform.localPosition = initialPositions[t];
        }
    }

    private Vector3 CalculateCenterPosition(List<Vector3> Positions)
    {
        var center = new Vector3();
        foreach (var pos in Positions)
        {
            center += pos;
        }

        center /= Positions.Count;
        return center;
    }

    private Vector3 CalculateObjectsCenteredPosition(GameObject obj, Vector3 center)
    {
        var local = obj.transform.localPosition;
        var x = local.x * stoppingDistance.x;
        var y = local.y * stoppingDistance.y;
        var z = local.z * stoppingDistance.z;

        var newPos = new Vector3(x, y, z);
        return newPos;
    }
}
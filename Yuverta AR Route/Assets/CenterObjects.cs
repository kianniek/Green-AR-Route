using System;
using System.Collections.Generic;
using UnityEngine;

public class CenterObjects : MonoBehaviour
{
    public enum Axis
    {
        None,
        X,
        Y,
        Z
    }

    public List<GameObject> objects;
    
    public Vector3 stoppingDistance = new Vector3(0.1f,0.1f,0.1f);
    
    public Axis limitAxis = Axis.None;

    private List<Vector3> initalPositions = new List<Vector3>();

    private void Start()
    {
        // Store initial positions

        initalPositions.Clear();
        foreach (var obj in objects)
        {
            initalPositions.Add(obj.transform.position);
        }
    }

    public void MoveToCenter()
    {
        if (objects == null || objects.Count == 0)
        {
            Debug.LogError("No objects assigned.");
            return;
        }
        
        Debug.Log("Moving objects to center.");
        Vector3 center = CalculateCenterPosition(objects);
        Debug.Log($"Center position: {center}");
        MoveObjectsToCenter(objects, center);
    }

    public void ResetPositions()
    {
        if (objects == null || objects.Count == 0)
        {
            Debug.LogError("No objects assigned.");
            return;
        }

        Debug.Log("Resetting object positions.");
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].transform.position = initalPositions[i];
        }
    }

    Vector3 CalculateCenterPosition(List<GameObject> objects)
    {
        Vector3 center = Vector3.zero;

        foreach (GameObject obj in objects)
        {
            center += obj.transform.position;
        }

        center /= objects.Count;

        return center;
    }

    private void MoveObjectsToCenter(List<GameObject> objects, Vector3 center)
    {
        foreach (var obj in objects)
        {
            var collider = obj.GetComponentInChildren<Collider>();
            if (collider)
            {
                var local = obj.transform.localPosition;
                var x = local.x * stoppingDistance.x;
                var y = local.y * stoppingDistance.y;
                var z = local.z * stoppingDistance.z;
                obj.transform.localPosition = new Vector3(x, y, z);
            }
        }
    }
}
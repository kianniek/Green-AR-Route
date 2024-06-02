using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class CenterHorizontaly : MonoBehaviour
{
    private List<GameObject> objects = new List<GameObject>();
    private List<Vector3> initialPositions = new List<Vector3>();
    private List<Vector3> centerPositions = new List<Vector3>();

    public GameObject AddObject
    {
        get => objects[^1];
        set
        {
            objects.Add(value);
            initialPositions.Add(value.transform.localPosition);
            var center = CalculateCenterPosition(objects);
            centerPositions.Add(CalculateObjectsCenteredPosition(value, center));
        }
    }
    
    public void CenterObjects()
    {
        CenterObjectsHorizontaly(objects);
    }
    
    public void MoveCenteredObjecsBack()
    {
        foreach (var obj in objects)
        {
            obj.transform.localPosition = Vector3.zero;
        }
    }
    
    
    private void CenterObjectsHorizontaly(List<GameObject> objects)
    {
        if (objects == null || objects.Count == 0)
        {
            Debug.LogError("No objects assigned.");
            return;
        }

        var center = CalculateCenterPosition(objects);

        foreach (var obj in objects)
        {
            obj.transform.localPosition = CalculateObjectsCenteredPosition(obj, center);
        }
    }
    
    private Vector3 CalculateCenterPosition(List<GameObject> objects)
    {
        var center = Vector3.zero;
        foreach (var obj in objects)
        {
            center += obj.transform.localPosition;
        }

        center /= objects.Count;
        return center;
    }
    
    private Vector3 CalculateObjectsCenteredPosition(GameObject obj, Vector3 center)
    {
        var pos = obj.transform.localPosition;
        var newPos = new Vector3(center.x, pos.y, center.z);
        return newPos;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class CenterVertically : MonoBehaviour
{
    [Tooltip("Multiplier for the movement of the objects to the center")]
    public float centerMultiplier = 1.3f;
    private List<GameObject> objects = new List<GameObject>();
    
    private Dictionary<GameObject, Vector3> initialPositionsDict = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Vector3> centerPositionsDict = new Dictionary<GameObject, Vector3>();

    private Vector3 globalCenter;
    public GameObject AddObject
    {
        get => objects[^1];
        set
        {
            objects.Add(value);
            initialPositionsDict.Add(value, value.transform.localPosition);
        }
    }
    
    public void CenterObjects()
    {
        if (objects == null || objects.Count == 0)
        {
            Debug.LogError("No objects assigned.");
            return;
        }
        
        //clear center dictionary
        centerPositionsDict.Clear();
        
        globalCenter = CalculateCenterPosition(objects);
        
        foreach (var obj in objects)
        {
            var objectCenteredPosition = CalculateObjectsCenteredPosition(obj, globalCenter);
            centerPositionsDict.Add(obj, objectCenteredPosition);
        }
        
        CenterObjectsVertically();
    }
    
    public void MoveCenteredObjectsBack()
    {
        foreach (var obj in objects)
        {
            obj.transform.localPosition = initialPositionsDict[obj];
        }
    }
    
    
    private void CenterObjectsVertically()
    {
        foreach (var obj in objects)
        {
            //check if obj is in centerPositions and if not calculate it
            if (!centerPositionsDict.ContainsKey(obj))
            {
                centerPositionsDict.Add(obj, CalculateObjectsCenteredPosition(obj, globalCenter));
            }
            
            obj.transform.localPosition = centerPositionsDict[obj];
        }
    }
    
    private Vector3 CalculateCenterPosition(List<GameObject> objects)
    {
        var center = Vector3.zero;
        foreach (var obj in objects)
        {
            center += initialPositionsDict[obj];
        }

        center /= objects.Count;
        return center;
    }
    
    private Vector3 CalculateObjectsCenteredPosition(GameObject obj, Vector3 center)
    {
        var pos = obj.transform.localPosition;
        var pureCenter = new Vector3(pos.x, center.y, pos.z);
        
        var direction = pureCenter - pos;
        var newPos = direction * centerMultiplier + pos;
        
        return newPos;
    }
}

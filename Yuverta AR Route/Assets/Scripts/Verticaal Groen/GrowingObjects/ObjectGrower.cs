using Autodesk.Fbx;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrower : MonoBehaviour
{
    private Dictionary<GameObject, Vector3> childrenObjects = new();

    public float growthDuration = 2f; // Duration for the flower to fully grow

    void Awake()
    {
        foreach (Transform obj in transform)
        {
            childrenObjects.Add(obj.gameObject, obj.transform.localScale);
            obj.transform.localScale = Vector3.zero;
        }

    }

    public IEnumerator Grow(GameObject Key, Vector3 Value)
    {

        var elapsedTime = 0f;

        while (elapsedTime < growthDuration)
        {
            Key.transform.localScale = Vector3.Lerp(Vector3.zero, Value, elapsedTime / growthDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Key.transform.localScale = Value;
    }

    public void GrowChildObjects()
    {
        foreach (var item in childrenObjects)
        {
            StartCoroutine(Grow(item.Key, item.Value));
        }
    }

    public void ResetSize()
    {
        StopAllCoroutines();
        transform.localScale = Vector3.zero;
    }
}
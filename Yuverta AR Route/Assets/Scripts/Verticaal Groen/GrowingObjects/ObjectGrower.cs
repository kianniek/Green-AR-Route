using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectGrower : MonoBehaviour
{
    private Dictionary<GameObject, Vector3> childrenObjects = new();

    [SerializeField] private GameObject[] individualObjectsToGrow;

    [SerializeField] private UnityEvent OnGrownObjects;
    public float growthDuration = 2f; // Duration for the flower to fully grow

    private bool hasGrownObjects;


    void Awake()
    {
        foreach (Transform obj in transform)
        {
            childrenObjects.Add(obj.gameObject, obj.transform.localScale);
            obj.transform.localScale = Vector3.zero;
        }

        //Add objects from individualObjectsToGrow into the dictionary for child objects aswell
        foreach (GameObject obj in individualObjectsToGrow)
        {
            childrenObjects.Add(obj, obj.transform.localScale);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="covagageID">Is used to determine which paint index was used to complete the covarage</param>
    public void GrowChildObjects(int covagageID)
    {
        if (covagageID > 0)
        {
            if (hasGrownObjects)
            {
                return;
            }

            foreach (var item in childrenObjects)
            {
                StartCoroutine(Grow(item.Key, item.Value));
            }

            OnGrownObjects.Invoke();

            hasGrownObjects = true;
        }
    }

    public void ResetSize()
    {
        StopAllCoroutines();
        transform.localScale = Vector3.zero;
    }
}
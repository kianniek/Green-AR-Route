using System;
using UnityEngine;

public class CopyChildren : MonoBehaviour
{
    // The reference to the GameObject whose children we want to copy
    public GameObject referenceObject;
    
    public bool removeColliders = true;

    public void TriggerChildUpdate()
    {
        if (referenceObject != null)
        {
            CopyChildrenFromReference();
        }
    }

    public void Update()
    {
        if (referenceObject != null)
        {
            CopyChildrenFromReference();
        }
    }


    private void CopyChildrenFromReference()
    {
        // Clear existing children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        if(referenceObject.transform.childCount == 0)
            return;

        // Copy children from the reference object
        foreach (Transform child in referenceObject.transform)
        {
            GameObject newChild = Instantiate(child.gameObject, transform);
            if (removeColliders)
            {
                Collider[] colliders = newChild.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    Destroy(collider);
                }
            }
            newChild.transform.localPosition = child.localPosition;
            newChild.transform.localRotation = child.localRotation;
            newChild.transform.localScale = child.localScale;
        }
    }
}
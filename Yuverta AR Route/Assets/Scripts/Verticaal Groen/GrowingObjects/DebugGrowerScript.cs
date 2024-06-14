using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGrowerScript : MonoBehaviour
{
    public float growthDuration = 2f; // Duration for the flower to fully grow
    public float maxGrowDistance = 5f; // Max distance for the flower to grow fully
    private Vector3 simulatedHitPosition; // Position to simulate bullet hits
    public float resetDelay = 1f; // Delay before resetting flowers

    private bool isGrowing = false;

    void Start()
    {
        StartCoroutine(SimulateBulletHit());
    }

    private void Update()
    {
        simulatedHitPosition = transform.position;
    }

    private IEnumerator SimulateBulletHit()
    {
        while (true)
        {
            HandleBulletHit(simulatedHitPosition);

            yield return new WaitForSeconds(growthDuration + resetDelay);
            if (isActiveAndEnabled)
            {
                yield return null;
            }
            ResetFlowers();
            yield return null;
        }
    }

    private void HandleBulletHit(Vector3 hitPosition)
    {
        Collider[] hitColliders = Physics.OverlapSphere(hitPosition, maxGrowDistance);
        Debug.Log(hitColliders.Length);
        foreach (var hitCollider in hitColliders)
        {
            ObjectGrower flower = hitCollider.GetComponent<ObjectGrower>();
            Debug.Log(flower);
            if (flower != null)
            {
                
                var distance = Vector3.Distance(hitPosition, flower.transform.position);
                Debug.Log(distance);
                StartCoroutine(flower.Grow(distance, maxGrowDistance, growthDuration));
            }
        }
    }

    private void ResetFlowers()
    {
        Collider[] hitColliders = Physics.OverlapSphere(simulatedHitPosition, maxGrowDistance);
        foreach (var hitCollider in hitColliders)
        {
            ObjectGrower flower = hitCollider.GetComponent<ObjectGrower>();
            if (flower != null)
            {
                flower.ResetSize();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxGrowDistance);
    }
}

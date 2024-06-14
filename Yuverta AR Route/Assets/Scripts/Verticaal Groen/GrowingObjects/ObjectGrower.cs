using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrower : MonoBehaviour
{
    public float growthDuration = 2f; // Duration for the flower to fully grow
    public float maxGrowDistance = 5f; // Max distance for the flower to grow fully

    private Vector3 initialScale;

    void Awake()
    {
        initialScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    void OnEnable()
    {
        BulletLogic.OnBulletHit += HandleBulletHit;
    }

    void OnDisable()
    {
        BulletLogic.OnBulletHit -= HandleBulletHit;
    }

    public IEnumerator Grow(float distance, float maxDistance, float duration)
    {
        var growFactor = 1f - Mathf.Clamp01(distance / maxDistance);
        var targetScale = initialScale * growFactor;
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private void HandleBulletHit(Vector3 hitPosition)
    {
        Collider[] hitColliders = Physics.OverlapSphere(hitPosition, maxGrowDistance);
        foreach (var hitCollider in hitColliders)
        {
            float distance = Vector3.Distance(hitPosition, transform.position);
            StartCoroutine(Grow(distance, maxGrowDistance, growthDuration));
        }
    }
    
    public void ResetSize()
    {
        StopAllCoroutines();
        transform.localScale = Vector3.zero;
    }
}
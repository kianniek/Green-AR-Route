using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class LeafCollectionScript : MonoBehaviour
{
    [Serializable]
    public struct Leaf
    {
        public Image image;
        public Sprite sprite;
        public GameObject animation;
        public bool collected;
    }
    public List<Leaf> leaves;

    public UnityEvent allLeavesCollected;
    private int collectedLeafCount;
    
    private ARRaycastManager arRaycastManager;

    void Awake()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
    }

    public void SetQrCodeEvents()
    {
        for (int i = 0; i < DaktuinManager.Instance.QrCodeManager.qrCodes.Count; i++)
        {
            DaktuinManager.Instance.QrCodeManager.qrCodes[i].action.AddListener(OnLeafCollected);
        }
    }

    private void OnLeafCollected(int index)
    {
        Leaf leaf = leaves[index];
        leaf.collected = true;
        leaf.image.sprite = leaf.sprite;
        PerformRaycast(leaf.animation);
        collectedLeafCount++;
        if (collectedLeafCount == leaves.Count)
        {
            allLeavesCollected.Invoke();
        }
    }

    private void SpawnNewAnimation(GameObject animationPrefab, Vector3 position)
    {
        var animation = Instantiate(animationPrefab, position, Quaternion.identity);
        animation.transform.GetChild(1).gameObject.AddComponent<AnimationDeleter>();
    }
    
    private void PerformRaycast(GameObject animationPrefab)
    {
        // Create a list to store the raycast hits
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        // Perform the raycast straight down from the device
        if (arRaycastManager.Raycast(new Vector2(Screen.width / 2f, Screen.height / 2f), hits, TrackableType.Planes))
        {
            // If we hit a plane, get the hit position
            ARRaycastHit hit = hits[0];
            Vector3 hitPosition = hit.pose.position;

            // Spawn the object at the hit position
            SpawnNewAnimation(animationPrefab, hitPosition);
        }
    }
}

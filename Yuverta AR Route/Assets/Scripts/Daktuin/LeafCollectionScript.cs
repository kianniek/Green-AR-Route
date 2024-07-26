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
    public class Leaf
    {
        public Image image;
        public Sprite sprite;
        public GameObject animation;
        public bool collected;
        
        public Leaf(Image image, Sprite sprite, GameObject animation, bool collected)
        {
            this.image = image;
            this.sprite = sprite;
            this.animation = animation;
            this.collected = collected;
        }
        
        public void SetCollected(bool collected)
        {
            this.collected = collected;
        }
    }
    public List<Leaf> leaves;

    public UnityEvent allLeavesCollected;
    private int collectedLeafCount;
    
    public GameObject leaveUIParent;
    public GameObject leaveUIPrefab;
    
    private ARRaycastManager arRaycastManager;

    void Awake()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
    }
    
    // public void SetQrCodeEvents()
    // {
    //     for (int i = 0; i < DaktuinManager.Instance.QrCodeManager.qrCodes.Count; i++)
    //     {
    //         DaktuinManager.Instance.QrCodeManager.qrCodes[i].action.AddListener(OnLeafCollected);
    //     }
    // }

    public void OnLeafCollected(int index)
    {
        Debug.Log("Leaf collected");
        Debug.Log(index);
        
        var leaf = leaves[index];
        
        if(leaf.collected) 
            return;
        
        leaf.collected = true;
        
        
        var leafObj = Instantiate(leaveUIPrefab, leaveUIParent.transform);
        var leafVisual = leafObj.GetComponent<Image>();
        leafVisual.sprite = leaf.sprite;
        
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

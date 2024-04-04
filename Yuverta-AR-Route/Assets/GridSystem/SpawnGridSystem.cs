using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SpawnGridSystem : MonoBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;
    private ARRaycastManager raycastManager;
    
    // Start is called before the first frame update
    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 0) return;
        
        var touchPosition = Input.GetTouch(0).position;
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        raycastManager.Raycast(ray, hits, TrackableType.Planes);
        
        if (hits.Count == 0) return;

        Instantiate(prefabToSpawn, hits[0].pose.position, quaternion.identity);
        this.enabled = false;
    }
}

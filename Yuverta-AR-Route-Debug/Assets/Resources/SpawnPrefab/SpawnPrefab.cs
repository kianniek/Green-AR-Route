using System;
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
    private bool spawning;
    
    // Start is called before the first frame update
    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
    }

    /*private void Update()
    {
        if (Input.touchCount < 0 && !Input.GetMouseButtonDown(0)) return;
        
        SpawnPrefab();
    }*/

    public void SpawnPrefab()
    {
        if (spawning) return;
        spawning = true;
        Vector2 touchPosition;
        try
        {
            touchPosition = Input.GetTouch(0).position;
        }
        catch (Exception e)
        {
            touchPosition = Input.mousePosition;
        }
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        raycastManager.Raycast(ray, hits, TrackableType.Planes);
        
        if (hits.Count == 0) return;

        Instantiate(prefabToSpawn, hits[0].pose.position, quaternion.identity);
        gameObject.SetActive(false);
    }
}

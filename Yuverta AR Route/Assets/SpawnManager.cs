using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.XR.ARFoundation;

public class SpawnableManager : MonoBehaviour
{
    [SerializeField] [Tooltip("only spawn once, then disable this script")]
    bool spawnOnce = false;
    [SerializeField]
    ARRaycastManager m_RaycastManager;
    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    [SerializeField]
    GameObject spawnablePrefab;

    Camera arCam;
    GameObject spawnedObject;

    private void Start()
    {
        spawnedObject = null;
        arCam = Camera.main;
    }

    public void OnPressedScreen()
    {
        var screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        var ray = arCam.ScreenPointToRay(screenCenter);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.tag == "Spawnable")
            {
                spawnedObject = hit.collider.gameObject;
            }
            else
            {
                if (m_RaycastManager.Raycast(screenCenter, m_Hits))
                {
                    SpawnPrefab(m_Hits[0].pose.position);
                }
            }
        }
    }

    private void SpawnPrefab(Vector3 spawnPosition)
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
        }
        spawnedObject = Instantiate(spawnablePrefab);
        spawnedObject.transform.position = spawnPosition;
        
        if (spawnOnce)
            gameObject.SetActive(false);
    }
}

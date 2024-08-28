using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARRecenterManager : MonoBehaviour
{
    public static ARRecenterManager Instance { get; private set; }

    [SerializeField] private List<ARRecenter> m_Recenters = new List<ARRecenter>();

    [SerializeField, Tooltip("The AR Interactor that determines where to spawn the object.")]
    ARRaycastManager m_RaycastManager;

    [SerializeField, Tooltip("the max distance a object can be re-centered from the camera")] 
    private float maxDistance = 10f;
    
    private Camera mainCam;

    // Start is called before the first frame update
    private void Awake()
    {
        mainCam = Camera.main;
    }

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }


    public void AddRecenter(ARRecenter recenter)
    {
        m_Recenters.Add(recenter);
    }

    public void RemoveRecenter(ARRecenter recenter)
    {
        m_Recenters.Remove(recenter);
    }

    public void Recenter()
    {
        StartCoroutine(RecenterCoroutine());
    }

    private IEnumerator RecenterCoroutine()
    {
        if (m_Recenters.Count == 0)
        {
            yield return null;
        }

        foreach (var obj in m_Recenters)
        {
            var direction = mainCam.transform.forward;
            var distance = Vector3.Distance(mainCam.transform.position, obj.transform.position);
            
            var clampDistance = Mathf.Clamp(distance, 0, maxDistance);
            
            var newPosition = mainCam.transform.position + direction * clampDistance;
            
            obj.Recenter(newPosition);
        }

        yield return null;
    }
}
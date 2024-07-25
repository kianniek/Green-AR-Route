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

    private bool recenter;
    // Start is called before the first frame update
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
        recenter = true;
        StartCoroutine(RecenterCoroutine());
    }
    
    private IEnumerator RecenterCoroutine()
    {
        if(m_Recenters.Count == 0)
            yield return null;
        
        
        var m_Hits = new List<ARRaycastHit>();
        var touchPosition = new Vector2(Screen.width / 2, Screen.height / 2);
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 10, Color.red);

        do
        {

            if (m_RaycastManager.Raycast(touchPosition, m_Hits))
            {
                var arRaycastHit = m_Hits[0];
                Debug.DrawRay(arRaycastHit.pose.position, arRaycastHit.pose.up * 10, Color.green, 10f);
                var position = arRaycastHit.pose.position;

                foreach (var recenter in m_Recenters)
                {
                    recenter.Recenter(position);
                }
                recenter = false;
            }
        } while (recenter);

        yield return null;
    }
}

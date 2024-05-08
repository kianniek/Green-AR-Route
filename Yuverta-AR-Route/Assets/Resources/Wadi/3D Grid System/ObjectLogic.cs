using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ObjectLogic : MonoBehaviour
{
    //Values are set on spawn through the gridmanager
    public int layerObj;
    public int objectIndex;
    public int objectPrefabIndex;
    
    public bool isPlaced;
    public Vector3 previousSnappedPosition;

    [SerializeField] Material targetMaterial;
    private static readonly int ObjectLayerId = Shader.PropertyToID("_ObjectLayerId");
    private static readonly int CurrentLayerId = Shader.PropertyToID("_CurrentLayerId");

    private void Start()
    {
        targetMaterial = GetComponent<MeshRenderer>().material;
    }

    public void OnDestroy()
    {
        #if !ApplicationIsClosing
        GridManager.Instance.uiMenu.Add(gameObject);
        #endif
    }

    public void SetObjectLayerID(int layerIdOfObject)
    {
        layerObj = layerIdOfObject;

        targetMaterial.SetFloat(ObjectLayerId, layerIdOfObject);
        targetMaterial.SetFloat(CurrentLayerId, GridManager.Instance.gridCurrentLayer);
    }

    public void Update()
    {
        if(GridManager.Instance == null)
        {
            return;
        }
        targetMaterial.SetFloat(CurrentLayerId, GridManager.Instance.gridCurrentLayer);
    }
}

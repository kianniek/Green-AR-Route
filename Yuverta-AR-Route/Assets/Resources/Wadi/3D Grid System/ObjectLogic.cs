using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ObjectLogic : MonoBehaviour
{
    //Values are set on spawn through the gridmanager
    public int objectLayer;
    public int objectIndex;
    public int objectPrefabIndex;
    
    public bool isPlaced;
    public Vector3 previousSnappedPosition;

    Material targetMaterial;

    private void Start()
    {
        targetMaterial = GetComponent<MeshRenderer>().material;
    }

    public void OnDestroy()
    {
        #if ApplicationIsClosing
        return;
        #endif
        //GridManager.Instance.objectSpawner.OnObjectDelete(objectPrefabIndex);
        GridManager.Instance.uiMenu.Add(gameObject);
    }

    public void SetObjectLayerID(int layerIdOfObject)
    {
        objectLayer = layerIdOfObject;

        targetMaterial.SetFloat("_ObjectLayerId", layerIdOfObject);
        targetMaterial.SetFloat("_CurrentLayerId", GridManager.Instance.gridCurrentLayer);
    }

    public void Update()
    {
        if(GridManager.Instance == null)
        {
            return;
        }
        targetMaterial.SetFloat("_CurrentLayerId", GridManager.Instance.gridCurrentLayer);
    }
}

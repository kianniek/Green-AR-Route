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

    public void OnDestroy()
    {
        #if ApplicationIsClosing
        return;
        #endif
        //GridManager.Instance.objectSpawner.OnObjectDelete(objectPrefabIndex);
        GridManager.Instance.uiMenu.Add(gameObject);
    }
}

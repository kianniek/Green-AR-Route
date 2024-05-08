using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ObjectMovement : BaseMovement
{
    private ARRaycastManager arRaycastManager;
    public ObjectLogic objectLogic;
    
    private void Start()
    {
        currentManager = FindObjectOfType<BaseManager>();
        objectLogic = gameObject.GetComponent<ObjectLogic>();
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
    }

    public void MoveObject()
    {
        CheckLayer();
        StartCoroutine(TrackTouchPosition());
    }

    //GridManager only functions
    private void CheckLayer()
    {
        if (!GridManager.Instance || !objectLogic) return;
        if (GridManager.Instance.gridCurrentLayer == objectLogic.layerObj) return;
        
        StartCoroutine(ChangeLayer(GridManager.Instance.gridCurrentLayer));
    }

    private IEnumerator ChangeLayer(int newLayer)
    {
        var currentPos = gameObject.transform.position;
        
        var newLayerIndex = newLayer - objectLogic.layerObj;
        currentPos.y += newLayerIndex * GridManager.Instance.distanceLayers;
        
        while (Mathf.Abs(gameObject.transform.position.y - currentPos.y) > 0.005)
        {
            var whilePos = gameObject.transform.position;
            whilePos.y = Mathf.Lerp(gameObject.transform.position.y, currentPos.y, 0.05f);
            gameObject.transform.position = whilePos;
            yield return new WaitForSeconds(0.01f);
        }
        
        objectLogic.SetObjectLayerID(newLayer);
    }
}
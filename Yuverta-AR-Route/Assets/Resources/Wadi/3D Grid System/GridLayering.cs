using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GridLayering : MonoBehaviour
{
    public Dictionary<GameObject, int> gridSortedLayer = new Dictionary<GameObject, int>();
    
    [SerializeField] private TextMeshProUGUI gridText;
    [SerializeField] private string gridDisplayText;
    
    public int gridCurrentLayer = 0;
    public Vector2Int gridDimensions = new Vector2Int(0, 0);
    
    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        SwipeDetection.Instance.swipePerformed += context => { LayerSwap(context.y); };
        
        GridManager.Instance.gridBuilder.layerParents[0].SetActive(true);
        foreach (var layer in GridManager.Instance.gridBuilder.layerParents.Where(layer => layer != GridManager.Instance.gridBuilder.layerParents[0]))
        {
            layer.SetActive(false);
        }
    }

    private void LayerSwap(float upValue)
    {
        gridCurrentLayer += Mathf.RoundToInt(upValue);
        Debug.Log(gridCurrentLayer);
        gridCurrentLayer = Mathf.Clamp(gridCurrentLayer, 0, gridDimensions.y);
        gridText.text = gridDisplayText + gridCurrentLayer;
        GridManager.Instance.gridBuilder.layerParents[gridCurrentLayer].SetActive(true);
        foreach (var layer in GridManager.Instance.gridBuilder.layerParents.Where(layer => layer != GridManager.Instance.gridBuilder.layerParents[gridCurrentLayer]))
        {
            layer.SetActive(false);
        }
    }
}

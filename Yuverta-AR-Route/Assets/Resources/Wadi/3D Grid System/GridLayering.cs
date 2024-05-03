using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridLayering : MonoBehaviour
{
    public static GridLayering Instance { get; private set; }
    public Dictionary<GameObject, int> gridSortedLayer = new Dictionary<GameObject, int>();
    
    [SerializeField] private TextMeshProUGUI gridText;
    [SerializeField] private string gridDisplayText;
    
    private int gridCurrentLayer = 0;
    public Vector2Int gridDimensions = new Vector2Int(0, 0);

    void Awake()
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
    }
    
    private void Start()
    {
        gridText.text = gridDisplayText + gridCurrentLayer;
    }

    public void LayerSwap(float upValue)
    {
        gridCurrentLayer += Mathf.RoundToInt(upValue);
        Debug.Log(gridCurrentLayer);
        gridCurrentLayer = Mathf.Clamp(gridCurrentLayer, 0, gridDimensions.y);
        gridText.text = gridDisplayText + gridCurrentLayer;
    }
}

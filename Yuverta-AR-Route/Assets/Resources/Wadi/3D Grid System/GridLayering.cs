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
            DontDestroyOnLoad(gameObject);
        }
    }
    
    private void Start()
    {
        gridText.text = gridDisplayText + gridCurrentLayer;
    }

    public void LayerSwap()
    {
        #if UNITY_EDITOR
        if (Input.GetMouseButtonUp(0))
        {
            var touchDirectionMouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).normalized;
        
            if (Mathf.Abs(touchDirectionMouse.x) <= 0.5 && Mathf.Abs(touchDirectionMouse.y) <= 0.5) return;
        
            float directionMouse = Mathf.Abs(touchDirectionMouse.x) > Mathf.Abs(touchDirectionMouse.y) ? touchDirectionMouse.x : touchDirectionMouse.y;
            gridCurrentLayer += Mathf.RoundToInt(directionMouse);
            Debug.Log(gridCurrentLayer);
            gridCurrentLayer = Mathf.Clamp(gridCurrentLayer, 0, gridDimensions.y);
            gridText.text = gridDisplayText + gridCurrentLayer;
            return;
        }
        #endif
        
        if (Input.touchCount == 0 || Input.GetTouch(0).phase != TouchPhase.Ended) return;

        var touchDirection = Input.GetTouch(0).deltaPosition.normalized;
        if (Mathf.Abs(touchDirection.x) <= 0.5 && Mathf.Abs(touchDirection.y) <= 0.5) return;
        
        float direction = Mathf.Abs(touchDirection.x) > Mathf.Abs(touchDirection.y) ? touchDirection.x : touchDirection.y;
        gridCurrentLayer += Mathf.RoundToInt(direction);
        Debug.Log(gridCurrentLayer);
        gridCurrentLayer = Mathf.Clamp(gridCurrentLayer, 0, gridDimensions.y);
        gridText.text = gridDisplayText + gridCurrentLayer;
    }
}

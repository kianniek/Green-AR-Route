using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedFunctionality : MonoBehaviour
{
    public static SharedFunctionality Instance;
    
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
    
    public RaycastHit[] TouchToRay()
    {
        Vector2 touchPosition;
        try
        {
            touchPosition = Input.GetTouch(0).position;
        }
        catch (Exception e)
        {
            touchPosition = Input.mousePosition;
        }
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        var hits = Physics.RaycastAll(ray);
        return hits;
    }
}

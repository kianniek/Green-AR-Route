using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToGrid : MonoBehaviour
{
    private GameObject grid;
    public Vector3 localOffset;
    // Start is called before the first frame update
    void Start()
    {
        grid = FindObjectOfType<GridManager>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (grid == null)
        {
            grid = FindObjectOfType<GridManager>().gameObject;
        }
    }

    private void LateUpdate()
    {
        gameObject.transform.position = grid.transform.position;
        
        gameObject.transform.position += localOffset;
    }
}

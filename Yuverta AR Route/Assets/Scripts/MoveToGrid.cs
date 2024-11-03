using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToGrid : MonoBehaviour
{
    private GameObject grid;
    public bool gridIsCamera = true;

    public Vector3 localOffset;

    // Start is called before the first frame update
    void Start()
    {
        if (gridIsCamera)
        {
            grid = Camera.main.gameObject;
        }else if (!grid)
        {
            grid = FindObjectOfType(typeof(GridManager)) as GameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!grid && gridIsCamera)
        {
            grid = Camera.main.gameObject;
        }else if (!grid)
        {
            grid = FindObjectOfType<GridManager>().gameObject;
        }
    }

    private void LateUpdate()
    {
        if (!grid)
        {
            gameObject.transform.position = Camera.main.gameObject.transform.position;

            gameObject.transform.position += localOffset;
            return;
        }
        
        gameObject.transform.position = grid.transform.position;

        gameObject.transform.position += localOffset;
    }
}
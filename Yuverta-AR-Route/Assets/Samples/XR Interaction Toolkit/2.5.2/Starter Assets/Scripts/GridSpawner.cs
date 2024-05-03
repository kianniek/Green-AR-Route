using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GridSpawner : MonoBehaviour
{
    //script takes in a gameobject (gridManager) with tooltip "only use object that is disabled in the scene" and positions it at its own position

    public GameObject gridManager;

    public UnityEvent OnGridManagerPositioned;
    public void PositionGridManager()
    {
        if (gridManager.activeSelf)
        {
            gridManager.SetActive(false);
        }
        gridManager.transform.position = transform.position;
        gridManager.SetActive(true);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    public GameObject AddBlockButton;
    public GameObject ObjectSelectedButtons;

    private void Start()
    {
        ObjectSelectedButtons.SetActive(false);
    }

    private void FixedUpdate()
    {
        ObjectSelectedButtons.SetActive(BuildingSystem.current.objectToPlace != null);
    }

    public void PlaceBlock()
    {
        BuildingSystem.current.PlaceBlock();
    }

    public void RemoveBlock()
    {
        BuildingSystem.current.RemoveBlock();
    }

    public void SpawnBlock()
    {
        BuildingSystem.current.SpawnBlock();
    }

    public void MoveUp()
    {
        BuildingSystem.current.MoveUp();
    }

    public void MoveDown()
    {
        BuildingSystem.current.MoveDown();
    }

    public void ResetWorld()
    {
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    [SerializeField] private GameObject ObjectSelectedButtons;

    private void Start()
    {
        ObjectSelectedButtons.SetActive(false);
    }

    private void FixedUpdate()
    {
        ObjectSelectedButtons.SetActive(BuidlingSystem.current.objectToPlace != null);
    }

    public void PlaceBlock()
    {
        BuidlingSystem.current.PlaceBlock();
    }

    public void RemoveBlock()
    {
        BuidlingSystem.current.RemoveBlock();
    }

    public void SpawnBlock()
    {
        BuidlingSystem.current.SpawnBlock();
    }

    public void MoveUp()
    {
        BuidlingSystem.current.MoveUp();
    }

    public void MoveDown()
    {
        BuidlingSystem.current.MoveDown();
    }

    public void ResetWorld()
    {
        
    }
}

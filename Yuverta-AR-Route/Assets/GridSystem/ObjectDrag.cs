using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Input = UnityEngine.Windows.Input;

public class ObjectDrag : MonoBehaviour
{
    public bool selected;
    private Vector3 offset;
    private float dragTime;
    public PlaceableObject script;
    public int floorLevel;

    private void Start()
    {
        floorLevel = 0;
        script = gameObject.GetComponent<PlaceableObject>();
    }

    //Debug
    public void OnMouseDown()
    {
        BuidlingSystem.current.objectToPlace = script;
        offset = transform.position - BuidlingSystem.GetMouseWorldPosition();
    }

    //Debug
    public void OnMouseDrag()
    {
        var pos = BuidlingSystem.GetMouseWorldPosition() + offset;
        transform.position = BuidlingSystem.current.SnapCoordinateToGrid(pos, gameObject);
    }
    
    public void OnTouch()
    {
        BuidlingSystem.current.objectToPlace = script;
        offset = transform.position - BuidlingSystem.current.GetTouchWorldPosition();
        dragTime = 0;
    }

    public void OnTouchDrag()
    {
        var pos = BuidlingSystem.current.GetTouchWorldPosition() + offset;
        transform.position = BuidlingSystem.current.SnapCoordinateToGrid(pos, gameObject);
        BuidlingSystem.current.dragging = true;
        dragTime = 0;
    }

    private void FixedUpdate()
    {
        if (!selected) return;
        dragTime += Time.deltaTime;
        BuidlingSystem.current.dragging = dragTime > 1.75f;
    }

    /*private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collided");
        if (!other.gameObject.CompareTag("Collide")) return;
        
        this.transform.position = other.transform.position;
        this.gameObject.GetComponent<PlaceableObject>().Place();
    }*/
}

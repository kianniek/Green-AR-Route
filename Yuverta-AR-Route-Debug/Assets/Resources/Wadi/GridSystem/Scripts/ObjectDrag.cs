using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Input = UnityEngine.Windows.Input;

public class ObjectDrag : MonoBehaviour
{
    [SerializeField] private float maxDragTime;
    public bool selected;
    private Vector3 offset;
    private float dragTime;
    public PlaceableObject script;
    public int floorLevel;
    private MeshRenderer renderer;

    private void Start()
    {
        renderer = gameObject.GetComponent<MeshRenderer>();
        floorLevel = 0;
        script = gameObject.GetComponent<PlaceableObject>();
    }

    public void UpdateLayer(int newLayer)
    {
        floorLevel = newLayer;
    }

    public void ChangeAlpha(float newValue)
    {
        Debug.Log(newValue);
        
        var color = renderer.materials[0].color;
        color.a = newValue;
        
        if (newValue < BuildingSystem.current.loweredAlphaLayer || newValue > 1) return;
        
        renderer.materials[0].color = color;
    }

    //Debug
    public void OnMouseDown()
    {
        BuildingSystem.current.objectToPlace = script;
        offset = transform.position - BuildingSystem.GetMouseWorldPosition();
    }

    //Debug
    public void OnMouseDrag()
    {
        var pos = BuildingSystem.GetMouseWorldPosition() + offset;
        transform.position = BuildingSystem.current.SnapCoordinateToGrid(pos, gameObject);
    }
    
    public void OnTouch()
    {
        BuildingSystem.current.objectToPlace = script;
        script.canvas.gameObject.SetActive(true);
        offset = transform.position - BuildingSystem.current.GetTouchWorldPosition();
        dragTime = 0;
    }

    public void OnTouchDrag()
    {
        Debug.Log("MFER");
        var pos = BuildingSystem.current.GetTouchWorldPosition() + offset;
        transform.position = BuildingSystem.current.SnapCoordinateToGrid(pos, gameObject);
        if (selected) BuildingSystem.current.dragging = true;
        dragTime = 0;
    }

    public void UnSelect()
    {
        selected = false;
        BuildingSystem.current.dragging = false;
    }

    private void FixedUpdate()
    {
        if (!selected) return;
        dragTime += Time.deltaTime;
        BuildingSystem.current.dragging = dragTime < maxDragTime;
        script.canvas.gameObject.transform.LookAt(Camera.main!.transform);
    }

    /*private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collided");
        if (!other.gameObject.CompareTag("Collide")) return;
        
        this.transform.position = other.transform.position;
        this.gameObject.GetComponent<PlaceableObject>().Place();
    }*/
}

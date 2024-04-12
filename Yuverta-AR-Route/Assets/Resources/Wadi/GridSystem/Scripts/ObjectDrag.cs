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

    private void Start()
    {
        floorLevel = 0;
        script = gameObject.GetComponent<PlaceableObject>();
    }

    public void UpdateLayer(int newLayer, Material newLayerMaterial)
    {
        floorLevel = newLayer;
        var materials = gameObject.GetComponent<MeshRenderer>().materials;
        if (materials.Contains(newLayerMaterial)) return;
        materials[1] = newLayerMaterial;
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
        script.canvas.gameObject.SetActive(true);
        offset = transform.position - BuidlingSystem.current.GetTouchWorldPosition();
        dragTime = 0;
    }

    public void OnTouchDrag()
    {
        Debug.Log("MFER");
        var pos = BuidlingSystem.current.GetTouchWorldPosition() + offset;
        transform.position = BuidlingSystem.current.SnapCoordinateToGrid(pos, gameObject);
        if (selected) BuidlingSystem.current.dragging = true;
        dragTime = 0;
    }

    public void UnSelect()
    {
        selected = false;
        BuidlingSystem.current.dragging = false;
    }

    private void FixedUpdate()
    {
        if (!selected) return;
        dragTime += Time.deltaTime;
        BuidlingSystem.current.dragging = dragTime < maxDragTime;
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

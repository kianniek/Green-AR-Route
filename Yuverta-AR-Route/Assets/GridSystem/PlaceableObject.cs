using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    public bool Placed { get; private set; }
    public Vector3Int Size { get; private set; }
    private Vector3[] Vertices;
    public Material material;
    public int index;
    
    private void Start()
    {
        index = gameObject.GetHashCode();
        material = gameObject.GetComponent<MeshRenderer>().material;
        GetColliderVertexPositionsLocal();
        CalculateSizeInCells();
    }
    
    private void GetColliderVertexPositionsLocal()
    {
        var b = gameObject.GetComponent<BoxCollider>();
        Vertices = new Vector3[4];
        Vertices[0] = b.center + new Vector3(-b.size.x, -b.size.y, -b.size.z) * 0.5f;
        Vertices[1] = b.center + new Vector3(b.size.x, -b.size.y, -b.size.z) * 0.5f;
        Vertices[2] = b.center + new Vector3(b.size.x, -b.size.y, b.size.z) * 0.5f;
        Vertices[3] = b.center + new Vector3(-b.size.x, -b.size.y, b.size.z) * 0.5f;
    }

    private void CalculateSizeInCells()
    {
        var vertices = new Vector3Int[Vertices.Length];

        for (var i = 0; i < vertices.Length; i++)
        {
            var worldPos = transform.TransformPoint(Vertices[i]);
            vertices[i] = BuidlingSystem.current.gridLayout.WorldToCell(worldPos);
        }

        Size = new Vector3Int(Math.Abs((vertices[0] - vertices[1]).x), Math.Abs((vertices[0] - vertices[3]).x));

    }

    public Vector3 GetStartPosition()
    {
        return transform.TransformPoint(Vertices[0]);
    }
    
    public void Rotate()
    {
        transform.Rotate(new Vector3(0, 90, 0));
        Size = new Vector3Int(Size.y, Size.x, 1);

        var vertices = new Vector3[Vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vertices[(i + 1) % Vertices.Length];
        }

        Vertices = vertices;
    }

    public virtual void Place()
    {
        /*var drag = gameObject.GetComponent<ObjectDrag>();
        Destroy(drag);*/

        if (!BuidlingSystem.current.placedObjects.Contains(this.gameObject)) BuidlingSystem.current.placedObjects.Add(this.gameObject);

        var color = material.color;
        color.a = 0.5f;
        material.color = color;
        
        Placed = true;
    }
}

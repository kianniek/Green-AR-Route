using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlaceableObject : MonoBehaviour
{
    public GameObject canvas;
    public Button confirmPlacement;
    private Image confirmImage;
    public Button removeObject;
    private ObjectDrag script;
    
    public bool Placed { get; private set; }
    public Vector3Int Size { get; private set; }
    private Vector3[] Vertices;
    public Material material;
    public int index;
    public int uiIndex;
    public bool canBePlaced;
    
    private void Start()
    {
        script = gameObject.GetComponent<ObjectDrag>();
        material = gameObject.GetComponent<MeshRenderer>().material;
        confirmImage = confirmPlacement.GetComponent<Image>();
        GetColliderVertexPositionsLocal();
        CalculateSizeInCells();
    }

    private void FixedUpdate()
    {
        //buttonMaterial.color = Color.white;
        if (!canBePlaced)
        {
            confirmImage.color = Color.gray;
            confirmPlacement.interactable = false;
            return;
        }
        confirmImage.color = Color.white;
        confirmPlacement.interactable = true;
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
            vertices[i] = BuildingSystem.current.gridLayout.WorldToCell(worldPos);
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

    public void OnClickConfirm()
    {
        BuildingSystem.current.PlaceBlock();
    }

    public void OnClickRemove()
    {
        BuildingSystem.current.RemoveBlock();
    }

    public virtual void Place()
    {
        if (!BuildingSystem.current.placedObjects.Contains(this.gameObject)) BuildingSystem.current.placedObjects.Add(this.gameObject);

        var color = material.color;
        color.a = 0.5f;
        material.color = color;
        
        Placed = true;
    }
}

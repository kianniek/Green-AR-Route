using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events.GameEvents.Typed;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIMenuLogic : MonoBehaviour
{
    [Header("General")] [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject swipeCanvas;

    [Header("Scroll Area")] [SerializeField]
    private Transform uiObjectParentTransform;

    [SerializeField] private GameObject uiButtonPrefab;
    [SerializeField] private GameObject gridmanagerPrefab;

    [SerializeField] private Button startAnimationsButton;
    [SerializeField] private Button clearGridButton;

    private Dictionary<GameObject, DragDropHandler> uiObjects = new();

    [Header("Moving Objects To Scroll Area")] [SerializeField]
    private float speedModifier;

    [SerializeField] private UnityEvent onWadiCorrect;
    [SerializeField] private UnityEvent onWadiIncorrect;

    private Dictionary<string, Sprite> UIObjectImages = new();
    
    public Canvas Canvas => canvas;

    private void Start()
    {
        startAnimationsButton.gameObject.SetActive(false);
        clearGridButton.gameObject.SetActive(false);

        var objectsToSpawn = gridmanagerPrefab.GetComponent<GridManager>().ObjsToSpawn;
        foreach (var obj in objectsToSpawn.keys)
        {
            var dragDropHandler = obj.GetComponent<DragDropHandler>();

            var objName = dragDropHandler.itemPrefab.name;
            var objSprite = dragDropHandler.dragSprite;

            if (objSprite == null)
            {
                objSprite = null;
            }

            UIObjectImages.Add(objName, objSprite);
            uiObjects.Add(obj, dragDropHandler);
        }

        EnableCanvas(false);
    }

    public bool OnObjectDelete(GameObject go)
    {
        if (go == null)
            return false;

        //find object in dictionary
        var obj = uiObjects.FirstOrDefault(x => x.Value.itemPrefab.GetComponent<ObjectLogic>().objectGridLocation == go.GetComponent<ObjectLogic>().objectGridLocation).Key;
        //if the object is found, enable the object
        if (!obj)
        {
            Debug.LogWarning("Object not found in dictionary");
            return false;
        }
        Debug.Log(obj);
        
        obj.SetActive(true);
        
        return true;
    }
    
    public void CheckAnimation()
    {
        var gridManager = FindObjectOfType<GridManager>();

        if (gridManager == null)
        {
            startAnimationsButton.gameObject.SetActive(false);
            return;
        }
        
        startAnimationsButton.gameObject.SetActive(true);

        var correct = gridManager.CheckPosition(out var wrongPlaces);
        
        
        gridManager.GridBuilder.MoveGridPointsToConvergedPosition();

        if (correct)
        {
            onWadiCorrect.Invoke();
        }
        else
        {
            onWadiIncorrect.Invoke();
        }
    }

    /// <summary>
    /// Enables the canvas. used for the start of the game when grid is spawned
    /// </summary>
    public void EnableCanvas(bool enable)
    {
        swipeCanvas.gameObject.SetActive(enable);
    }
}
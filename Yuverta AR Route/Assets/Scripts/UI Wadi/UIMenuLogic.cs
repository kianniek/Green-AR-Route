using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events.GameEvents.Typed;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TouchPhase = UnityEngine.TouchPhase;

public class UIMenuLogic : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Header("General")] [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject swipeCanvas;
    
    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    [SerializeField] private RectTransform scrollView;
    [SerializeField] private ScrollRect scrollRect;
    private Vector2 previousTouchPosition;

    [Header("Scroll Area")] [SerializeField]
    private Transform uiObjectParentTransform;

    [SerializeField] private GameObject uiButtonPrefab;

    [SerializeField] private Button startAnimationsButton;
    [SerializeField] private Button clearGridButton;

    private Dictionary<GameObject, DragDropHandler> uiObjects = new();

    [Header("Moving Objects To Scroll Area")] [SerializeField]
    private float speedModifier;

    [SerializeField] private UnityEvent onWadiCorrect;
    [SerializeField] private UnityEvent onWadiIncorrect;

    private GridManager _gridManager;
    
    public Canvas Canvas => canvas;

    private void Start()
    {
        startAnimationsButton.gameObject.SetActive(false);
        clearGridButton.gameObject.SetActive(false);

        if (scrollView)
        {
            scrollRect = scrollView.GetComponent<ScrollRect>();
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

        var correct = gridManager.CheckPosition(out _);
        
        
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

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = touch.position
            };

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    dragStartPos = touch.position;
                    previousTouchPosition = touch.position;
                    break;

                case TouchPhase.Moved:
                    OnDrag(eventData);
                    previousTouchPosition = touch.position;
                    break;

                case TouchPhase.Ended:
                    dragEndPos = touch.position;
                    previousTouchPosition = Vector2.zero;
                    break;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging");
        var touchDelta = eventData.position - previousTouchPosition;

        var distanceX = Mathf.Abs(dragStartPos.x - eventData.position.x);
        if (distanceX > scrollView.rect.width / 2)
        {
            return;
        }
        Debug.Log(touchDelta);
        
        touchDelta *= scrollRect.scrollSensitivity;

        switch(scrollRect.horizontal && scrollRect.vertical)
        {
            case true:
                scrollRect.content.anchoredPosition += new Vector2(touchDelta.x, touchDelta.y);
                break;
            case false:
                if (scrollRect.horizontal)
                {
                    scrollRect.content.anchoredPosition += new Vector2(touchDelta.x, 0);
                }
                else if (scrollRect.vertical)
                {
                    scrollRect.content.anchoredPosition += new Vector2(0, touchDelta.y);
                }
                break;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Implement any additional behavior you want when the drag ends
    }
    
    /// <summary>
    /// Enables the canvas. used for the start of the game when grid is spawned
    /// </summary>
    public void EnableCanvas(bool enable)
    {
        swipeCanvas.gameObject.SetActive(enable);
    }
    
    public void FillUIObjects(GridManager gridManager)
    {
        _gridManager = gridManager;
        var objectsToSpawn = _gridManager.ObjsToSpawn;
        foreach (var obj in objectsToSpawn.keys)
        {
            var dragDropHandler = obj.GetComponent<DragDropHandler>();
            uiObjects.Add(obj, dragDropHandler);
        }
    }
}
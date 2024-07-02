using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events.GameEvents.Typed;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
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
    private Dictionary<GameObject, DragDropHandler> _UIObjectDictionary = new();
    
    public Dictionary<GameObject, DragDropHandler> UIObjectDictionary => _UIObjectDictionary;

    [Header("Moving Objects To Scroll Area")] [SerializeField]
    private float speedModifier;

    [SerializeField] private UnityEvent onWadiCorrect;
    [SerializeField] private UnityEvent onWadiIncorrect;

    private GridManager _gridManager;
    
    public Canvas Canvas => canvas;

    private void Start()
    {
        startAnimationsButton.gameObject.SetActive(false);

        if (!scrollView)
            scrollRect = scrollView.GetComponentInChildren<ScrollRect>();

        EnableCanvas(false);
    }

    public bool OnObjectDelete(GameObject go)
    {
        if (go == null)
            return false;

        //find object in dictionary
        var obj = _UIObjectDictionary.ContainsKey(go) ? go : null;
        
        //if the object is found, enable the object
        if (!obj)
        {
            Debug.LogWarning("Object not found in dictionary");
            return false;
        }
        Debug.Log(obj);
        
        var uiObject = _UIObjectDictionary[obj].gameObject;
        
        //Remove object from dictionary
        _UIObjectDictionary.Remove(obj);
        
        //move object to scroll area
        uiObject.SetActive(true);
        
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
        var touchDelta = eventData.position - previousTouchPosition;

        var distanceX = Mathf.Abs(dragStartPos.x - eventData.position.x);
        if (distanceX > scrollView.rect.width / 2)
        {
            return;
        }
        
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
        
        startAnimationsButton.gameObject.SetActive(enable);
    }
}
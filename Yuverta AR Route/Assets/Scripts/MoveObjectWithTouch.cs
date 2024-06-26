using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MoveObjectWithTouch : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    private ObjectLogic _objectLogic;
    private Camera arCamera;
    public InputActionReference touchInputAction;
    private UIMenuLogic _uiMenuLogic;
    public Sprite dragSprite; // The sprite to display while dragging

    private Canvas _canvas;
    private GameObject _dragObject;
    private MeshRenderer _ObjectVisuals;

    private bool _startDrag;

    private void Awake()
    {
        // Find the AR Camera
        arCamera = Camera.main;

        touchInputAction.action.Enable();

        _ObjectVisuals = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        _uiMenuLogic = FindObjectOfType<UIMenuLogic>();
        _objectLogic = GetComponent<ObjectLogic>();

        _canvas = _uiMenuLogic ? _uiMenuLogic.Canvas : FindObjectOfType<Canvas>();
    }

    private void OnEnable()
    {
        touchInputAction.action.performed += OnTouchPerformed;
        touchInputAction.action.canceled += OnEndDrag;
    }

    private void OnDisable()
    {
        touchInputAction.action.performed -= OnTouchPerformed;
        touchInputAction.action.canceled += OnEndDrag;
    }

    private void OnPointerDown(Vector2 touchPosition, List<ARRaycastHit> hits)
    {
        if (arCamera == null)
            return;

        var hitPose = hits[0].pose;

        CreateDragImage(touchPosition);
        _startDrag = true;
    }

    private void OnDrag(Vector2 touchPosition)
    {
        if (_dragObject == null)
            return;

        _dragObject.transform.position = touchPosition;
    }

    private void OnEndDrag(InputAction.CallbackContext context)
    {
        if (_dragObject != null)
        {
            Destroy(_dragObject);

            // Show the selected object
            _ObjectVisuals.enabled = true;
        }
        
        _startDrag = false;
        
        var touchPosition = context.ReadValue<Vector2>();
        if (arCamera == null)
            return;

        var ray = arCamera.ScreenPointToRay(touchPosition);
        var hits = new List<ARRaycastHit>();

        if (!raycastManager.Raycast(ray, hits, TrackableType.Planes))
            return;

        if (hits.Count <= 0) return;
        
        transform.position = hits[0].pose.position;
        _objectLogic.SnapToNewGridPoint();
    }

    private void OnTouchPerformed(InputAction.CallbackContext context)
    {
        var touchPosition = context.ReadValue<Vector2>();
        if (arCamera == null)
            return;

        var ray = arCamera.ScreenPointToRay(touchPosition);
        var hits = new List<ARRaycastHit>();

        if (!raycastManager.Raycast(ray, hits, TrackableType.Planes))
            return;

        if (!_startDrag)
            OnPointerDown(touchPosition, hits);

        OnDrag(touchPosition);
    }

    private void CreateDragImage(Vector2 touchPosition)
    {
        _dragObject = new GameObject("DragImage");
        _dragObject.transform.SetParent(_canvas.transform, false); // Make it a child of the _canvas
        _dragObject.transform.position = touchPosition;

        var image = _dragObject.AddComponent<Image>();
        image.sprite = dragSprite;
        image.raycastTarget = false; // Make sure it does not block any events

        var rectTransform = image.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 100); // Set size, adjust as needed

        // Hide the selected object
        _ObjectVisuals.enabled = false;

        // Move this object to the end of the children
        _dragObject.transform.SetAsLastSibling();
    }
}
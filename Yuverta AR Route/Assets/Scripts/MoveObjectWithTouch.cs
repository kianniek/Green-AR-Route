using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MoveObjectWithTouch : MonoBehaviour
{
    [SerializeField] private string tagToRaycast = "GridPoint";
    public ARRaycastManager raycastManager;
    private ObjectLogic _objectLogic;
    private Camera arCamera;
    private UIMenuLogic _uiMenuLogic;
    public Sprite dragSprite; // The sprite to display while dragging
    private LerpedObjectMovement _lerpedObjectMovement;

    private Canvas _canvas;
    private GameObject _dragObject;
    private MeshRenderer _ObjectVisuals;

    private bool _startDrag;
    private bool _canDrag;

    private Vector2 _touchStartPosition;
    public float dragSensitive = 1;

    public UnityEvent onDragStart = new();
    public UnityEvent onDrag = new();
    public UnityEvent onDragEnd = new();

    private bool _isInFocus;

    private void Awake()
    {
        // Find the AR Camera
        arCamera = Camera.main;

        _ObjectVisuals = GetComponent<MeshRenderer>();

        _lerpedObjectMovement = GetComponent<LerpedObjectMovement>();
    }

    private void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        _uiMenuLogic = FindObjectOfType<UIMenuLogic>();
        _objectLogic = GetComponent<ObjectLogic>();

        _canvas = _uiMenuLogic ? _uiMenuLogic.Canvas : FindObjectOfType<Canvas>();
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchPerformed(touch.position);
                    break;
                case TouchPhase.Moved:
                    OnDrag(touch.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnEndDrag();
                    break;
            }
        }
    }

    private void OnPointerDown(Vector2 touchPosition)
    {
        if (arCamera == null)
            return;

        Debug.Log($"Pointer down at position: {touchPosition}", gameObject);
        CreateDragImage(touchPosition);
        onDragStart.Invoke();

        _lerpedObjectMovement.enabled = false;

        _startDrag = true;
    }

    private void OnDrag(Vector2 touchPosition)
    {
        if (!_canDrag || !_isInFocus)
            return;

        Debug.Log($"Dragging at position: {touchPosition}", gameObject);

        var distance = Vector2.Distance(touchPosition, _touchStartPosition);

        //check if the touch is a drag
        if (distance < dragSensitive && !_startDrag)
            return;

        if (!_startDrag)
            OnPointerDown(touchPosition);

        if (_dragObject == null)
            return;

        _dragObject.transform.position = touchPosition;

        if (arCamera == null)
            return;

        var ray = arCamera.ScreenPointToRay(touchPosition);
        Debug.Log($"Raycast from position: {touchPosition}", gameObject);

        if (Physics.Raycast(ray, out var hit))
        {
            Debug.Log($"Raycast hit: {hit.collider.gameObject.name}", gameObject);

            if (hit.collider.gameObject.CompareTag(tagToRaycast))
            {
                Debug.Log($"Raycast hit object with tag: {tagToRaycast} at position: {hit.point}", gameObject);
                transform.position = hit.point;
            }
        }

        _objectLogic.SnapToNewGridPoint();
        onDrag.Invoke();
    }

    private void OnEndDrag()
    {
        if (_dragObject != null)
        {
            Destroy(_dragObject);
            Debug.Log("Drag ended, drag object destroyed", gameObject);

            // Show the selected object
            _ObjectVisuals.enabled = true;
        }

        _startDrag = false;
        _canDrag = false;

        _isInFocus = false;

        _lerpedObjectMovement.enabled = true;
        onDragEnd.Invoke();
    }

    private void OnTouchPerformed(Vector2 touchPosition)
    {
        Debug.Log($"Touch performed at position: {touchPosition}", gameObject);

        if (arCamera == null)
            return;

        var ray = arCamera.ScreenPointToRay(touchPosition);
        Debug.Log($"Raycast from touch performed at position: {touchPosition}", gameObject);

        if (!Physics.Raycast(ray, out var hit) && !_startDrag)
            return;

        if (hit.collider.gameObject != gameObject)
        {
            _isInFocus = false;
            return;

        }

        _isInFocus = true;

        _canDrag = true;
        Debug.Log("Object can be dragged", gameObject);

        _touchStartPosition = touchPosition;
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
        Debug.Log("Drag image created and object visuals hidden", gameObject);

        // Move this object to the end of the children
        _dragObject.transform.SetAsLastSibling();
    }
}

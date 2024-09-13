using System;
using System.Collections.Generic;
using Kamgam.UGUIWorldImage;
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
    public RenderTexture dragSprite; // The sprite to display while dragging
    private WorldImage _dragSpriteWI;
    private LerpedObjectMovement _lerpedObjectMovement;

    private Canvas _canvas;
    private GameObject _dragObject;
    private List<MeshRenderer> _ObjectVisuals;

    private bool _startDrag;
    private bool _canDrag;

    private Vector2 _touchStartPosition;
    public float dragSensitive = 1;
    [SerializeField] private float dragImageSize = 450f;

    public UnityEvent onDragStart = new();
    public UnityEvent onDrag = new();
    public UnityEvent onDragEnd = new();

    private bool _isInFocus;

    private void Awake()
    {
        // Find the AR Camera
        arCamera = Camera.main;
        _ObjectVisuals = new ();
        _ObjectVisuals.Add(GetComponent<MeshRenderer>());
        _ObjectVisuals.AddRange(GetComponentsInChildren<MeshRenderer>());

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
        if (!arCamera)
            return;

        CreateDragImage(touchPosition);
        onDragStart.Invoke();

        _lerpedObjectMovement.enabled = false;

        _startDrag = true;
    }

    private void OnDrag(Vector2 touchPosition)
    {
        if (!_canDrag || !_isInFocus)
            return;

        var distance = Vector2.Distance(touchPosition, _touchStartPosition);

        //check if the touch is a drag
        if (distance < dragSensitive && !_startDrag)
            return;

        if (!_startDrag)
            OnPointerDown(touchPosition);

        if (!_dragObject)
            return;

        _dragObject.transform.position = touchPosition;

        if (!arCamera)
            return;

        var ray = arCamera.ScreenPointToRay(touchPosition);

        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.collider.gameObject.CompareTag(tagToRaycast))
            {
                transform.position = hit.point;
            }
        }

        _objectLogic.SnapToNewGridPoint();
        onDrag.Invoke();
    }

    private void OnEndDrag()
    {
        if (_dragObject)
        {
            if (_dragSpriteWI)
            {
                _dragSpriteWI.CameraFollowTransform = true;
            }
            Destroy(_dragObject);

            // Show the selected object
            foreach (var mr in _ObjectVisuals)
            {
                mr.enabled = true;
            }
        }
        
        _objectLogic.LockObjectIfRightlyPlaced();

        _startDrag = false;
        _canDrag = false;

        _isInFocus = false;

        _lerpedObjectMovement.enabled = true;
        onDragEnd.Invoke();
    }

    private void OnTouchPerformed(Vector2 touchPosition)
    {
        if (arCamera == null)
            return;

        var ray = arCamera.ScreenPointToRay(touchPosition);

        if (!Physics.Raycast(ray, out var hit) && !_startDrag)
            return;

        if (hit.collider.gameObject != gameObject)
        {
            _isInFocus = false;
            return;
        }

        _isInFocus = true;

        _canDrag = true;

        _touchStartPosition = touchPosition;
    }

    private void CreateDragImage(Vector2 touchPosition)
    {
        if(_dragObject)
            Destroy(_dragObject);
        
        _dragObject = new GameObject("DragImage");
        _dragObject.transform.SetParent(_canvas.transform, false); // Make it a child of the _canvas
        _dragObject.transform.position = touchPosition;

        var image = _dragObject.AddComponent<RawImage>();

        if (_dragSpriteWI)
        {
            image.texture = _dragSpriteWI.RenderTextureOverride; // Set the sprite
            Debug.Log(_dragSpriteWI.RenderTextureOverride.name);
            _dragSpriteWI.CameraFollowTransform = true;
        }
        else
        {
            image.texture = dragSprite; // Set the sprite
        }

        Debug.Log(image.texture.name);

        image.raycastTarget = false; // Make sure it does not block any events

        // Set the size of the drag image
        var rectTransform = image.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(dragImageSize, dragImageSize); // Set size, adjust as needed

        // Hide the selected object
        foreach (var mr in _ObjectVisuals)
        {
            mr.enabled = false;
        }

        // Move this object to the end of the children
        _dragObject.transform.SetAsLastSibling();
    }

    public void SetDragSprite(WorldImage worldImage)
    {
        _dragSpriteWI = worldImage;
    }
}
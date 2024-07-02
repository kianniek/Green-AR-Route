using System.Collections.Generic;
using Kamgam.UGUIWorldImage;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(ObjectSpawner))]
public class DragDropHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private string tagToRaycast;
    [SerializeField] private ARRaycastManager m_RaycastManager;
    public GameObject itemPrefab; // The actual item to place on the grid
    [SerializeField] private GameObject visuals;
    [SerializeField] private float holdTimeThreshold = 0.5f; // Time to hold before it counts as a drag

    private GameObject dragObject; // The temporary drag object (UI representation)
    private Canvas _canvas;
    private bool isDragging;
    private bool isPointerOverUI;
    private ObjectSpawner _objectSpawner;
    private GridManager _gridManager;
    private Camera _mainCamera;
    private bool isDraggableObjectSelected;
    private WorldImage _dragSprite; // The sprite to display while dragging
    private UIMenuLogic _uiMenuLogic;

    private Button _button;

    private void Start()
    {
        _canvas = FindObjectOfType<Canvas>(); // Find the _canvas in the scene
        _gridManager = FindObjectOfType<GridManager>();
        _objectSpawner = GetComponent<ObjectSpawner>();
        _button = GetComponent<Button>();
        _mainCamera = Camera.main;

        _objectSpawner.ObjectPrefabs.Clear();
        _objectSpawner.ObjectPrefabs.Add(itemPrefab);

        _dragSprite = GetComponentInChildren<WorldImage>();

        _uiMenuLogic = GetComponentInParent<UIMenuLogic>();

        if (!_canvas)
        {
            Debug.LogError("No Canvas found in the scene.");
        }
    }

    private void Update()
    {
        // If ARRaycastManager is not set, find it
        if (!m_RaycastManager)
        {
            m_RaycastManager = FindObjectOfType<ARRaycastManager>();
        }

        // Handle touch input and convert to pointer events
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
                    OnPointerDown(eventData);
                    break;
                case TouchPhase.Moved:
                    OnDrag(eventData);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnPointerUp(eventData);
                    break;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject(eventData.pointerId);

        isDraggableObjectSelected = EventSystem.current.currentSelectedGameObject == gameObject;

        if (!isPointerOverUI && isDraggableObjectSelected)
        {
            isDragging = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject(eventData.pointerId);

        if (dragObject == null && isDragging)
        {
            CreateDragImage(eventData);
        }

        if (dragObject != null && isDragging)
        {
            dragObject.transform.position = eventData.position; // Follow the mouse or finger
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject(eventData.pointerId);
        if (isDragging)
        {
            SpawnObject(eventData.position);
            Destroy(dragObject);
        }

        dragObject = null; // Clear the reference
        isDragging = false; // Reset dragging state
        isDraggableObjectSelected = false; // Reset draggable object selection state
    }

    private void CreateDragImage(PointerEventData eventData)
    {
        dragObject = new GameObject("DragImage");
        dragObject.transform.SetParent(_canvas.transform, false); // Make it a child of the _canvas
        dragObject.transform.position = eventData.position;

        var image = dragObject.AddComponent<RawImage>();

        image.texture = _dragSprite.RenderTexture; // Set the sprite
        image.raycastTarget = false; // Make sure it does not block any events

        var rectTransform = image.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 100); // Set size, adjust as needed

        // Move this object to the end of the children
        dragObject.transform.SetAsLastSibling();
    }

    private void SpawnObject(Vector2 screenPosition)
    {
        var RayFromScreen = _mainCamera.ScreenPointToRay(screenPosition);

        // Raycast to find the position to spawn the object
        if (Physics.Raycast(RayFromScreen, out var hit, 100f))
        {
            if (!hit.transform.CompareTag(tagToRaycast))
                return;

            var spawnedObject = _objectSpawner.TrySpawnObject(hit.point, hit.normal, out var spawnObject);

            if (spawnedObject)
            {
                _uiMenuLogic.UIObjectDictionary.Add(spawnObject, this);
                //debug UIObjectDictionary
                foreach (var item in _uiMenuLogic.UIObjectDictionary)
                {
                    Debug.Log(item.Key.name + " | " + item.Value.name);
                }
            }

            gameObject.SetActive(!spawnedObject);
            return;
        }
    }
}
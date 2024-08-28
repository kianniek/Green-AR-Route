using System.Collections.Generic;
using Kamgam.UGUIWorldImage;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(ObjectSpawner))]
public class DragDropHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("AR Settings")] [Tooltip("Tag used to identify objects for raycasting.")] [SerializeField]
    private string tagToRaycast;

    [Tooltip("ARRaycastManager for handling AR raycasts.")] [SerializeField]
    private ARRaycastManager m_RaycastManager;

    [Header("Item Settings")] [Tooltip("Prefab of the item to place on the grid.")]
    public GameObject itemPrefab; // The actual item to place on the grid

    [Tooltip("Visuals of the item.")] [SerializeField]
    private GameObject visuals;
    
    [FormerlySerializedAs("spawnParticles")] [Tooltip("Particles of the item.")] [SerializeField]
    private GameObject spawnParticlesPrefab;

    [Tooltip("Time to hold before it counts as a drag.")] [SerializeField]
    private float holdTimeThreshold = 0.5f; // Time to hold before it counts as a drag

    [SerializeField] private float dragImageSize;
    private GameObject dragObject; // The temporary drag object (UI representation)
    private Canvas _canvas;
    private bool isDragging;
    private bool isDraggingUI;
    private bool isPointerOverUI;
    private ObjectSpawner _objectSpawner;
    private GridManager _gridManager;
    private Camera _mainCamera;
    private bool isDraggableObjectSelected;
    private WorldImage _dragSprite; // The sprite to display while dragging
    private UIMenuLogic _uiMenuLogic;
    private ScrollRect _scroll;
    private Vector2 lastMousePosition;
    private Button _button;
    private Vector2 _scrollDelta;

    private void Start()
    {
        // Find necessary components in the scene
        _canvas = FindObjectOfType<Canvas>();
        _gridManager = FindObjectOfType<GridManager>();
        _objectSpawner = GetComponent<ObjectSpawner>();
        _button = GetComponent<Button>();
        _mainCamera = Camera.main;
        _scroll = GetComponentInParent<ScrollRect>();

        // Initialize ObjectSpawner with the item prefab
        _objectSpawner.ObjectPrefabs.Clear();
        _objectSpawner.ObjectPrefabs.Add(itemPrefab);

        // Find the drag sprite in children
        _dragSprite = GetComponentInChildren<WorldImage>();

        // Find the UI menu logic in parents
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
            var touch = Input.GetTouch(0);

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

        if (_scrollDelta != Vector2.zero && !isDraggingUI)
        {
            _scrollDelta = Vector2.Lerp(_scrollDelta, Vector2.zero, _scroll.decelerationRate);
            
            //update scroll position
            ScrollContent(_scrollDelta);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Check if the pointer is over a UI element
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject(eventData.pointerId);
        isDraggableObjectSelected = EventSystem.current.currentSelectedGameObject == gameObject;

        if (isPointerOverUI)
        {
            lastMousePosition = eventData.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Check if the pointer is over a UI element
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject(eventData.pointerId);

        // Determine if dragging an AR object or UI element
        isDragging = !isPointerOverUI && isDraggableObjectSelected;
        isDraggingUI = isPointerOverUI;

        // Create drag image if dragging an AR object
        if (dragObject == null && isDragging)
        {
            CreateDragImage(eventData);
        }

        // Update the drag object position
        if (dragObject != null && isDragging)
        {
            dragObject.transform.position = eventData.position; // Follow the mouse or finger
        }
        else
        {
            DestroyDragImage();
        }

        // Handle scrolling logic if dragging a UI element
        if (!isDraggingUI || _scroll == null)
            return;

        // Calculate the delta movement
        _scrollDelta = eventData.position - lastMousePosition;

        // Determine if the drag is more horizontal or vertical
        if (Mathf.Abs(_scrollDelta.x) > Mathf.Abs(_scrollDelta.y))
        {
            // Horizontal drag is greater, so do not scroll
            return;
        }

        // Adjust the delta by the scroll sensitivity
        _scrollDelta *= 0.1f * _scroll.scrollSensitivity;

        // Convert delta to ScrollRect's local space and adjust by scroll axis
        ScrollContent(_scrollDelta);

        // Update last mouse position
        lastMousePosition = eventData.position;

        // Call ScrollRect drag events
        _scroll.OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Check if the pointer is over a UI element
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject(eventData.pointerId);

        // Handle releasing the drag object
        if (dragObject != null || isDragging)
        {
            SpawnObject(eventData.position);
            DestroyDragImage();
        }

        // Clear references and reset states
        dragObject = null;
        isDragging = false;
        isDraggingUI = false;
        isDraggableObjectSelected = false;
    }

    private void CreateDragImage(PointerEventData eventData)
    {
        // Create a new game object for the drag image
        dragObject = new GameObject("DragImage");
        dragObject.transform.SetParent(_canvas.transform, false); // Make it a child of the _canvas
        dragObject.transform.position = eventData.position;

        // Add and configure the RawImage component
        var image = dragObject.AddComponent<RawImage>();
        image.texture = _dragSprite.RenderTexture; // Set the sprite
        image.raycastTarget = false; // Make sure it does not block any events

        // Set the size of the drag image
        var rectTransform = image.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(dragImageSize, dragImageSize); // Set size, adjust as needed

        // Move this object to the end of the children
        dragObject.transform.SetAsLastSibling();
    }

    private void DestroyDragImage()
    {
        if (dragObject != null)
        {
            Destroy(dragObject);
        }
    }

    private void ScrollContent(Vector2 delta)
    {
        // Convert delta to ScrollRect's local space and adjust by scroll axis
        var localDelta = _scroll.content.InverseTransformVector(delta);
        switch (_scroll.horizontal)
        {
            case true when !_scroll.vertical:
                localDelta = new Vector2(localDelta.x, 0);
                break;
            case false when _scroll.vertical:
                localDelta = new Vector2(0, localDelta.y);
                break;
        }

        // Adjust scroll position
        _scroll.content.anchoredPosition += (Vector2)localDelta;
    }

    private void SpawnObject(Vector2 screenPosition)
    {
        var RayFromScreen = _mainCamera.ScreenPointToRay(screenPosition);

        // Raycast to find the position to spawn the object
        if (Physics.Raycast(RayFromScreen, out var hit, 100f))
        {
            if (!hit.transform.CompareTag(tagToRaycast))
                return;

            // Try to spawn the object at the hit point
            var spawnedObject = _objectSpawner.TrySpawnObject(hit.point, hit.normal, out var spawnObject, out var spawnPosition);

            if (spawnedObject)
            {
                // Add the spawned object to the UI menu logic's dictionary
                _uiMenuLogic.UIObjectDictionary.Add(spawnObject, this);
                
                var particles = Instantiate(spawnParticlesPrefab, spawnPosition, Quaternion.identity);
                var particleSystem = particles.GetComponent<ParticleSystem>();
                
                if (particleSystem != null)
                    particleSystem.Play();
            }

            gameObject.SetActive(!spawnedObject);
            return;
        }
    }
}
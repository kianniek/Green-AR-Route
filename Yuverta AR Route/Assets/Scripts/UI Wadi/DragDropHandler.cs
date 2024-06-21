using System.Collections.Generic;
using Events.GameEvents.Typed;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

//By Glenn
[RequireComponent(typeof(ObjectSpawner))]
public class DragDropHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    public GameObject itemPrefab; // The actual item to place on the grid
    public Sprite dragSprite; // The sprite to display while dragging
    private GameObject dragObject; // The temporary drag object (UI representation)
    private Canvas canvas;
    private Camera mainCamera;
    private bool isDragging;
    private ObjectSpawner _objectSpawner;
    [SerializeField] private ARRaycastManager m_RaycastManager;

    private GridManager _gridManager;

    // Input action references
    [SerializeField] private InputActionReference touchAction;
    

    private void Start()
    {
        mainCamera = Camera.main; // Ensure this is the correct camera for your setup
        canvas = FindObjectOfType<Canvas>(); // Find the canvas in the scene
        _gridManager = FindObjectOfType<GridManager>();
        _objectSpawner = GetComponent<ObjectSpawner>();

        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene.");
        }

        // Enable the touch input action
        touchAction.action.Enable();
    }

    private void Update()
    {
        // Handle touch input
        if (touchAction.action.triggered)
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                var touch = touchscreen.primaryTouch;
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    var eventData = new PointerEventData(EventSystem.current)
                    {
                        position = touch.position.ReadValue()
                    };
                    OnPointerDown(eventData);
                }
                else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
                {
                    var eventData = new PointerEventData(EventSystem.current)
                    {
                        position = touch.position.ReadValue()
                    };
                    OnDrag(eventData);
                }
                else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    var eventData = new PointerEventData(EventSystem.current)
                    {
                        position = touch.position.ReadValue()
                    };
                    OnEndDrag(eventData);
                    OnPointerUp(eventData);
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        gameObject.GetComponent<Button>().onClick.Invoke();
        CreateDragImage(eventData);
        isDragging = false; // Reset dragging state
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            dragObject.transform.position = eventData.position; // Follow the mouse or finger
            isDragging = true; // Set dragging state to true
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            SpawnObject(eventData.position);

            Destroy(dragObject);
        }

        dragObject = null; // Clear the reference
        isDragging = false; // Reset dragging state
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // If the drag never started, destroy the drag object on pointer up
        if (!isDragging && dragObject != null)
        {
            Destroy(dragObject);
            dragObject = null;
        }
    }

    private void CreateDragImage(PointerEventData eventData)
    {
        dragObject = new GameObject("DragImage");
        dragObject.transform.SetParent(canvas.transform, false); // Make it a child of the canvas
        dragObject.transform.position = eventData.position;

        var image = dragObject.AddComponent<Image>();
        image.sprite = dragSprite;
        image.raycastTarget = false; // Make sure it does not block any events

        var rectTransform = image.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 100); // Set size, adjust as needed
    }

    private void SpawnObject(Vector2 screenPosition)
    {
        var hitResults = new List<ARRaycastHit>();
        m_RaycastManager.Raycast(screenPosition, hitResults, UnityEngine.XR.ARSubsystems.TrackableType.Planes);

        if (hitResults.Count == 0) return;

        foreach (var hit in hitResults)
        {
            if (hit.pose != null && hit.trackable != null && hit.trackable.gameObject.CompareTag("Ground"))
            {
                var hitNormal = hit.pose.rotation * Vector3.up;
                _objectSpawner.TrySpawnObject(hit.pose.position, hitNormal.normalized, out var spawnedObject);
                return;
            }
        }
    }
}
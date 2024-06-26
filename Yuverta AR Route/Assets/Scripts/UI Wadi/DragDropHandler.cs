using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;

//By Glenn
[RequireComponent(typeof(ObjectSpawner))]
public class DragDropHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    [SerializeField] private string tagToRaycast;
    [SerializeField] private ARRaycastManager m_RaycastManager;
    public GameObject itemPrefab; // The actual item to place on the grid
    public Sprite dragSprite; // The sprite to display while dragging
    [SerializeField] private GameObject visuals;

    private GameObject dragObject; // The temporary drag object (UI representation)
    private Canvas _canvas;
    private bool isDragging;
    private ObjectSpawner _objectSpawner;
    private GridManager _gridManager;

    // Input action references
    [SerializeField] private InputActionProperty dragDeltaAction;

    private void Start()
    {
        _canvas = FindObjectOfType<Canvas>(); // Find the _canvas in the scene
        _gridManager = FindObjectOfType<GridManager>();
        _objectSpawner = GetComponent<ObjectSpawner>();

        _objectSpawner.ObjectPrefabs.Clear();
        _objectSpawner.ObjectPrefabs.Add(itemPrefab);

        if (!_canvas)
        {
            Debug.LogError("No Canvas found in the scene.");
        }

        // Enable the drag delta input action if there is one
        if (dragDeltaAction != null && dragDeltaAction.action != null)
        {
            dragDeltaAction.action.Enable();
        }
    }

    private void Update()
    {
        //if ARRaycastManager is not set, find it
        if (!m_RaycastManager)
        {
            m_RaycastManager = FindObjectOfType<ARRaycastManager>();
        }

        // Handle drag delta input action
        if (dragDeltaAction.action != null && dragDeltaAction.action.triggered)
        {
            Vector2 touchPosition = dragDeltaAction.action.ReadValue<Vector2>();

            if (Touchscreen.current.primaryTouch.press.isPressed)
            {
                var eventData = new PointerEventData(EventSystem.current)
                {
                    position = touchPosition
                };
                OnPointerDown(eventData);
            }
            else if (Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                var eventData = new PointerEventData(EventSystem.current)
                {
                    position = touchPosition
                };
                OnDrag(eventData);
            }
            else if (Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                var eventData = new PointerEventData(EventSystem.current)
                {
                    position = touchPosition
                };
                OnEndDrag(eventData);
                OnPointerUp(eventData);
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
        dragObject.transform.SetParent(_canvas.transform, false); // Make it a child of the _canvas
        dragObject.transform.position = eventData.position;

        var image = dragObject.AddComponent<Image>();
        image.sprite = dragSprite;
        image.raycastTarget = false; // Make sure it does not block any events

        var rectTransform = image.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 100); // Set size, adjust as needed
        
        // Disable the visuals of the object
        visuals.SetActive(false);
        
        //move this object to the end of the children
        dragObject.transform.SetAsLastSibling();
    }

    private void SpawnObject(Vector2 screenPosition)
    {
        var RayFromScreen = Camera.main.ScreenPointToRay(screenPosition);
        //raycast to find the position to spawn the object
        if (Physics.Raycast(RayFromScreen, out var hit, 100f))
        {
            if (!hit.transform.CompareTag(tagToRaycast))
                return;
            
            var deleteObj = _objectSpawner.TrySpawnObject(hit.point, hit.normal, out var spawnedObject);
            
            gameObject.SetActive(!deleteObj);
            return;
        }
    }
}
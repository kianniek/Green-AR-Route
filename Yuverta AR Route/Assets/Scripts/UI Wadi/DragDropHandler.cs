using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;

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
    }

    private void Update()
    {
        // If ARRaycastManager is not set, find it
        if (!m_RaycastManager)
        {
            m_RaycastManager = FindObjectOfType<ARRaycastManager>();
        }

        // Handle touch input
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
                    OnEndDrag(eventData);
                    OnPointerUp(eventData);
                    break;
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
        //visuals.SetActive(false);

        // Move this object to the end of the children
        dragObject.transform.SetAsLastSibling();
    }

    private void SpawnObject(Vector2 screenPosition)
    {
        var RayFromScreen = Camera.main.ScreenPointToRay(screenPosition);
        // Raycast to find the position to spawn the object
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

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;

//By Glenn
public class DragDropHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    public GameObject itemPrefab;  // The actual item to place on the grid
    public Sprite dragSprite;      // The sprite to display while dragging
    private GameObject dragObject; // The temporary drag object (UI representation)
    private Canvas canvas;
    private Camera mainCamera;
    private bool isDragging;

    void Start()
    {
        mainCamera = Camera.main; // Ensure this is the correct camera for your setup
        canvas = FindObjectOfType<Canvas>(); // Find the canvas in the scene
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene.");
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
            dragObject.transform.position = Input.mousePosition; // Follow the mouse or finger
            isDragging = true; // Set dragging state to true
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            SpawnObject();
            GridManager.Instance.uiMenu.isDragging = false;
            Destroy(dragObject);
            /*if (PlaceItemInGrid(Input.mousePosition))
            {
                Destroy(dragObject); // Successfully placed, destroy the image
            }
            else
            {
                // Not placed successfully, destroy any drag object
                if (dragObject != null) Destroy(dragObject);
            }*/
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
        dragObject.transform.position = Input.mousePosition;

        Image image = dragObject.AddComponent<Image>();
        image.sprite = dragSprite;
        image.raycastTarget = false; // Make sure it does not block any events

        RectTransform rectTransform = image.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 100); // Set size, adjust as needed
    }

    private bool PlaceItemInGrid(Vector3 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1.0f); // Visualize the ray

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.collider.CompareTag("GridPoint"))
            {
                // Instantiate and place the actual prefab at the hit location
                GameObject placedObject = Instantiate(itemPrefab, hit.collider.transform.position, Quaternion.identity, hit.collider.transform);
                placedObject.transform.localScale = itemPrefab.transform.localScale; // Ensure scale is correct
                return true;
            }
        }
        return false; 
    }
    
    private void SpawnObject()
    {
        var touchHits = SharedFunctionality.Instance.TouchToRay();

        if (touchHits.Length == 0) return;

        foreach (var hit in touchHits)
        {
            if (hit.collider.gameObject.CompareTag("Ground"))
            {
                GridManager.Instance.objectSpawner.TrySpawnObject(hit.point, hit.normal);
                return;
            }
        }
    }
}

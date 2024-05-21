using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TouchPhase = UnityEngine.TouchPhase;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem current;

    public GridLayout gridLayout;
    private Grid grid;
    private Tilemap MainTileMap;
    [SerializeField] private List<Tilemap> tileMaps;
    [SerializeField] private List<MeshCollider> groundLayers;
    [SerializeField] private TileBase placeAbleTile;
    
    [SerializeField] private ScrollArea scrollArea;
    public int selectedPrefabIndex;
    [SerializeField] private List<Material> groundMaterials;
    [SerializeField] public float loweredAlphaLayer;
    [SerializeField] private float alphaStepValue;

    public GameObject[] prefab;

    public List<GameObject> placedObjects;

    public PlaceableObject objectToPlace;

    public ObjectDrag currentDrag;

    private ARRaycastManager arRaycastManager;

    private List<GameObject> cubes;

    private int currentIndex;

    public bool dragging;
    private bool draggingUI;

    private float layerDistance;
    private int layerIndex;

    [SerializeField] private float minSwipeDistanceUI;

    [SerializeField] private GameObject blockParent;

    private Dictionary<GameObject, int> layerSave;

    #region UnityMethods

    private void Awake()
    {
        cubes = new List<GameObject>();
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        MainTileMap = tileMaps[0];
        current = this;
        grid = gridLayout.gameObject.GetComponent<Grid>();
        groundLayers[0].enabled = true;
        for (int i = 1; i < groundLayers.Count; i++)
        {
            groundLayers[i].enabled = false;
        }
        layerDistance = groundLayers[1].transform.position.y - groundLayers[0].transform.position.y;
        selectedPrefabIndex = -1;
        layerSave = new Dictionary<GameObject, int>();
        
        foreach (var material in groundMaterials)
        {
            if (material == groundMaterials[0]) continue;
            var color = material.color;
            color.a = 0;
            material.color = color;
        }
    }

    private void Update()
    {
        if (Input.touchCount <= 0) return;
        
        //Debug
        if (Input.GetKeyDown(KeyCode.A))
        {
            SpawnBlock();
        }
        
        if (!objectToPlace) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Rotate();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveUp();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveDown();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaceBlock();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            RemoveBlock();
        }
    }

    #endregion

    #region Input functions

    public void SelectObject()
    {
        if (UIHit() || dragging && currentDrag != null) return;
        if (cubes.Count == 0)
        {
            SpawnBlockOnTouch();
            return;
        }
        
        var rays = TouchToRay();
        
        if (rays.Length == 0)
        {
            NullifyBlock();
            return;
        }

        var ray = rays[0];
        
        if (ray.collider == null)
        {
            NullifyBlock();
            return;
        }
                
        var collider = ray.collider.gameObject;

        if (collider == null) 
        { 
            NullifyBlock();
            return;
        }

        if (!collider.CompareTag("MoveableObject"))
        {
            if (!collider.CompareTag("UI") || !collider.TryGetComponent<Button>(out var button))
            {
                if (collider.CompareTag("Ground") || collider.CompareTag("ARGround"))
                {
                    if (selectedPrefabIndex >= 0) SpawnBlockOnTouch();
                    else if (currentDrag != null)
                    {
                        DragObject();
                    }
                    else
                    {
                        NullifyBlock();
                        foreach (var ray2 in rays)
                        {
                            if (ray2.collider.CompareTag("MoveableObject"))
                            {
                                Debug.Log(collider.gameObject);
                                SelectBlock(collider);
                                return;
                            }
                        }
                    }
                    return;
                }
                
                NullifyBlock();
                return;
            }
            button.onClick.Invoke();
            return;
        }

        if (currentDrag&& currentDrag.gameObject == collider)
        {
            return;
        }

        NullifyBlock();
        SelectBlock(collider);
        //currentTouchIndex = Input.GetTouch(0).fingerId;
    }

    public void DragObject()
    {
        if (cubes.Count <= 0 || UIHit() || draggingUI /*|| Input.touchCount <= 0*/) return;
        if (currentDrag != null && dragging)
        {
            objectToPlace.canBePlaced = CanBePlaced(objectToPlace, MainTileMap);
            currentDrag.OnTouchDrag();
            return;
        }
        
        var rays = TouchToRay();
        if (rays.Length == 0) return;
        var ray = rays[0];
        
        if (ray.collider != null && ray.collider.gameObject.CompareTag("MoveableObject"))
        {
            SelectObject();
            return;
        }
        
        //Dragging code for the UI here
        StartCoroutine(CheckUIDrag());
    }

    #endregion

    #region UI functions

    private IEnumerator CheckUIDrag()
    {
        Debug.Log("UIDrag");
        draggingUI = true;
        Vector2 startPosition = new Vector2();
        Vector2 endPosition = new Vector2();

        switch (Input.touchCount)
        {
            case 0:
                startPosition = Input.mousePosition;
                while (Input.GetMouseButtonDown(0))
                {
                    yield return new WaitForFixedUpdate();
                }

                endPosition = Input.mousePosition;
                break;
            default:
                if (Input.GetTouch(0).phase == TouchPhase.Began) startPosition = Input.GetTouch(0).position;
                while (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    yield return new WaitForFixedUpdate();
                }

                if (Input.GetTouch(0).phase == TouchPhase.Ended) endPosition = Input.GetTouch(0).position;
                break;
        }

        draggingUI = false;

        var distance = endPosition.y - startPosition.y;

        if (distance ! > minSwipeDistanceUI)
        {
            NullifyBlock();
            SelectObject();
            Debug.Log("Distance too small");
            yield return null;
        }

        if (endPosition.x > scrollArea.gameObject.transform.position.x +
            scrollArea.gameObject.GetComponent<BoxCollider>().size.x)
        {
            Debug.Log("Touched scrollUI");
            yield return null;
        }

        switch (distance)
        {
            case > 0:
                if (MainTileMap != tileMaps[^1]) SwapLayer(layerIndex + 1);
                break;
            case < 0:
                //No need to fear it dropping below 0 cuz if it is 0 it can't be called again
                if (MainTileMap != tileMaps[0]) SwapLayer(layerIndex - 1);
                break;
        }
    }
    
    public void PlaceBlock()
    {
        if (CanBePlaced(objectToPlace, MainTileMap))
        {
            objectToPlace.Place();
            objectToPlace.canvas.SetActive(false);
            var start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
            TakeArea(start, objectToPlace.Size);
            scrollArea.TurnOffUI(objectToPlace);
        }
        else StartCoroutine(CannotBePlaced());
    }

    private void SpawnBlockOnTouch()
    {
        if (selectedPrefabIndex < 0) return;
        var rays = TouchToRay();
        if (!rays[0].collider) return;
        var ray = rays[0];
        InitializeWithObject(prefab[selectedPrefabIndex], ray.point);
    }

    public void RemoveBlock()
    {
        scrollArea.TurnOnUI(objectToPlace);
        cubes.Remove(objectToPlace.gameObject);
        Destroy(objectToPlace.gameObject);
    }

    public void SpawnBlock()
    {
        if (selectedPrefabIndex < 0) return;
        InitializeWithObject(prefab[selectedPrefabIndex], Vector3.zero);
    }

    public void MoveUp()
    {
        if (MainTileMap == tileMaps[^1]) return;
        CheckList(objectToPlace.gameObject);   
        
        var pos = objectToPlace.transform.position;
        pos.y += layerDistance;
        objectToPlace.transform.position = pos;
        groundLayers[layerIndex].enabled = false;

        layerIndex++;
        MainTileMap = tileMaps[layerIndex];
        groundLayers[layerIndex].enabled = true;
        currentDrag.UpdateLayer(1);
    }

    public void MoveDown()
    {
        if (MainTileMap == tileMaps[0]) return;
        CheckList(objectToPlace.gameObject);    
        
        var pos = objectToPlace.transform.position;
        pos.y -= layerDistance;
        objectToPlace.transform.position = pos;
        groundLayers[layerIndex].enabled = false;

        layerIndex--;
        MainTileMap = tileMaps[layerIndex];
        groundLayers[layerIndex].enabled = true;
        currentDrag.UpdateLayer(0);
    }

    public void ChangeSelectedBlock(int index)
    {
        if (index < 0) selectedPrefabIndex = index;
        
        selectedPrefabIndex = index;
    }

    //Not used (but could?)
    public void Rotate()
    {
        objectToPlace.Rotate();
    }

    #endregion

    #region Utils

    private void SelectBlock(GameObject newBlock)
    {
        objectToPlace = newBlock.GetComponent<PlaceableObject>();
        currentDrag = newBlock.GetComponent<ObjectDrag>();
        currentDrag.selected = true;
        currentDrag.OnTouch();
        scrollArea.TurnOnUI(objectToPlace);
    }
    
    /// <summary>
    /// This function checks if the mouse or finger/touch is not hitting the UI
    /// </summary>
    /// <returns></returns>
    private bool UIHit()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        
        if (Input.touchCount <= 0) return false;
        
        var touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return false;
        return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
    }

    private void NullifyBlock()
    {
        if (!objectToPlace) return;
        objectToPlace.canvas.SetActive(false);
        objectToPlace = null;
        
        if (!currentDrag) return;
        currentDrag.UnSelect();
        currentDrag = null;
    }

    private void SwapLayer(int newTileMap)
    {
        var currentAlpha = loweredAlphaLayer;

        for (currentAlpha = 0; currentAlpha < 1; currentAlpha += alphaStepValue)
        {
            foreach (var t in cubes)
            {
                var reference = t.GetComponent<ObjectDrag>();
                if (reference.floorLevel == newTileMap)
                {
                    reference.ChangeAlpha(alphaStepValue);
                    continue;
                }
                reference.ChangeAlpha(-alphaStepValue);
                /*var currentColor = layerMaterials[i].color;
                
                Debug.Log(currentColor.a);

                if (i == newTileMap)
                {
                    currentColor.a += alphaStepValue;
                    layerMaterials[i].color = currentColor;

                    foreach (var block in layerSave.Where(block => block.Value == i))
                    {
                        block.Key.GetComponent<ObjectDrag>().ChangeAlpha(currentColor.a);
                    }

                    continue; // Skip to the next iteration
                }

                if (currentColor.a <= loweredAlphaLayer) continue;
                currentColor.a -= alphaStepValue;
                layerMaterials[i].color = currentColor;
                foreach (var block in layerSave.Where(block => block.Value == i))
                {
                    block.Key.GetComponent<ObjectDrag>().ChangeAlpha(currentColor.a);
                }*/
            }
        }
        
        bool up = newTileMap > layerIndex;
        groundLayers[layerIndex].enabled = false;
        layerIndex = newTileMap;
        MainTileMap = tileMaps[layerIndex];
        groundLayers[layerIndex].enabled = true;

        if (!currentDrag || !objectToPlace) return;
        
        CheckList(objectToPlace.gameObject);    
        
        if (currentDrag.floorLevel == layerIndex) return;
        var pos = objectToPlace.transform.position;
        pos.y += up ? layerDistance : -layerDistance;
        objectToPlace.transform.position = pos;
        currentDrag.UpdateLayer(layerIndex);
    }
    
    public Vector3 GetTouchWorldPosition()
    {
        if (Input.touchCount <= 0) return GetMouseWorldPosition();
    
        var touchPosition = Input.GetTouch(0).position;
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        
        var rayCastHit = new List<ARRaycastHit>();
        return arRaycastManager.Raycast(ray, rayCastHit) ? rayCastHit[0].pose.position : Vector3.zero;
    }
    
    //Debug
    public static Vector3 GetMouseWorldPosition()
    {
        var ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out var rayCastHit) ? rayCastHit.point : Vector3.zero;
    }

    public Vector3 SnapCoordinateToGrid(Vector3 newPosition, GameObject objectToMove = null)
    {
        if (!objectToPlace)
        {
            return objectToMove switch
            {
                not null => objectToMove.transform.position,
                null => currentDrag switch
                {
                    not null => currentDrag.gameObject.transform.position,
                    null => Vector3.zero
                }
            };
        }
        if (objectToMove != null)
        {
            CheckList(objectToMove);
            if (!CanBePlaced(objectToMove!.GetComponent<PlaceableObject>(), MainTileMap))
            {
                newPosition.y = objectToMove.gameObject.transform.position.y;
                objectToPlace.canBePlaced = false;
                return newPosition;
            }
        }
        var cellPos = gridLayout.WorldToCell(newPosition);
        newPosition = grid.GetCellCenterWorld(cellPos);
        if (objectToMove != null) newPosition.y = objectToMove.gameObject.transform.position.y;
        objectToPlace.canBePlaced = true;
        return newPosition;
    }

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        var array = new TileBase[area.size.x * area.size.y * area.size.z];
        var counter = 0;

        foreach (var v in area.allPositionsWithin)
        {
            var pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }

        return array;
    }

    private void CheckList(GameObject objectToMove)
    {
        if (!placedObjects.Contains(objectToMove)) return;
        
        placedObjects.Remove(objectToMove);
        RemoveArea(gridLayout.WorldToCell(objectToPlace.GetStartPosition()), objectToPlace.Size);
        var color = objectToPlace.material.color;
        color.a = 255;
        objectToPlace.material.color = color;
        objectToPlace.canvas.SetActive(true);
    }
    
    private RaycastHit[] TouchToRay()
    {
        Vector2 touchPosition;
        try
        {
            touchPosition = Input.GetTouch(0).position;
        }
        catch (Exception e)
        {
            touchPosition = Input.mousePosition;
        }
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        var hits = Physics.RaycastAll(ray);
        return hits;
    }

    #endregion

    #region Building Placement

    private void InitializeWithObject(GameObject prefab, Vector3 spawnPosition)
    {
        NullifyBlock();
        
        var newPos = spawnPosition;
        newPos.y = MainTileMap.transform.position.y;
        spawnPosition = newPos;
        
        var obj = Instantiate(prefab, spawnPosition, Quaternion.identity, blockParent.transform);
        objectToPlace = obj.GetComponent<PlaceableObject>();
        cubes.Add(prefab);
        currentIndex = objectToPlace.index = cubes.Count - 1;
        currentDrag = obj.GetComponent<ObjectDrag>();
        currentDrag.selected = true;
        currentDrag.floorLevel = layerIndex;
        currentDrag.UpdateLayer(0);
        obj.name += " " + cubes.Count;
        scrollArea.TurnOffUI(objectToPlace);
        scrollArea.NoBlockSelected();
        layerSave.Add(obj, layerIndex);
    }

    private bool CanBePlaced(PlaceableObject placeableObject, Tilemap tilemap)
    {
        if (!objectToPlace) return false;
        var area = new BoundsInt();
        area.position = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
        area.size = placeableObject.Size + Vector3Int.one;

        var baseArray = GetTilesBlock(area, tilemap);
        
        foreach (var b in baseArray)
        {
            if (b == placeAbleTile) return true;
        }
        
        return false;

        //return baseArray.All(b => b == whiteTile);
    }

    private IEnumerator CannotBePlaced()
    {
        var gameObject = objectToPlace.gameObject;
        var renderer = gameObject.GetComponent<MeshRenderer>();

        var material = renderer.material;
        
        var oldColor = material.color;
        material.color = Color.black;
        
        //WIP animation cannot be placed
        /*var oldPosition = gameObject.transform.position;
        var randomRange = new Vector2(0.01f, 0.1f);
        
        gameObject.transform.Translate(oldPosition + new Vector3(Random.Range(randomRange.x, randomRange.y), 0, Random.Range(randomRange.x, randomRange.y)));

        yield return new WaitForSeconds(0.2f);
        
        gameObject.transform.Translate(oldPosition + new Vector3(Random.Range(-randomRange.x, -randomRange.y), 0, Random.Range(-randomRange.x, -randomRange.y)));*/
        
        yield return new WaitForSeconds(0.1f);
        material.color = oldColor;
        //gameObject.transform.Translate(oldPosition);
    }

    private void TakeArea(Vector3Int start, Vector3Int size)
    {
        MainTileMap.BoxFill(start, null, start.x, start.y, start.x + size.x, start.y + size.y);
        
        //if (MainTileMap == SecondTileMap) return;
        //start = gridLayout.WorldToCell(objectToPlace.GetStartPosition())
        
        //Turned off for now but could be used later (turns off higher tilePlacements)
        //SecondTileMap.BoxFill(start, whiteTile, start.x, start.y, start.x + size.x, start.y + size.y);
    }

    private void RemoveArea(Vector3Int start, Vector3Int size)
    {
        MainTileMap.BoxFill(start, placeAbleTile, start.x, start.y, start.x + size.x, start.y + size.y); 
        
        //Debatable if the layers should be separate or only buildable if there is something under the block
        //if (MainTileMap == SecondTileMap) return;
        
        //Turned off for now but could be used later (turns off higher tilePlacements)
        //SecondTileMap.BoxFill(start, null, start.x, start.y, start.x + size.x, start.y + size.y);
    }

    #endregion
}

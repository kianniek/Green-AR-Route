using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TouchPhase = UnityEngine.TouchPhase;

public class BuidlingSystem : MonoBehaviour
{
    public static BuidlingSystem current;

    public GridLayout gridLayout;
    private Grid grid;
    private Tilemap MainTileMap;
    [SerializeField] private List<Tilemap> tileMaps;
    [SerializeField] private List<MeshCollider> groundLayers;
    [SerializeField] private TileBase placeAbleTile;
    [SerializeField] private ScrollArea scrollArea;

    [SerializeField] private List<Material> layerMaterials;
    [SerializeField] private float loweredAlphaLayer;
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
    }

    private void Update()
    {
        if (Input.touchCount <= 0) return;
        
        //Debug
        if (Input.GetKeyDown(KeyCode.A))
        {
            SpawnBlock(0);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            SpawnBlock(1);
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
        if (UIHit() || cubes.Count == 0 || dragging && currentDrag != null) return;
        
        var ray = TouchToRay();

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
                NullifyBlock();
                return;
            }
            button.onClick.Invoke();
            return;
        }

        if (currentDrag&& currentDrag.gameObject == collider)
        {
            Debug.Log("lol");
            return;
        }

        NullifyBlock();
        currentDrag = collider.GetComponent<ObjectDrag>();
        Debug.Log(currentDrag);
        currentDrag.selected = true;
        currentDrag.OnTouch(); //Does this basically: objectToPlace = currentDrag.script;
        currentIndex = objectToPlace.index; 
        objectToPlace.canvas.enabled = true;
        MainTileMap = tileMaps[currentDrag.floorLevel]; 
        objectToPlace.canBePlaced = CanBePlaced(objectToPlace, MainTileMap);
        //currentTouchIndex = Input.GetTouch(0).fingerId;
    }

    public void DragObject()
    {
        if (cubes.Count <= 0 || UIHit() || draggingUI || Input.touchCount <= 0) return;
        if (currentDrag != null && dragging)
        {
            objectToPlace.canBePlaced = CanBePlaced(objectToPlace, MainTileMap);
            currentDrag.OnTouchDrag();
            return;
        }
        
        var ray = TouchToRay();
        
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
        draggingUI = true;
        Vector2 startPosition = new Vector2();
        if (Input.GetTouch(0).phase == TouchPhase.Began) startPosition = Input.GetTouch(0).position;
        while (Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            yield return new WaitForFixedUpdate();
        }

        Vector2 endPosition = new Vector2();
        if (Input.GetTouch(0).phase == TouchPhase.Ended) endPosition = Input.GetTouch(0).position;

        var distance = (endPosition - startPosition).magnitude;

        if (distance !> minSwipeDistanceUI) yield return null;
        if (endPosition.x > scrollArea.gameObject.transform.position.x +
            scrollArea.gameObject.GetComponent<BoxCollider>().size.x) yield return null;

        switch (distance)
        {
            case > 0:
                if (MainTileMap != tileMaps[1]) SwapLayer(1);
                break;
            case < 0:
                if (MainTileMap != tileMaps[0]) SwapLayer(0);
                break;
        }
    }
    
    public void PlaceBlock()
    {
        if (CanBePlaced(objectToPlace, MainTileMap))
        {
            objectToPlace.Place();
            objectToPlace.canvas.enabled = false;
            var start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
            TakeArea(start, objectToPlace.Size);
        }
        else StartCoroutine(CannotBePlaced());
    }

    public void RemoveBlock()
    {
        Destroy(objectToPlace.gameObject);
    }

    public void SpawnBlock(int i = 0)
    {
        //Temp if
        //if(cubes.Count > placedObjects.Count) return;
        InitializeWithObject(prefab[i]);
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
        currentDrag.UpdateLayer(1, layerMaterials[1]);
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
        currentDrag.UpdateLayer(0, layerMaterials[0]);
    }

    //Not used (but could?)
    public void Rotate()
    {
        objectToPlace.Rotate();
    }

    #endregion

    #region Utils

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
        if (objectToPlace == null) return;
        objectToPlace.canvas.enabled = false;
        objectToPlace = null;
        currentDrag.UnSelect();
        currentDrag = null;
    }

    private void SwapLayer(int newTileMap)
    {
        var currentAlpha = layerMaterials[newTileMap].color.a;
        while (currentAlpha !>= 1)
        {
            for (int i = 0; i < layerMaterials.Count; i++)
            {
                if (i == newTileMap)
                {
                    var currentColor = layerMaterials[i].color;
                    currentAlpha = currentColor.a -= alphaStepValue;
                    layerMaterials[i].color = currentColor;
                }
                
                var currentColorOther = layerMaterials[i].color;
                if (currentColorOther.a >= 1) continue;
                currentColorOther.a += alphaStepValue;
                layerMaterials[i].color = currentColorOther;
            }
        }
    }
    
    //Debug
    public static Vector3 GetMouseWorldPosition()
    {
        var ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out var rayCastHit) ? rayCastHit.point : Vector3.zero;
    }
    
    public Vector3 GetTouchWorldPosition()
    {
        if (Input.touchCount <= 0) return GetMouseWorldPosition();
    
        var touchPosition = Input.GetTouch(0).position;
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        
        var rayCastHit = new List<ARRaycastHit>();
        return arRaycastManager.Raycast(ray, rayCastHit) ? rayCastHit[0].pose.position : Vector3.zero;
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
        objectToPlace.canvas.enabled = true;
    }
    
    private RaycastHit TouchToRay()
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
        Physics.Raycast(ray, out var rayCastHit);
        return rayCastHit;
    }
    /*
    private ARRaycastHit TouchToARRay()
    {
        var touchPosition = Input.GetTouch(0).position;
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        var rayCastHit = new List<ARRaycastHit>();
        arRaycastManager.Raycast(ray, rayCastHit);
        return rayCastHit[0];
    }*/

    #endregion

    #region Building Placement

    public void InitializeWithObject(GameObject prefab)
    {
        var position = SnapCoordinateToGrid(Vector3.zero);

        NullifyBlock();
        
        var obj = Instantiate(prefab, position, Quaternion.identity);
        objectToPlace = obj.GetComponent<PlaceableObject>();
        cubes.Add(prefab);
        currentIndex = objectToPlace.index = cubes.Count - 1;
        currentDrag = obj.GetComponent<ObjectDrag>();
        currentDrag.selected = true;
        currentDrag.UpdateLayer(0, layerMaterials[0]);
        obj.name = obj.name + cubes.Count;
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

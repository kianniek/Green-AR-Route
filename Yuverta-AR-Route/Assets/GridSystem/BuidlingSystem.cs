using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.XR.ARFoundation;
using Random = UnityEngine.Random;

public class BuidlingSystem : MonoBehaviour
{
    [SerializeField] private bool debug;
    
    public static BuidlingSystem current;

    public GridLayout gridLayout;
    private Grid grid;
    private Tilemap MainTileMap;
    [SerializeField] private Tilemap FirstTileMap;
    [SerializeField] private Tilemap SecondTileMap;
    [SerializeField] private MeshCollider FirstGroundLayer;
    [SerializeField] private MeshCollider SecondGroundLayer;
    [SerializeField] private TileBase whiteTile;

    public GameObject[] prefab;

    public List<GameObject> placedObjects;

    public PlaceableObject objectToPlace;

    public ObjectDrag currentDrag;

    private ARRaycastManager arRaycastManager;

    private List<GameObject> cubes;

    public int currentIndex;

    public bool dragging;

    private float layerDistance;

    #region UnityMethods

    private void Awake()
    {
        cubes = new List<GameObject>();
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        MainTileMap = FirstTileMap;
        current = this;
        grid = gridLayout.gameObject.GetComponent<Grid>();
        FirstGroundLayer.enabled = true;
        SecondGroundLayer.enabled = false;
        layerDistance = SecondGroundLayer.transform.position.y - FirstGroundLayer.transform.position.y;
    }

    private void Update()
    {
        if (!debug)
        {
            if (Input.touchCount <= 0) return;
            
            Debug.Log("Touch");

            var ray = TouchToRay();

            if (ray.collider == null) return;
            
            var collider = ray.collider.gameObject;
            
            if (collider != null && collider.CompareTag("MoveableObject"))
            {
                if (!dragging && objectToPlace.index != currentIndex)
                {
                    currentDrag.selected = false; //Turn of the previous dragTimer
                    currentDrag = collider.GetComponent<ObjectDrag>();
                    currentDrag.selected = true;
                    currentDrag.OnTouch(); //Does this basically objectToPlace = currentDrag.script;
                    currentIndex = objectToPlace.index;
                    MainTileMap = currentDrag.floorLevel == 0 ? FirstTileMap : SecondTileMap;
                }
                
                currentDrag.OnTouchDrag();
            }
            return;
        }
        
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

    #region UI functions
    
    public void PlaceBlock()
    {
        if (CanBePlaced(objectToPlace, MainTileMap))
        {
            objectToPlace.Place();
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
        if(cubes.Count > placedObjects.Count) return;
        InitializeWithObject(prefab[i]);
    }

    public void MoveUp()
    {
        if (!(objectToPlace.transform.position.y < layerDistance)) return;
        CheckList(objectToPlace.gameObject);   
        
        var pos = objectToPlace.transform.position;
        pos.y += layerDistance;
        objectToPlace.transform.position = pos;
        MainTileMap = SecondTileMap;
        FirstGroundLayer.enabled = false;
        SecondGroundLayer.enabled = true;
        currentDrag.floorLevel = 1;
    }

    public void MoveDown()
    {
        if (!(objectToPlace.transform.position.y > layerDistance)) return;
        CheckList(objectToPlace.gameObject);    
        
        var pos = objectToPlace.transform.position;
        pos.y -= layerDistance;
        objectToPlace.transform.position = pos;
        MainTileMap = FirstTileMap;
        FirstGroundLayer.enabled = true;
        SecondGroundLayer.enabled = false;
        currentDrag.floorLevel = 0;
    }

    //Not used (but could?)
    public void Rotate()
    {
        objectToPlace.Rotate();
    }

    #endregion

    #region Utils

    //Debug
    public static Vector3 GetMouseWorldPosition()
    {
        if (Input.touchCount <= 0) return Vector3.zero;
    
        var touchPosition = Input.GetTouch(0).position;
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        
        //Debug
        //var ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out var rayCastHit) ? rayCastHit.point : Vector3.zero;
    }
    
    public Vector3 GetTouchWorldPosition()
    {
        if (Input.touchCount <= 0) return Vector3.zero;
    
        var touchPosition = Input.GetTouch(0).position;
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        
        var rayCastHit = new List<ARRaycastHit>();
        return arRaycastManager.Raycast(ray, rayCastHit) ? rayCastHit[0].pose.position : Vector3.zero;
    }

    public Vector3 SnapCoordinateToGrid(Vector3 newPosition, GameObject objectToMove = null)
    {
        if (objectToMove != null)
        {
            CheckList(objectToMove);
            if (!CanBePlaced(objectToMove!.GetComponent<PlaceableObject>(), MainTileMap))
            {
                newPosition.y = objectToMove.gameObject.transform.position.y;
                return newPosition;
            }
        }
        var cellPos = gridLayout.WorldToCell(newPosition);
        newPosition = grid.GetCellCenterWorld(cellPos);
        if (objectToMove != null) newPosition.y = objectToMove.gameObject.transform.position.y;
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
    }
    
    private RaycastHit TouchToRay()
    {
        var touchPosition = Input.GetTouch(0).position;
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        Physics.Raycast(ray, out var rayCastHit);
        return rayCastHit;
    }

    private ARRaycastHit TouchToARRay()
    {
        var touchPosition = Input.GetTouch(0).position;
        var ray = Camera.main!.ScreenPointToRay(touchPosition);
        var rayCastHit = new List<ARRaycastHit>();
        arRaycastManager.Raycast(ray, rayCastHit);
        return rayCastHit[0];
    }

    #endregion

    #region Building Placement

    public void InitializeWithObject(GameObject prefab)
    {
        var position = SnapCoordinateToGrid(Vector3.zero);

        var obj = Instantiate(prefab, position, Quaternion.identity);
        objectToPlace = obj.GetComponent<PlaceableObject>();
        cubes.Add(prefab);
        currentIndex = objectToPlace.index = cubes.Count - 1;
    }

    private bool CanBePlaced(PlaceableObject placeableObject, Tilemap tilemap)
    {
        var area = new BoundsInt();
        area.position = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
        area.size = placeableObject.Size + Vector3Int.one;

        var baseArray = GetTilesBlock(area, tilemap);
        
        foreach (var b in baseArray)
        {
            if (b == whiteTile) return true;
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

    public void TakeArea(Vector3Int start, Vector3Int size)
    {
        MainTileMap.BoxFill(start, null, start.x, start.y, start.x + size.x, start.y + size.y);
        
        if (MainTileMap == SecondTileMap) return;
        //start = gridLayout.WorldToCell(objectToPlace.GetStartPosition())
        //start.y += 1;
        SecondTileMap.BoxFill(start, whiteTile, start.x, start.y, start.x + size.x, start.y + size.y);
    }

    public void RemoveArea(Vector3Int start, Vector3Int size)
    {
        MainTileMap.BoxFill(start, whiteTile, start.x, start.y, start.x + size.x, start.y + size.y); 
        
        //Debatable if the layers should be separate or only buildable if there is something under the block
        if (MainTileMap == SecondTileMap) return;
        
        //start.y += 1;
        SecondTileMap.BoxFill(start, null, start.x, start.y, start.x + size.x, start.y + size.y);
    }

    #endregion
}

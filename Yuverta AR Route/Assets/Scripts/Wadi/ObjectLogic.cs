using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectLogic : MonoBehaviour
{
    
    //Values are set on spawn through the gridmanager
    public int layerObj;
    public int objectIndex;
    public int objectPrefabIndex;
    public GridManager.ObjectPosition objectPosition;
    
    public bool isPlaced;
    public Vector3 previousSnappedPosition;
    private GameObject _snappedObject;
    public GameObject SnappedObject
    {
        get => _snappedObject;
        set
        {
            _snappedObject = value;
            if (value != null)
            {
                snappedGridPoint = value.GetComponent<GridPointScript>();
            }
        }
    }
    private GridPointScript snappedGridPoint;

    [SerializeField] Material targetMaterial;
    private static readonly int ObjectLayerId = Shader.PropertyToID("_ObjectLayerId");
    private static readonly int CurrentLayerId = Shader.PropertyToID("_CurrentLayerId");
    private static readonly Vector3 newScale = new Vector3(0.2f,  0.2f, 0.2f);

    private void Start()
    {
        gameObject.transform.localScale = newScale;
        gameObject.transform.rotation = Quaternion.Euler(0, -90, 0);
        targetMaterial = GetComponent<MeshRenderer>().material;
    }

    public void OnDestroy()
    {
        if (SharedFunctionality.IsQuitting)
            return; // Stop the function if the application is closing
        
        GridManager.Instance.uiMenu.OnObjectDelete(gameObject);
    }

    public void SetObjectLayerID(int layerIdOfObject)
    {
        layerObj = layerIdOfObject;

        /*targetMaterial.SetFloat(ObjectLayerId, layerIdOfObject);
        targetMaterial.SetFloat(CurrentLayerId, GridManager.Instance.gridCurrentLayer);*/
    }

    public void Update()
    {
        if (GridManager.Instance == null)
        {
            return;
        }
        targetMaterial.SetFloat(CurrentLayerId, GridManager.Instance.gridCurrentLayer);
    }
    
    public bool IsCorrectlyPlaced()
    {
        return objectPosition == snappedGridPoint.objectPosition;
    }
    
    public void ShakeObject()
    {
        StartCoroutine(Shake());
    }
    
    private IEnumerator Shake()
    {
        var originalPos = gameObject.transform.position;
        var shakeAmount = 0.1f;
        var shakeTime = 0.1f;
        var shakeSpeed = 0.1f;
        var shakeTimer = 0.0f;
        while (shakeTimer < shakeTime)
        {
            shakeTimer += Time.deltaTime;
            gameObject.transform.position = originalPos + UnityEngine.Random.insideUnitSphere * shakeAmount;
            yield return new WaitForEndOfFrame();
        }
        gameObject.transform.position = originalPos;
    }
}

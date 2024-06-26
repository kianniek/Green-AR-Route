using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectLogic : MonoBehaviour
{
    public GridManager.ObjectGridLocation objectGridLocation;
    
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
    private static readonly Vector3 newScale = new Vector3(0.2f,  0.2f, 0.2f);

    private void Start()
    {
        gameObject.transform.localScale = newScale;
        gameObject.transform.rotation = Quaternion.Euler(0, -90, 0);
    }

    public void OnDestroy()
    {
        var uiMenuLogic = FindObjectOfType<UIMenuLogic>();
        uiMenuLogic.OnObjectDelete(gameObject);
    }
    
    public bool IsCorrectlyPlaced()
    {
        return objectGridLocation == snappedGridPoint.objectGridLocation;
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

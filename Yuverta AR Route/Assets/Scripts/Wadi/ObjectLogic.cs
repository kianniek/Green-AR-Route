using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectLogic : MonoBehaviour
{
    public GridManager.ObjectGridLocation objectGridLocation;

    private GameObject snappedGridPoint;

    public GameObject SnappedGridPoint
    {
        get => snappedGridPoint;
        private set
        {
            snappedGridPoint = value;
            currentGridPoint = SnappedGridPoint.GetComponent<GridPointScript>();
        }
    }

    private GridPointScript currentGridPoint;

    private GridManager gridManager;

    private static readonly Vector3 newScale = new Vector3(0.2f, 0.2f, 0.2f);

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        gameObject.transform.localScale = newScale;
        gameObject.transform.rotation = Quaternion.Euler(0, -90, 0);

        if (gridManager != null)
            SnappedGridPoint = gridManager.SnapToGridPoint(gameObject);
    }

    public bool IsCorrectlyPlaced()
    {
        return objectGridLocation == currentGridPoint.objectGridLocation;
    }

    public void SnapToNewGridPoint()
    {
        SnappedGridPoint = gridManager.MoveObjectToNewGridPoint(gameObject, SnappedGridPoint);
    }

    public void RemoveObjectFromGrid()
    {
        gridManager.RemoveObjectFromGrid(gameObject, SnappedGridPoint);
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
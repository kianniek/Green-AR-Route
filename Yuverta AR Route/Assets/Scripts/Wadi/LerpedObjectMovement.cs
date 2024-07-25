using System;
using UnityEngine;

[RequireComponent(typeof(ObjectLogic))]
public class LerpedObjectMovement : MonoBehaviour
{
    [Tooltip("The speed at which the object should lerp to the new position.")] [SerializeField]
    private float lerpSpeed = 0.05f;

    [SerializeField] private float snapDistance = 0.01f;

    private ObjectLogic _objectLogic;
    private Vector3 _originalPos;

    private void Start()
    {
        _objectLogic = gameObject.GetComponent<ObjectLogic>();
    }

    private void Update()
    {
        KeepObjectOnSnappedPosition();
        RotateToMatchParent();
    }

    private void KeepObjectOnSnappedPosition()
    {
        if (!_objectLogic.SnappedGridPoint)
            return;

        var closestGridPosition = _objectLogic.SnappedGridPoint.transform.position;

        //Lerp the object to the closest grid position and snap it to the point if the distance is less than 0.01
        if (Vector3.Distance(gameObject.transform.position, closestGridPosition) > snapDistance)
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, closestGridPosition, lerpSpeed);
        }
        else
        {
            gameObject.transform.position = closestGridPosition;
        }
    }

    private void RotateToMatchParent()
    {
        gameObject.transform.rotation = _objectLogic.SnappedGridPoint.transform.rotation;
    }
}
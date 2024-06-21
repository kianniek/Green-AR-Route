using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ObjectLogic))]
public class LerpedObjectMovement : MonoBehaviour
{
    
    [Tooltip("The speed at which the object should lerp to the new position.")]
    [SerializeField] private float lerpSpeed = 0.05f;
    [SerializeField] private float snapDistance = 0.01f;
    
    private ObjectLogic _objectLogic;
    
    private void Start()
    {
        _objectLogic = gameObject.GetComponent<ObjectLogic>();
    }
    
    private void Update()
    {
        KeepObjectOnSnappedPosition();
    }

    private void KeepObjectOnSnappedPosition()
    {
        if (!_objectLogic.SnappedObject) 
            return;
        
        var closestGridPosition = _objectLogic.SnappedObject.transform.position;
        
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
}
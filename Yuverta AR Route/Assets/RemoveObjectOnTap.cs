using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class RemoveObjectOnTap : MonoBehaviour
{
    [SerializeField] private InputActionReference multiTapClickAction;
    [SerializeField] private string tagToRaycast = "GridPoint";

    private ObjectLogic _objectLogic;

    private void Start()
    {
        _objectLogic = GetComponent<ObjectLogic>();

        if (_objectLogic == null)
        {
            Debug.LogError("ObjectLogic component not found on this GameObject.");
        }
    }

    private void OnEnable()
    {
        if (multiTapClickAction != null)
        {
            multiTapClickAction.action.Enable();
            multiTapClickAction.action.performed += OnMultiTap;
        }
        else
        {
            Debug.LogError("multiTapClickAction is not assigned.");
        }
    }

    private void OnDisable()
    {
        if (multiTapClickAction != null)
        {
            multiTapClickAction.action.performed -= OnMultiTap;
        }
    }

    private void OnMultiTap(InputAction.CallbackContext context)
    {
        Debug.Log("OnMultiTap");

        if (Camera.main == null)
        {
            Debug.LogError("Main camera not found.");
            return;
        }

        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                Debug.Log("Hit object: " + gameObject.name);
                _objectLogic.RemoveObjectFromGrid();
            }
        }
    }
}
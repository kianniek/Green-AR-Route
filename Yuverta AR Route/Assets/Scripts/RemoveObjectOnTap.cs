using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using TouchPhase = UnityEngine.TouchPhase;

public class RemoveObjectOnTap : MonoBehaviour
{
    [SerializeField] private string tagToRaycast = "GridPoint";
    [SerializeField] private GameObject removeParticlesPrefab;
    [SerializeField] private UnityEvent onDoubleTap = new();

    private ObjectLogic _objectLogic;

    // Double tap detection
    private float _lastTapTime = 0;
    private const float DoubleTapTime = 0.3f; // Time in seconds to consider it a double tap

    private Camera _mainCamera;

    private void Start()
    {
        _objectLogic = GetComponent<ObjectLogic>();
        _mainCamera = Camera.main;

        if (_objectLogic == null)
        {
            Debug.LogError("ObjectLogic component not found on this GameObject.");
        }
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended)
            {
                float currentTime = Time.time;
                if (currentTime - _lastTapTime < DoubleTapTime)
                {
                    OnDoubleTap(touch.position);
                }

                _lastTapTime = currentTime;
            }
        }
    }

    private void OnDoubleTap(Vector2 touchPosition)
    {
        Debug.Log("OnDoubleTap");

        if (_mainCamera == null)
        {
            Debug.LogError("Main camera not found.");
            return;
        }

        var ray = _mainCamera.ScreenPointToRay(touchPosition);

        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                Debug.Log("Hit object: " + gameObject.name);
                _objectLogic.RemoveObjectFromGrid();

                // Spawn particles
                var particles = Instantiate(removeParticlesPrefab, transform.position, Quaternion.identity);
                var particleSystem = particles.GetComponent<ParticleSystem>();
                
                if (particleSystem != null)
                    particleSystem.Play();
                
                onDoubleTap.Invoke();
            }
        }
    }
}
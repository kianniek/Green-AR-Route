using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Spawns an object at an <see cref="XRRayInteractor"/>'s raycast hit position when a trigger is activated.
/// </summary>
public class ARInteractorSpawnTrigger : MonoBehaviour
{
    [SerializeField] private bool setActiveInsteadOfInstantiate = false;
    private Vector2 touchPosition;
    [SerializeField] private LayerMask m_LayerMask = -1;

    [SerializeField, Tooltip("The AR Interactor that determines where to spawn the object.")]
    ARRaycastManager m_RaycastManager;

    [SerializeField, Tooltip("The behavior to use to spawn objects.")]
    public ObjectSpawner m_ObjectSpawner;

    /// <summary>
    /// The behavior to use to spawn objects.
    /// </summary>
    public ObjectSpawner objectSpawner
    {
        get => m_ObjectSpawner;
        set => m_ObjectSpawner = value;
    }

    [SerializeField,
     Tooltip(
         "Whether to require that the AR Interactor hits an AR Plane with a horizontal up alignment in order to spawn anything.")]
    bool m_RequireHorizontalUpSurface;

    /// <summary>
    /// Whether to require that the <see cref="XRRayInteractor"/> hits an <see cref="ARPlane"/> with an alignment of
    /// <see cref="PlaneAlignment.HorizontalUp"/> in order to spawn anything.
    /// </summary>
    public bool requireHorizontalUpSurface
    {
        get => m_RequireHorizontalUpSurface;
        set => m_RequireHorizontalUpSurface = value;
    }

    [SerializeField, Tooltip("The action to use to trigger spawn. (Button Control)")]
    InputActionProperty m_SpawnAction = new(new InputAction(type: InputActionType.Button));

    [SerializeField, Tooltip("The action to get touch position.")]
    InputActionProperty m_TouchPositionAction = new(new InputAction(type: InputActionType.Value));

    /// <summary>
    /// The Input System action to use to trigger spawn.
    /// </summary>
    public InputActionProperty spawnAction
    {
        get => m_SpawnAction;
        set
        {
            if (Application.isPlaying)
                m_SpawnAction.DisableDirectAction();

            m_SpawnAction = value;

            if (Application.isPlaying && isActiveAndEnabled)
                m_SpawnAction.EnableDirectAction();
        }
    }

    public InputActionProperty touchPositionAction
    {
        get => m_TouchPositionAction;
        set
        {
            if (Application.isPlaying)
                m_TouchPositionAction.DisableDirectAction();

            m_TouchPositionAction = value;

            if (Application.isPlaying && isActiveAndEnabled)
                m_TouchPositionAction.EnableDirectAction();
        }
    }
    
    [SerializeField] private UnityEvent m_OnObjectSpawned = new UnityEvent();

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    void Start()
    {
        if (m_ObjectSpawner == null)
            m_ObjectSpawner = FindObjectOfType<ObjectSpawner>();

        if (m_RaycastManager == null)
            m_RaycastManager = FindObjectOfType<ARRaycastManager>();
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    void OnEnable()
    {
        m_SpawnAction.EnableDirectAction();
        m_TouchPositionAction.EnableDirectAction();

        m_SpawnAction.action.performed += OnSpawnActionPerformed;
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    void OnDisable()
    {
        m_SpawnAction.DisableDirectAction();
        m_TouchPositionAction.DisableDirectAction();

        m_SpawnAction.action.performed -= OnSpawnActionPerformed;
    }

    private void OnSpawnActionPerformed(InputAction.CallbackContext context)
    {
        touchPosition = m_TouchPositionAction.action.ReadValue<Vector2>();
        Debug.Log($"Spawning at touch position: {touchPosition}");
        SpawnObject();
    }

    private void SpawnObject()
    {
        if (!m_ObjectSpawner) return;

        Debug.Log("Spawning object");
        var m_Hits = new List<ARRaycastHit>();
        if (m_RaycastManager.Raycast(touchPosition, m_Hits))
        {
            Debug.Log("Raycast hit");
            var arRaycastHit = m_Hits[0];
            Debug.Log($"Raycast hit position: {arRaycastHit.pose.position}");

            if (setActiveInsteadOfInstantiate)
            {
                foreach (var VARIABLE in objectSpawner.ObjectPrefabs)
                {
                    VARIABLE.SetActive(true);
                    VARIABLE.transform.position = arRaycastHit.pose.position;
                    m_OnObjectSpawned.Invoke();
                }
                return;
            }
            
            if (m_ObjectSpawner.TrySpawnObject(arRaycastHit.pose.position, arRaycastHit.pose.up, out var spawnedObject))
            {
                Debug.Log("Object spawned successfully");
                m_OnObjectSpawned.Invoke();
            }
            else
            {
                Debug.Log("Failed to spawn object");
            }
        }
    }
}

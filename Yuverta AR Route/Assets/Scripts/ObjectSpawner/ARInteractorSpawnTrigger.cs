using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Object = UnityEngine.Object;

/// <summary>
/// Spawns an object at an <see cref="XRRayInteractor"/>'s raycast hit position when a trigger is activated.
/// </summary>
public class ARInteractorSpawnTrigger : MonoBehaviour
{
    private bool attemptSpawn = false;
    private Vector2 touchPosition;
    [SerializeField] private LayerMask m_LayerMask = -1;

    [SerializeField, Tooltip("The AR Interactor that determines where to spawn the object.")]
    ARRaycastManager m_RaycastManager;

    /// <summary>
    /// The <see cref="XRRayInteractor"/> that determines where to spawn the object.
    /// </summary>
    public ARRaycastManager arInteractorObject
    {
        get => m_RaycastManager;
        set => m_RaycastManager = value;
    }

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

    bool m_EverHadSelection;

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
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    void OnDisable()
    {
        m_SpawnAction.DisableDirectAction();
        m_TouchPositionAction.DisableDirectAction();
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    private void Update()
    {
        if (m_SpawnAction.action.WasPerformedThisFrame())
        {
            touchPosition = m_TouchPositionAction.action.ReadValue<Vector2>();
            attemptSpawn = true;
            Debug.Log("Spawning");
        }

        if (!attemptSpawn)
            return;

        attemptSpawn = SpawnObject();
    }

    private bool SpawnObject()
    {
        if (!m_ObjectSpawner) return true;

        Debug.Log("Spawning object");
        var m_Hits = new List<ARRaycastHit>();
        if (m_RaycastManager.Raycast(touchPosition, m_Hits))
        {
            if (m_Hits.Count == 0)
                return true;
            
            var arRaycastHit = m_Hits[0];
            
            return !m_ObjectSpawner.TrySpawnObject(arRaycastHit.pose.position, arRaycastHit.pose.up, out var spawnedObject);
        }

        return true;
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
        // Enable touch input
        // No direct equivalent to InputActionProperty.EnableDirectAction() needed
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    void OnDisable()
    {
        // Disable touch input
        // No direct equivalent to InputActionProperty.DisableDirectAction() needed
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                case TouchPhase.Moved:
                    touchPosition = touch.position;
                    break;
                case TouchPhase.Ended:
                    OnSpawnActionPerformed();
                    break;
            }
        }
    }

    private void OnSpawnActionPerformed()
    {
        SpawnObject();
    }

    private void SpawnObject()
    {
        if (!m_ObjectSpawner) return;

        var m_Hits = new List<ARRaycastHit>();
        if (m_RaycastManager.Raycast(touchPosition, m_Hits))
        {
            var arRaycastHit = m_Hits[0];

            if (setActiveInsteadOfInstantiate)
            {
                if (m_ObjectSpawner.TryEnableObject(arRaycastHit.pose.position, arRaycastHit.pose.up))
                {
                    m_OnObjectSpawned.Invoke();
                }
                else
                {
                    Debug.Log("Failed to enable object");
                }
                return;
            }

            if (m_ObjectSpawner.TrySpawnObject(arRaycastHit.pose.position, arRaycastHit.pose.up, out var spawnedObject))
            {
                m_OnObjectSpawned.Invoke();
            }
            else
            {
                Debug.Log("Failed to spawn object");
            }
        }
    }
}

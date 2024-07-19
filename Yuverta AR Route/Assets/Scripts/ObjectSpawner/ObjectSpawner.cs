using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

/// <summary>
/// Behavior with an API for spawning objects from a given set of prefabs.
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The camera that objects will face when spawned. If not set, defaults to the main camera.")]
    Camera m_CameraToFace;

    /// <summary>
    /// The camera that objects will face when spawned. If not set, defaults to the <see cref="Camera.main"/> camera.
    /// </summary>
    public Camera cameraToFace
    {
        get
        {
            EnsureFacingCamera();
            return m_CameraToFace;
        }
        set => m_CameraToFace = value;
    }
    
    [Tooltip("Object Spawns with its rotation set to Quaternion.identity")]
    [SerializeField] private bool m_SpawnWithIdentityRotation = false;

    [FormerlySerializedAs("m_ObjectPrefab")] [SerializeField] [Tooltip("The list of prefabs available to spawn.")]
    public List<GameObject> m_ObjectPrefabs = new ();

    /// <summary>
    /// The list of prefabs available to spawn.
    /// </summary>
    public List<GameObject> ObjectPrefabs
    {
        get => m_ObjectPrefabs;
        set => m_ObjectPrefabs = value;
    }

    [SerializeField] [Tooltip("Whether to only spawn an object if the spawn point is within view of the camera.")]
    bool m_OnlySpawnInView = true;

    /// <summary>
    /// Whether to only spawn an object if the spawn point is within view of the <see cref="cameraToFace"/>.
    /// </summary>
    public bool OnlySpawnInView
    {
        get => m_OnlySpawnInView;
        set => m_OnlySpawnInView = value;
    }

    [SerializeField]
    [Tooltip(
        "The size, in viewport units, of the periphery inside the viewport that will not be considered in view.")]
    float m_ViewportPeriphery = 0.15f;

    /// <summary>
    /// The size, in viewport units, of the periphery inside the viewport that will not be considered in view.
    /// </summary>
    public float ViewportPeriphery
    {
        get => m_ViewportPeriphery;
        set => m_ViewportPeriphery = value;
    }

    [SerializeField]
    [Tooltip("When enabled, the object will be rotated about the y-axis when spawned by Spawn Angle Range, " +
             "in relation to the direction of the spawn point to the camera.")]
    bool m_ApplyRandomAngleAtSpawn = true;

    /// <summary>
    /// When enabled, the object will be rotated about the y-axis when spawned by <see cref="SpawnAngleRange"/>
    /// in relation to the direction of the spawn point to the camera.
    /// </summary>
    public bool ApplyRandomAngleAtSpawn
    {
        get => m_ApplyRandomAngleAtSpawn;
        set => m_ApplyRandomAngleAtSpawn = value;
    }

    [SerializeField]
    [Tooltip("The range in degrees that the object will randomly be rotated about the y axis when spawned, " +
             "in relation to the direction of the spawn point to the camera.")]
    float m_SpawnAngleRange = 45f;


    /// <summary>
    /// The range in degrees that the object will randomly be rotated about the y axis when spawned, in relation
    /// to the direction of the spawn point to the camera.
    /// </summary>
    public float SpawnAngleRange
    {
        get => m_SpawnAngleRange;
        set => m_SpawnAngleRange = value;
    }
    
    [SerializeField][Tooltip("Rather or not to spawn the object at a fixed lerp between the camera Y position and the spawn point Y position /n Uses the spawn height offset as the lerp value.")]
    private bool m_LerpToSpawnHeight = false;

    [SerializeField] [Tooltip("The height the object goes off the surface of the plane the raycast has hit")]
    float m_SpawnHeightOffset = 1.5f;

    public float spawnHeightOffset
    {
        get => m_SpawnHeightOffset;
        set => m_SpawnHeightOffset = value;
    }

    [SerializeField] [Tooltip("Whether to spawn each object as a child of this object.")]
    private bool mSpawnAsChild;

    /// <summary>
    /// Whether to spawn each object as a child of this object.
    /// </summary>
    public bool SpawnAsChild
    {
        get => mSpawnAsChild;
        set => mSpawnAsChild = value;
    }

    /// <summary>
    /// Event invoked after an object is spawned.
    /// </summary>
    public GameObjectEvent ObjectSpawned = new();

    private Camera _mainCamera;

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    private void Awake()
    {
        _mainCamera = Camera.main;

        EnsureFacingCamera();
    }

    private void EnsureFacingCamera()
    {
        if (!m_CameraToFace)
            m_CameraToFace = _mainCamera;
    }

    /// <summary>
    /// Attempts to spawn an object from <see cref="ObjectPrefabs"/> at the given position. The object will have a
    /// yaw rotation that faces <see cref="cameraToFace"/>, plus or minus a random angle within <see cref="SpawnAngleRange"/>.
    /// </summary>
    /// <param name="spawnPoint">The world space position at which to spawn the object.</param>
    /// <param name="spawnNormal">The world space normal of the spawn surface.</param>
    /// <returns>Returns <see langword="true"/> if the spawner successfully spawned an object. Otherwise returns
    /// <see langword="false"/>, for instance if the spawn point is out of view of the camera.</returns>
    /// <remarks>
    /// The object selected to spawn is based on <see cref="SpawnOptionIndex"/>. If the index is outside
    /// the range of <see cref="ObjectPrefabs"/>, this method will select a random prefab from the list to spawn.
    /// Otherwise, it will spawn the prefab at the index.
    /// </remarks>
    /// <seealso cref="objectSpawned"/>
    public bool TrySpawnObject(Vector3 spawnPoint, Vector3 spawnNormal, out GameObject objectSpawned)
{

    if (m_OnlySpawnInView)
    {
        var inViewMin = m_ViewportPeriphery;
        var inViewMax = 1f - m_ViewportPeriphery;
        var pointInViewportSpace = cameraToFace.WorldToViewportPoint(spawnPoint);


        if (pointInViewportSpace.z < 0f || pointInViewportSpace.x > inViewMax ||
            pointInViewportSpace.x < inViewMin ||
            pointInViewportSpace.y > inViewMax || pointInViewportSpace.y < inViewMin)
        {
            Debug.Log("Spawn point is outside the view.");
            objectSpawned = null;
            return false;
        }
    }

    objectSpawned = null;

    foreach (var objPrefab in m_ObjectPrefabs)
    {

        var newObject = Instantiate(objPrefab);

        if (mSpawnAsChild)
        {
            newObject.transform.parent = transform;
        }

        // Set the initial position and add the GridSnapper component
        newObject.transform.position = spawnPoint;

        EnsureFacingCamera();

        var facePosition = m_CameraToFace.transform.position;
        var forward = facePosition - spawnPoint;

        BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
        
        
        newObject.transform.rotation = m_SpawnWithIdentityRotation ? Quaternion.identity : Quaternion.LookRotation(projectedForward, spawnNormal);

        newObject.transform.position += spawnNormal * (m_LerpToSpawnHeight ? Mathf.Lerp(m_CameraToFace.transform.position.y, spawnPoint.y, m_SpawnHeightOffset) : m_SpawnHeightOffset);

        if (m_ApplyRandomAngleAtSpawn)
        {
            var randomRotation = Random.Range(-m_SpawnAngleRange, m_SpawnAngleRange);
            newObject.transform.Rotate(Vector3.up, randomRotation);
        }

        ObjectSpawned.Invoke(newObject);

        objectSpawned = newObject;
    }

    return true;
}
    public bool TryEnableObject(Vector3 spawnPoint, Vector3 spawnNormal)
    {
        foreach (var objPrefab in m_ObjectPrefabs)
        {
            if (mSpawnAsChild)
            {
                objPrefab.transform.parent = transform;
            }

            // Set the initial position and add the GridSnapper component
            objPrefab.transform.position = spawnPoint;

            EnsureFacingCamera();

            var facePosition = m_CameraToFace.transform.position;
            var forward = facePosition - spawnPoint;

            BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
        
        
            objPrefab.transform.rotation = m_SpawnWithIdentityRotation ? Quaternion.identity : Quaternion.LookRotation(projectedForward, spawnNormal);

            objPrefab.transform.position += spawnNormal * m_SpawnHeightOffset;

            if (m_ApplyRandomAngleAtSpawn)
            {
                var randomRotation = Random.Range(-m_SpawnAngleRange, m_SpawnAngleRange);
                objPrefab.transform.Rotate(Vector3.up, randomRotation);
            }
            
            objPrefab.SetActive(true);
        }

        return true;
    }
}
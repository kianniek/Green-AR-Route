using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTracking : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;
    public GameObject prefabToSpawn;

    private void OnEnable()
    {
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            SpawnObject(trackedImage);
        }
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateObject(trackedImage);
        }
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            RemoveObject(trackedImage);
        }
    }

    private void SpawnObject(ARTrackedImage trackedImage)
    {
        GameObject spawnedObject = Instantiate(prefabToSpawn, trackedImage.transform.position, trackedImage.transform.rotation);
        spawnedObject.name = trackedImage.referenceImage.name;
        DisplayVideoHandler.Instance.VideoPlayerSpawned(spawnedObject);
    }

    private void UpdateObject(ARTrackedImage trackedImage)
    {
        GameObject existingObject = GameObject.Find(trackedImage.referenceImage.name);
        if (existingObject)
        {
            existingObject.transform.position = trackedImage.transform.position;
            existingObject.transform.rotation = trackedImage.transform.rotation;
        }
    }

    private void RemoveObject(ARTrackedImage trackedImage)
    {
        GameObject existingObject = GameObject.Find(trackedImage.referenceImage.name);
        if (existingObject)
        {
            Destroy(existingObject);
        }
    }
}
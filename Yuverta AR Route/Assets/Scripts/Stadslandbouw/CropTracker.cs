using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CropTracker : MonoBehaviour
{
    [Tooltip("This list contains the spawn locations for the seeds the crop tracker spawns")]
    public Transform[] seedsSpawnLocations;

    [Tooltip("The list of crops that are actually used in Crop Rotation.")]
    [SerializeField]
    private List<CropObject> rightCrops;

    [FormerlySerializedAs("wrongCrops")]
    [Tooltip("The list of crops that are not used in Crop Rotation and purely here to distract the user.")]
    [SerializeField]
    private List<CropObject> distractCrops;

    /// <summary>
    /// The list of all crops that are used
    /// </summary>
    private List<CropObject> allCrops;

    private CropObject lastPlacedCrop;
    private CropObject.CropType nextCorrectCropType;

    /// <summary>
    /// The crop container that this script is attached to.
    /// </summary>
    [SerializeField] private CropContainer cropContainer;

    [SerializeField] private CropScript cropScript;

    [SerializeField] private CropGrowthSystem cropGrowthSystem;

    private void Start()
    {
        // Adding right crops and distract crops to the all crops list
        allCrops = new List<CropObject>();
        allCrops.AddRange(rightCrops);
        allCrops.AddRange(distractCrops);
        // Randomizing the list of all crops
        allCrops = RandomizeList(allCrops);

        // Initialize a fully grown crop the first time
        InitializeFullyGrownCrop();

    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                var ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.collider.CompareTag("Seed"))
                    {
                        PickedSeed(hit.collider.name);
                    }
                }
            }
        }
    }

    public void InitializeFullyGrownCrop()
    {
        // Pick a random crop from the right crops list
        var randomCrop = rightCrops[Random.Range(0, rightCrops.Count)];

        cropScript.FullyGrowCrop(randomCrop);

        Debug.Log("Fully grown crop initialized");

        // Spawning the new Crop on the crop container
        cropContainer.NewCrop(cropScript, CropObject.CropType.none);

        lastPlacedCrop = randomCrop;
        nextCorrectCropType = randomCrop.nextCrop;

        cropGrowthSystem.DisableButtons();
    }


    private void PickedSeed(string cropName)
    {
        // Find the picked seed
        var crop = allCrops.Find(crop => crop.cropName == cropName);
        Debug.Log("Picked seed: " + cropName);

        cropScript.NewCrop(crop);

        // Spawning the new Crop on the crop container
        cropContainer.NewCrop(cropScript, nextCorrectCropType);

        // Clearing the seed list from seedsSpawnLocations children
        foreach (var seeds in seedsSpawnLocations)
        {
            foreach (Transform seed in seeds)
            {
                Destroy(seed.gameObject);
            }
        }

        lastPlacedCrop = crop;
        nextCorrectCropType = crop.nextCrop;
    }

    public void NewRound()
    {
        // Getting the new seed list
        var seedList = NewSeedList();

        for (var i = 0; i < seedsSpawnLocations.Length; i++)
        {
            var prefab = seedList[i].seedPrefab;
            var parent = seedsSpawnLocations[i];
            var seed = Instantiate(prefab, parent);
            seed.name = seedList[i].cropName;
            seed.tag = "Seed";
        }
    }

    private List<CropObject> NewSeedList()
    {
        // List to hold the new crops to spawn
        var newCrops = new List<CropObject>();

        // Get the correct crop based on the last placed crop
        CropObject nextCorrectCrop = allCrops.Find(crop => lastPlacedCrop.nextCrop == crop.currentCropType);

        // Always add the correct crop first
        newCrops.Add(nextCorrectCrop);

        // Add distract crops until we reach the required number of seeds
        int cropsToAdd = seedsSpawnLocations.Length - 1; // Subtract 1 because we already added the correct crop

        for (int i = 0; i < cropsToAdd; i++)
        {
            CropObject randomCrop = GetRandomCrop(nextCorrectCrop);
            newCrops.Add(randomCrop);
        }

        // Randomize the order of the seeds before returning
        return RandomizeList(newCrops);
    }


    private CropObject GetRandomCrop(CropObject cropToExclude = null)
    {
        var cropObject = allCrops[Random.Range(0, allCrops.Count)];

        // Try to find a crop that is not used and not the excluded one
        var availableCrops = new List<CropObject>(allCrops);
        if (cropToExclude != null)
        {
            if (availableCrops.Contains(cropToExclude))
            {
                availableCrops.Remove(cropToExclude);
            }
        }

        if (availableCrops.Count <= 0)
            return cropObject;

        var randomIndex = Random.Range(0, availableCrops.Count);

        return availableCrops[randomIndex];
    }

    private CropObject GetRandomCrop(CropObject[] cropToExclude)
    {
        var cropObject = allCrops[Random.Range(0, allCrops.Count)];

        // Try to find a crop that is not used and not the excluded one
        var availableCrops = new List<CropObject>(allCrops);
        if (cropToExclude != null)
        {
            foreach (var crop in cropToExclude)
            {
                if (availableCrops.Contains(crop))
                {
                    availableCrops.Remove(crop);
                }
            }
        }

        if (availableCrops.Count <= 0)
            return cropObject;

        var randomIndex = Random.Range(0, availableCrops.Count);

        return availableCrops[randomIndex];
    }

    private static List<T> RandomizeList<T>(List<T> inputList)
    {
        var randomList = new List<T>();

        while (inputList.Count > 0)
        {
            var randomIndex = Random.Range(0, inputList.Count);
            randomList.Add(inputList[randomIndex]);
            inputList.RemoveAt(randomIndex);
        }

        return randomList;
    }
}

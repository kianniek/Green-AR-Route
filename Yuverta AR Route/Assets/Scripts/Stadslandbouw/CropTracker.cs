using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Local

public class CropTracker : MonoBehaviour
{
    [Tooltip("This list contains the spawn locations for the seeds the crop tracker spawns")]
    public List<Transform> seedsSpawnLocations; 

    [Tooltip("The list of crops that are actually used in Crop Rotation.")] [SerializeField]
    private List<CropObject> rightCrops;

    [FormerlySerializedAs("wrongCrops")]
    [Tooltip("The list of crops that are not used in Crop Rotation and purely here to distract the user.")]
    [SerializeField]
    private List<CropObject> distractCrops;

    /// <summary>
    /// The list of all crops that are used in Crop Rotation.
    /// </summary>
    private List<CropObject> allCrops;

    private CropObject lastPlacedCrop;
    private CropObject.CropType nextCorrectCropType;

    /// <summary>
    /// The crop container that this script is attached to.
    /// </summary>
    private CropContainer cropContainer;

    [Tooltip("The input that will let the user pick a seed.")] [SerializeField]
    private InputActionReference pickSeedTouch;

    private void Start()
    {
        //Initializing variables
        cropContainer = GetComponent<CropContainer>();

        //Adding right crops and distract crops to the all crops list
        allCrops = new List<CropObject>();
        allCrops.AddRange(rightCrops);
        allCrops.AddRange(distractCrops);
        //Randomizing the list of all crops
        allCrops = RandomizeList(allCrops);

        //Starting the first round
        NewRound();

        /*//Setting what should happen on the input action
        pickSeedTouch.action.performed += OnPickedSeedTouch;*/
    }

    private void OnEnable()
    {
        pickSeedTouch.action.Enable();
        pickSeedTouch.action.started += OnPickedSeedTouch;
    }

    private void OnDisable()
    {
        pickSeedTouch.action.started -= OnPickedSeedTouch;
        pickSeedTouch.action.Disable();
    }

    private void OnPickedSeedTouch(InputAction.CallbackContext ctx)
    {
        var touch = ctx.ReadValue<Vector2>();
        var ray = Camera.main.ScreenPointToRay(touch);
        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.collider.CompareTag("Seed"))
            {
                PickedSeed(hit.collider.name);
            }
        }
    }

    private void PickedSeed(string cropName)
    {
        //Find the picked seed
        var crop = allCrops.Find(crop => crop.cropName == cropName);
        Debug.Log("Picked seed: " + cropName);

        var cropScript = cropContainer.CropSpawnLocation.gameObject.GetComponent<CropScript>();
        cropScript.NewCrop(crop);

        //Spawning the new Crop on the crop container
        cropContainer.NewCrop(cropScript, nextCorrectCropType);

        //Clearing the seed list from seedsSpawnLocations children
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
        //Getting the new seed list
        var seedList = NewSeedList();

        for (var i = 0; i < seedList.Count; i++)
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
        //Getting new seeds minus one
        var newCrops = new List<CropObject>();

        if (lastPlacedCrop == null)
        {
            //Adding the wrong seeds to all except one correct seed
            for (var i = 0; i < seedsSpawnLocations.Count; i++)
            {
                newCrops.Add(GetRandomCrop(newCrops.ToArray()));
            }

            return RandomizeList(newCrops);
        }

        //The last one is added her to make sure there is always one correct seed
        CropObject nextCorrectCrop;

        do
        {
            nextCorrectCrop = allCrops.Find(crop => lastPlacedCrop.nextCrop == crop.currentCropType);
        } while (nextCorrectCrop == null);

        newCrops.Add(nextCorrectCrop);

        //Adding the wrong seeds to all except one correct seed
        for (var i = 0; i < seedsSpawnLocations.Count - 1; i++)
        {
            newCrops.Add(GetRandomCrop(nextCorrectCrop));
        }
        

        //Randomizing the list of seeds before returning it
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
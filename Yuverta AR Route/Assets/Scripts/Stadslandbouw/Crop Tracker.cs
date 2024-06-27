using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Local

public class CropTracker : MonoBehaviour
{
    [Header("Crops")]
    [Tooltip("The amount of seeds that the user can choose from.")]
    [SerializeField] private int seedCount;
    [Tooltip("The list of crops that are actually used in Crop Rotation.")]
    [SerializeField] private List<CropObject> rightCrops;
    [Tooltip("The list of crops that are not used in Crop Rotation and purely here to distract the user.")]
    [SerializeField] private List<CropObject> wrongCrops;
    
    /// <summary>
    /// The list of all crops that are used in Crop Rotation.
    /// </summary>
    private List<CropObject> allCrops;
    
    /// <summary>
    /// The crops that are already used are saved here to prevent duplicates.
    /// </summary>
    private List<CropObject> usedCrops;
    
    /// <summary>
    /// The last correct crop object that was planted is saved here to determine the next correct crop.
    /// </summary>
    private CropObject lastCorrectCropObject;
    
    /// <summary>
    /// The last seeds that where spawned are saved here to make sure they can be found when the player hits a seed.
    /// </summary>
    private List<CropObject> lastSeedList;
    
    /// <summary>
    /// The crop container that this script is attached to.
    /// </summary>
    private CropContainer cropContainer;
    
    [Tooltip("The input that will let the user pick a seed.")]
    [SerializeField] private InputActionReference pickSeedTouch;
    
    private void Start()
    {
        //Initializing variables
        cropContainer = GetComponent<CropContainer>();
        
        //Randomizing the list of crops
        allCrops = new List<CropObject>();
        allCrops.AddRange(rightCrops);
        allCrops.AddRange(wrongCrops);
        allCrops = RandomizeList(allCrops);
        
        usedCrops = new List<CropObject>();
        
        //Starting the first round
        NewRound();
        
        //Setting what should happen on the input action
        pickSeedTouch.action.performed += ctx =>
        {
            var touch = ctx.ReadValue<Vector2>();
            var ray = Camera.main.ScreenPointToRay(touch);
            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.collider.CompareTag("Seed"))
                {
                    PickedSeed(hit.collider.name);
                    pickSeedTouch.action.Disable();
                }
            }
        };
    }

    private void PickedSeed(string cropName)
    {
        //Find the picked seed
        var crop = lastSeedList.Find(crop => crop.cropName == cropName);
        
        //Check if the seed is the correct one
        if (crop.cropType == lastCorrectCropObject.nextCrop) lastCorrectCropObject = crop;
        
        //Instantiating a new GameObject and adding a CropScript to it
        var cropScript = Instantiate(new GameObject(), cropContainer.cropSpawnLocation).AddComponent<CropScript>();
        cropScript.cropObject = crop;
        //Spawning the new Crop on the crop container
        cropContainer.NewCrop(cropScript);

        //Clearing the seed list
        foreach (var seeds in lastSeedList)
        {
            Destroy(seeds);
        }
        lastSeedList.Clear();
    }
    
    public void NewRound()
    {
        List<CropObject> seeds = NewSeedList();
        for (int i = 0; i < seeds.Count; i++)
        {
            var seed = Instantiate(seeds[i].seedPrefab, cropContainer.seedsSpawnLocations[i]);
            seed.name = seeds[i].cropName;
        }
        lastSeedList = seeds;
        pickSeedTouch.action.Enable();
    }

    private List<CropObject> NewSeedList()
    {
        //Getting new seeds minus one
        List<CropObject> newCrops = new List<CropObject>();
        for (int i = 0; i < seedCount - 1; i++)
        {
            newCrops.Add(GetRandomCrop());
        }
        
        //The last one is added her to make sure there is always one correct seed
        newCrops.Add(lastCorrectCropObject
            ? rightCrops.Find(crop => crop.cropType == lastCorrectCropObject.nextCrop)
            : rightCrops[Random.Range(0, rightCrops.Count)]);
        
        //Setting the new correct crop object
        lastCorrectCropObject = newCrops[^1];
        //Randomizing the list of seeds before returning it
        return RandomizeList(newCrops);
    }
    
    private CropObject GetRandomCrop()
    {
        //If all crops are used randomize the list again
        if (usedCrops.Count == allCrops.Count)
        {
            allCrops.AddRange(RandomizeList(usedCrops));
            usedCrops.Clear();
        }
        CropObject cropObject = allCrops[Random.Range(0, allCrops.Count)];
        allCrops.Remove(cropObject);
        
        usedCrops.Add(cropObject);
        return cropObject;
    }
    
    private static List<T> RandomizeList<T>(List<T> inputList)
    {
        // Create a new list to hold the randomized elements
        List<T> randomizedList = new List<T>(inputList);
        
        // Create a Random object
        System.Random rng = new System.Random();
        
        // Loop through the list in reverse order
        int n = randomizedList.Count;
        while (n > 1)
        {
            n--;
            // Select a random element from the remaining unshuffled portion
            int k = rng.Next(n + 1);
            // Swap the selected element with the last unshuffled element
            (randomizedList[k], randomizedList[n]) = (randomizedList[n], randomizedList[k]);
        }

        return randomizedList;
    }
}

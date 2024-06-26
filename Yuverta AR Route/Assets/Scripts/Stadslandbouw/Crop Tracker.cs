using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropTracker : MonoBehaviour
{
    [Header("Crops")]
    [SerializeField] private GameObject cropPrefab;
    [SerializeField] private int cropCount;
    [SerializeField] private List<CropObject> rightCrops;
    [SerializeField] private List<CropObject> wrongCrops;
    
    //Private lists used for distributing the crops
    private List<CropObject> allCrops;
    private List<CropObject> usedCrops;
    
    //Saving the last placed crops
    private CropObject lastCorrectCropObject;
    private List<CropObject> lastSeedList;
    
    private CropContainer cropContainer;
    
    private void Start()
    {
        cropContainer = GetComponent<CropContainer>();
        
        allCrops = new List<CropObject>();
        allCrops.AddRange(rightCrops);
        allCrops.AddRange(wrongCrops);
        allCrops = RandomizeList(allCrops);
        
        usedCrops = new List<CropObject>();
        
        NewRound();
    }

    public void PickedSeed(string cropName)
    {
        var crop = lastSeedList.Find(crop => crop.cropName == cropName);
        
        if (crop == lastCorrectCropObject) lastCorrectCropObject = crop;
        
        var cropScript = Instantiate(cropPrefab, cropContainer.cropSpawnLocation).GetComponent<CropScript>();
        cropScript.cropObject = crop;
        cropContainer.NewCrop(cropScript);
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
    }

    private List<CropObject> NewSeedList()
    {
        List<CropObject> newCrops = new List<CropObject>();
        for (int i = 0; i < cropCount - 1; i++)
        {
            newCrops.Add(GetRandomCrop());
        }
        
        if (lastCorrectCropObject) newCrops.Add(rightCrops.Find(crop => crop.nextCrop == lastCorrectCropObject.cropName));
        else newCrops.Add(rightCrops[UnityEngine.Random.Range(0, rightCrops.Count)]);
        lastCorrectCropObject = newCrops[^1];
        return RandomizeList(newCrops);
    }
    
    private CropObject GetRandomCrop()
    {
        if (usedCrops.Count == allCrops.Count)
        {
            allCrops.AddRange(RandomizeList(usedCrops));
            usedCrops.Clear();
        }
        CropObject cropObject = allCrops[UnityEngine.Random.Range(0, allCrops.Count)];
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

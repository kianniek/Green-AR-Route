using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable UnassignedField.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global

[CreateAssetMenu]
public class CropObject : ScriptableObject
{
    public enum CropType
    {
        none, // None
        Legumes, // Peas, beans, lentils, etc.
        Brassicas, // Cabbage, broccoli, cauliflower, etc.
        SolanaceousCrops, // Tomatoes, potatoes, eggplants, peppers
        Cucurbits, // Cucumbers, pumpkins, zucchinis, melons
        GrainsOrCoverCrops, // Wheat, barley, oats, rye, or cover crops like clover
        RootCrops, // Carrots, beets, radishes
        Alliums, // Onions, garlic, leeks, shallots
        LeafyGreens, // Lettuce, spinach, kale, chard
        Herbs, // Basil, thyme, parsley, rosemary
        Tubers, // Potatoes, sweet potatoes

    }
    
    public GameObject seedPrefab;
    public GameObject cropPrefab;
    public CropType currentCropType;
    public CropType nextCrop;
    public string cropName;
}

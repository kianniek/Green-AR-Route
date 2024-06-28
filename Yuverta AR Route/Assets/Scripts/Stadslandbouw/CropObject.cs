using System.Collections.Generic;
using UnityEngine;
// ReSharper disable UnassignedField.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global

[CreateAssetMenu]
public class CropObject : ScriptableObject
{
    public enum CropType
    {
        Legumes,
        RootCrops,
        LeafyGreens,
        FruitsOrVegetables,
        Grains,
        FallowOrCoverCrops
    }
    
    public GameObject seedPrefab;
    public List<GameObject> growthStages;
    public CropType cropType;
    public CropType nextCrop;
    public string cropName;
}

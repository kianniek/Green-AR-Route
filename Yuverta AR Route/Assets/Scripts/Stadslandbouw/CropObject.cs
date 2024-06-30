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
        none,
        Legumes,
        Brassicas,
        SolanaceousCrops,
        Cucurbits,
        GrainsOrCoverCrops
    }
    
    public GameObject seedPrefab;
    public GameObject cropPrefab;
    public CropType currentCropType;
    public CropType nextCrop;
    public string cropName;
}

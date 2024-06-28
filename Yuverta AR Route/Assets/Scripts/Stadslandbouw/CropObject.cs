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
        Brassicas,
        SolanaceousCrops,
        Cucurbits,
        GrainsOrCoverCrops
    }
    
    public GameObject seedPrefab;
    public GameObject cropPrefab;
    public CropType cropType;
    public CropType nextCrop;
    public string cropName;
}

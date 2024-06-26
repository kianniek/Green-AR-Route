using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CropObject : ScriptableObject
{
    public GameObject seedPrefab;
    public List<GameObject> growthStages;
    public string cropName;
    public string nextCrop;
}

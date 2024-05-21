using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BlockData : MonoBehaviour
{
    [SerializeField] 
    private Image uiDisplayImage;
    public Image UIDisplayImage
    {
        get { return uiDisplayImage; }
        set { uiDisplayImage = value; }

    }
    
    [SerializeField]
    private string blockName;
    public string BlockName
    {
        get { return blockName; }
        set { blockName = value; }

    }
    
    [SerializeField]
    private int floorSpawnLevel;
    public int FloorSpawnLevel
    {
        get { return floorSpawnLevel; }
        set { floorSpawnLevel = value; }

    }
    
    [SerializeField]
    private GameObject blockToSpawn;
    public GameObject BlockToSpawn
    {
        get { return blockToSpawn; }
        set { blockToSpawn = value; }

    }
    public int index;
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class UIMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject UIObjectParent;
    
    [SerializeField]
    private GameObject UIObjectPrefab;
    
    [SerializeField]
    private SerializableDictionary<string, Sprite> UIObjectImages;
    
    private List<GameObject> UIObjects = new List<GameObject>();

    private void Start()
    {
        if (UIObjectParent.transform.childCount <= 0) return;
        foreach (Transform child in UIObjectParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void StartUp(List<GameObject> prefabList)
    {
        foreach (var prefab in prefabList)
        {
            Add(prefab);
        }
    }

    public void Add(GameObject prefab)
    {
        if (CheckName(prefab.name) || prefab == null) return;
        var newUIObject = Instantiate(UIObjectPrefab, UIObjectParent.transform);
        newUIObject.name = prefab.name;
        //newUIObject.GetComponent<Image>().sprite = UIObjectImages.GetValue(prefab.name);
        newUIObject.GetComponent<Button>(). onClick.AddListener(() => OnButtonClick(newUIObject.name));
        UIObjects.Add(newUIObject);
    }

    //Index is not used in this function but is required for the event
    public void Remove(GameObject prefab)
    {
        var prefabName = prefab.name;
        if (prefabName.Contains("(Clone)"))
            prefabName = prefabName.Replace("(Clone)", "");
        
        var objToRemove = UIObjects.Find(obj => obj.name == prefabName);
        UIObjects.Remove(objToRemove);
        Destroy(objToRemove);
    }

    private void OnButtonClick(string name)
    {
        Debug.Log(name);
        GridManager.Instance.objectSpawner.m_SpawnOptionName = name;
    }

    private bool CheckName(string newName)
    {
        if (UIObjects.Any(obj => obj.name == newName))
        {
            return false;
        }
        
        return false;
    }
}

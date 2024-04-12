using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollArea : MonoBehaviour
{
    [SerializeField] private SerializableDictionary<Sprite, string> spawnPrefabsDictionary;
    [SerializeField] private GameObject contentParent;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private GameObject uiPrefab;
    private List<GameObject> uiClickAbles = new List<GameObject>();
    private GameObject selectedClickAble;
    
    // Start is called before the first frame update
    void Start()
    {
        scrollbar.value = 1;

        if (contentParent.transform.childCount > 0)
        {
            for (int i = 0; i < contentParent.transform.childCount; i++)
            {
                Destroy(contentParent.transform.GetChild(i).gameObject);
            }
        }
        
        AddPrefabs();
    }

    private void AddPrefabs()
    {
        foreach (var image in spawnPrefabsDictionary.keys)
        {
            GameObject newClick = Instantiate(uiPrefab, contentParent.transform);
            newClick.GetComponent<Image>().sprite = image;
            uiClickAbles.Add(newClick);
        }
    }
}

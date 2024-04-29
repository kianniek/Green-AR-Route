using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollArea : MonoBehaviour
{
    [SerializeField] private SerializableDictionary<Sprite, GameObject> spawnPrefabsDictionary;
    [SerializeField] private GameObject contentParent;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private GameObject uiPrefab;
    private List<GameObject> uiClickAbles = new List<GameObject>();
    private List<GameObject> prefabs = new List<GameObject>();
    private GameObject selectedClickAble;
    private int selectedIndex;

    private UnityAction onSelect;
    
    // Start is called before the first frame update
    void Start()
    {
        onSelect = () => OnSelect();
        scrollbar.value = 1;

        if (contentParent.transform.childCount > 0)
        {
            for (int i = 0; i < contentParent.transform.childCount; i++)
            {
                Destroy(contentParent.transform.GetChild(i).gameObject);
            }
        }
        
        StartCoroutine(StartPrefabs());
    }

    private IEnumerator StartPrefabs()
    {
        yield return new WaitForFixedUpdate();
        foreach (var image in spawnPrefabsDictionary.keys)
        {
            GameObject newClick = Instantiate(uiPrefab, contentParent.transform);
            newClick.GetComponent<Image>().sprite = image;
            uiClickAbles.Add(newClick);
            var prefab = spawnPrefabsDictionary.GetValue(image);
            prefab.GetComponent<PlaceableObject>().uiIndex = uiClickAbles.Count - 1;
            prefabs.Add(prefab);
            var button = newClick.GetComponent<Button>();
            button.onClick.AddListener(onSelect);
            if (uiClickAbles.Count != 1) continue;
            try
            {
                button.onClick.Invoke();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            /*var triggers = newClick.GetComponentInChildren<EventTrigger>().triggers;
            Debug.Log(triggers);
            foreach (var trigger in triggers)
            {
                if (trigger.eventID == EventTriggerType.Select) trigger.callback.AddListener(selectEvent);
                if (trigger.eventID == EventTriggerType.Deselect) trigger.callback.AddListener(OnDeselect());

                if (uiClickAbles.Count == 1)
                {
                    BuildingSystem.current.ChangeSelectedBlock(0);
                    EventSystem.current.firstSelectedGameObject = uiClickAbles[0];
                    try
                    {
                        trigger.callback.Invoke(new PointerEventData(EventSystem.current));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }*/
        }

        /*foreach (var prefab in BuildingSystem.current.prefab)
        {
            DestroyImmediate(prefab.gameObject, true);
        }*/

        BuildingSystem.current.prefab = prefabs.ToArray();
        yield return null;
    }

    private void OnSelect()
    {
        if (selectedIndex > -1 && selectedIndex < uiClickAbles.Count)
        {
            var script = uiClickAbles[selectedIndex].GetComponent<OnButtonClick>();
            if (script.border.activeSelf) script.OnClick();
        }
        foreach (var button in uiClickAbles)
        {
            if (!button.GetComponent<OnButtonClick>().border.activeSelf) continue;
            Predicate<GameObject> objectToIndex = n => n == button;
            selectedIndex = uiClickAbles.FindIndex(objectToIndex);
            
            BuildingSystem.current.ChangeSelectedBlock(selectedIndex);
            return;
        }
        //If the codes runs till here no blocks are selected
        NoBlockSelected();
    }

    public void NoBlockSelected()
    {
        selectedIndex = -1;
        BuildingSystem.current.selectedPrefabIndex = -1;
    }

    public void TurnOnUI(PlaceableObject prefabSpawned)
    {
        var selectedClick = uiClickAbles[prefabSpawned.uiIndex];
        selectedClick.SetActive(true);
    }

    public void TurnOffUI(PlaceableObject prefabSpawned)
    {
        var obj = prefabSpawned.gameObject;
        var selectedClick = uiClickAbles[prefabSpawned.uiIndex];
        selectedClick.GetComponent<OnButtonClick>().OnClick();
        selectedClick.SetActive(false);
    }
}

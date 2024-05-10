using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class UIMenuLogic : MonoBehaviour
{
    #region ScrollArea
    [Header("Scroll Area")]
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
    #endregion
    
    /*#region Menu & Buttons

    [Header("Menu & Buttons")]
    [SerializeField] private Button createButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button deleteButton;
    
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private GameObject menuObject;
    
    [SerializeField]
    InputActionProperty clickOnScreen;
    
    //private XRUIInputModule inputActionReference;
    
    private bool menuShown;
    bool clickOnUI;
    private static readonly int Show = Animator.StringToHash("Show");

    void OnEnable()
    {
        HideMenu();
        clickOnScreen.action.started += HideTapOutsideUI;
        
        createButton.onClick.AddListener(ShowMenu);
        cancelButton.onClick.AddListener(HideMenu);
        deleteButton.onClick.AddListener(DeleteFocusedObject);
        
        deleteButton.gameObject.SetActive(false);
    }
    
    void OnDisable()
    {
        menuShown = false;
        clickOnScreen.action.started -= HideTapOutsideUI;
        
        createButton.onClick.RemoveListener(ShowMenu);
        cancelButton.onClick.RemoveListener(HideMenu);
        deleteButton.onClick.RemoveListener(DeleteFocusedObject);
    }

    private void Update()
    {
        if (menuShown)
        {
            clickOnUI = SharedFunctionality.Instance.TouchUI();
        }
        else
        {
            clickOnUI = false;
            createButton.gameObject.SetActive(true);
        }
    }
    
    public void DeleteButtonVisibility()
    {
        deleteButton.gameObject.SetActive(GridManager.Instance.placedObjects.Count > 0);
    }

    void ShowMenu()
    {
        if (menuShown)
        {
            HideMenu();
            return;
        }
        menuShown = true;
        menuObject.SetActive(true);
        
        menuAnimator.SetBool(Show, true);
    }
    
    private void HideMenu()
    {
        menuAnimator.SetBool(Show, false);
        menuShown = false;
    }
    
    void HideTapOutsideUI(InputAction.CallbackContext context)
    {
        if (!clickOnUI)
        {
            if (menuShown)
                HideMenu();
        }
    }
    
    private void DeleteFocusedObject()
    {
        GridManager.Instance.DestroyObject();
    }
    
    #endregion*/
}

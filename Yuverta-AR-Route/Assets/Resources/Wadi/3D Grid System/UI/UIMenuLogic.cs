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
    [Header("General")]
    [SerializeField] private Canvas canvas;
    #region ScrollArea
    [Header("Scroll Area")]
    [SerializeField]
    private GameObject UIObjectParent;
    
    [SerializeField]
    private GameObject UIObjectPrefab;
    
    [SerializeField]
    private SerializableDictionary<string, Sprite> UIObjectImages;
    
    private List<GameObject> UIObjects = new List<GameObject>();
    
    public bool  isDragging;
    
    [Header("Moving Objects To Scroll Area")]
    [SerializeField] private float speedModifier;

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
        newUIObject.name = CloneTagRemover(prefab.name);
        //newUIObject.GetComponent<Image>().sprite = UIObjectImages.GetValue(prefab.name);
        newUIObject.GetComponent<Button>(). onClick.AddListener(() => OnButtonClick(newUIObject));
        UIObjects.Add(newUIObject);
    }

    //Index is not used in this function but is required for the event
    public void Remove(GameObject prefab)
    {
        var prefabName = CloneTagRemover(prefab.name);
        
        var objToRemove = UIObjects.Find(obj => obj.name == prefabName);
        UIObjects.Remove(objToRemove);
        Destroy(objToRemove);
    }

    public void OnObjectDelete(GameObject prefab)
    {
        if (CheckName(prefab.name) || prefab == null) return;
        var canvasPosition = SharedFunctionality.Instance.WorldToCanvasPosition(canvas, Camera.main, prefab.transform.position);
        Debug.Log(canvasPosition);
        var newUIObject = Instantiate(UIObjectPrefab, canvasPosition, Quaternion.identity, canvas.transform);
        newUIObject.name = CloneTagRemover(prefab.name);
        //newUIObject.GetComponent<Image>().sprite = UIObjectImages.GetValue(prefab.name);
        newUIObject.GetComponent<Button>(). onClick.AddListener(() => OnButtonClick(newUIObject));
        newUIObject.transform.position = canvasPosition;
        StartCoroutine(RouteToFollow(newUIObject));
    }

    private IEnumerator RouteToFollow(GameObject spriteToMove)
    {
        CanvasScaler canvasRect = canvas.GetComponent<CanvasScaler>();
        Vector2 p0 = spriteToMove.transform.position;
        Vector2 p3 =  new Vector2(canvasRect.referenceResolution.x, -100);
        Vector2 p1 = new Vector2(p3.x / 2, 0);
        Vector2 p2 = new Vector2(p3.x, p3.y / 2);

        float t = 0;
        
        while (t < 1)
        {
            t += Time.deltaTime * speedModifier;
            
            //Bezier Curve calculation
            var newPosition = MathF.Pow(1 - t, 3) * p0 + 3 * MathF.Pow(1 - t, 2) * t * p1 + 3 * (1 - t) * MathF.Pow(t, 2) * p2 + MathF.Pow(t, 3) * p3;
            spriteToMove.transform.position = newPosition;
            yield return new WaitForEndOfFrame();
        }
        
        UIObjects.Add(spriteToMove);
        spriteToMove.transform.SetParent(UIObjectParent.transform);
        //spriteToMove.transform.position = UIObjects.Last().transform.position;
    }

    private void OnButtonClick(GameObject obj)
    {
        DragDropHandler thisObj = obj.GetComponent<DragDropHandler>();
        thisObj.dragSprite = UIObjectImages.GetValue(obj.name);
        isDragging = true;
        
        GridManager.Instance.objectSpawner.m_SpawnOptionName = obj.name;
    }

    private bool CheckName(string newName)
    {
        if (UIObjects.Any(obj => obj.name == newName))
        {   //Remember to put this to true once there are more objects
            return false;
        }
        
        return false;
    }

    private string CloneTagRemover(string checkString)
    {
        if (checkString.Contains("(Clone)"))
        {   
            checkString = checkString.Replace("(Clone)", "");
            //In case there are multiple (Clone) tags in the name it is checked again
            return CloneTagRemover(checkString);
        }
        return checkString;
    }
    #endregion
    
    #region Menu & Buttons

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
        return;
        HideMenu();
        clickOnScreen.action.started += HideTapOutsideUI;
        
        createButton.onClick.AddListener(ShowMenu);
        cancelButton.onClick.AddListener(HideMenu);
        deleteButton.onClick.AddListener(DeleteFocusedObject);
        
        deleteButton.gameObject.SetActive(false);
    }
    
    void OnDisable()
    {
        if (SharedFunctionality.IsQuitting)
            return; // Stop the function if the application is closing
        
        menuShown = false;
        clickOnScreen.action.started -= HideTapOutsideUI;
        
        createButton.onClick.RemoveListener(ShowMenu);
        cancelButton.onClick.RemoveListener(HideMenu);
        deleteButton.onClick.RemoveListener(DeleteFocusedObject);
    }

    /*private void Update()
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
    }*/
    
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
    
    #endregion
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events.GameEvents.Typed;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIMenuLogic : MonoBehaviour
{
    [Header("General")] [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject swipeCanvas;

    [Header("Scroll Area")] [SerializeField]
    private Transform uiObjectParentTransform;

    [SerializeField] private GameObject uiButtonPrefab;
    [SerializeField] private GameObject gridmanagerPrefab;

    [SerializeField] private Button startAnimationsButton;
    [SerializeField] private Button clearGridButton;

    private List<GameObject> uiObjects = new();

    [Header("Moving Objects To Scroll Area")] [SerializeField]
    private float speedModifier;

    [SerializeField] private UnityEvent onWadiCorrect;
    [SerializeField] private UnityEvent onWadiIncorrect;

    private Dictionary<string, Sprite> UIObjectImages = new();
    
    public Canvas Canvas => canvas;

    private void Start()
    {
        startAnimationsButton.gameObject.SetActive(false);
        clearGridButton.gameObject.SetActive(false);

        //Add all the objects to the scroll area
        // foreach (Transform child in uiObjectParentTransform.transform)
        // {
        //     Destroy(child.gameObject);
        // }

        var objectsToSpawn = gridmanagerPrefab.GetComponent<GridManager>().ObjsToSpawn;
        foreach (var obj in objectsToSpawn.keys)
        {
            var dragDropHandler = obj.GetComponent<DragDropHandler>();

            var objName = dragDropHandler.itemPrefab.name;
            var objSprite = dragDropHandler.dragSprite;

            if (objSprite == null)
            {
                objSprite = null;
            }

            UIObjectImages.Add(objName, objSprite);
            uiObjects.Add(obj);
        }

        EnableCanvas(false);
    }

    private void AddRange(List<GameObject> prefabList)
    {
        foreach (var prefab in prefabList)
        {
            Add(prefab);
        }
    }

    private void Add(GameObject prefab)
    {
        if (prefab == null)
            return;

        var newUIObject = Instantiate(uiButtonPrefab, uiObjectParentTransform.transform);
        newUIObject.name = CloneTagRemover(prefab.name);

        newUIObject.GetComponent<Button>().onClick.AddListener(() => OnButtonClick(newUIObject));
        uiObjects.Add(newUIObject);
    }

    public void Remove(GameObject prefab)
    {
        var prefabName = CloneTagRemover(prefab.name);

        uiObjects.Remove(prefab);
        Destroy(prefab);

        switch (uiObjects.Count)
        {
            case 0:
                startAnimationsButton.gameObject.SetActive(true);
                clearGridButton.gameObject.SetActive(false);
                break;
            case >= 16:
                startAnimationsButton.gameObject.SetActive(false);
                clearGridButton.gameObject.SetActive(false);
                break;
            default:
                startAnimationsButton.gameObject.SetActive(false);
                clearGridButton.gameObject.SetActive(true);
                break;
        }
    }

    public void OnObjectDelete(GameObject prefab)
    {
        if (prefab == null)
            return;

        var canvasPosition = Camera.main.WorldToScreenPoint(prefab.transform.position);

        var uiObject = Instantiate(uiButtonPrefab, canvasPosition, Quaternion.identity, canvas.transform);
        uiObject.name = CloneTagRemover(prefab.name);

        uiObject.GetComponent<Button>().onClick.AddListener(() => OnButtonClick(uiObject));

        uiObject.transform.position = canvasPosition;

        startAnimationsButton.gameObject.SetActive(false);

        StartCoroutine(RouteToFollow(uiObject));
    }

    private IEnumerator RouteToFollow(GameObject spriteToMove)
    {
        CanvasScaler canvasRect = canvas.GetComponent<CanvasScaler>();
        Vector2 p0 = spriteToMove.transform.position;
        Vector2 p3 = new Vector2(canvasRect.referenceResolution.x, -100);
        Vector2 p1 = new Vector2(p3.x / 2, 0);
        Vector2 p2 = new Vector2(p3.x, p3.y / 2);

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * speedModifier;

            //Bezier Curve calculation
            var newPosition = MathF.Pow(1 - t, 3) * p0 + 3 * MathF.Pow(1 - t, 2) * t * p1 +
                              3 * (1 - t) * MathF.Pow(t, 2) * p2 + MathF.Pow(t, 3) * p3;
            spriteToMove.transform.position = newPosition;
            yield return new WaitForEndOfFrame();
        }

        uiObjects.Add(spriteToMove);
        spriteToMove.transform.SetParent(uiObjectParentTransform.transform);
        //spriteToMove.transform.position = UIObjects.Last().transform.position;
    }

    private void OnButtonClick(GameObject obj)
    {
        var thisObj = obj.GetComponent<DragDropHandler>();
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

    /// <summary>
    /// Enables the canvas. used for the start of the game when grid is spawned
    /// </summary>
    public void EnableCanvas(bool enable)
    {
        swipeCanvas.gameObject.SetActive(enable);
    }
}
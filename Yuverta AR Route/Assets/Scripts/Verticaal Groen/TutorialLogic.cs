using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class TutorialLogic : MonoBehaviour, IDragHandler, IEndDragHandler
{

    public List<Sprite> images;
    public Image displayImage;
    public RectTransform displayImageRectTransform;
    public Button nextButton;
    public Button prevButton;
    public Transform paginationContainer;
    public GameObject paginationIndicatorPrefab;
    public float paginationIndicatorSpacing;

    private List<PaginationIndicator> paginationIndicators;
    private int currentImageIndex = 0;
    private Vector2 dragStartPos;

    void Start()
    {
        paginationIndicators = new List<PaginationIndicator>();

        foreach (Transform child in paginationContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < images.Count; i++)
        {
            var indicator = Instantiate(paginationIndicatorPrefab, paginationContainer);
            Debug.Log(indicator.GetComponent<RectTransform>().anchoredPosition);
            paginationIndicators.Add(indicator.GetComponent<PaginationIndicator>());
        }

        var halfList = paginationIndicators.Count / 2;
        for (int i = 0; i < paginationIndicators.Count; i++)
        {
            RectTransform indicator = paginationIndicators[i].gameObject.GetComponent<RectTransform>();
            if (i > halfList)
            {
                indicator.anchoredPosition = new Vector2((i - halfList) * paginationIndicatorSpacing, 0);
            }
            else if (i == halfList)
            {
                indicator.anchoredPosition = new Vector2(0, 0);
            }
            else if (i < halfList)
            {
                indicator.anchoredPosition = new Vector2((i - halfList) * paginationIndicatorSpacing, 0);
            }
        }

        currentImageIndex = 0;
        ShowCurrentImage();
        nextButton.onClick.AddListener(NextImage);
        prevButton.onClick.AddListener(PreviousImage);
    }

    void ShowCurrentImage()
    {
        if (images.Count > 0)
        {
            displayImage.sprite = images[currentImageIndex];
            UpdatePaginationIndicators();
            prevButton.interactable = true;
            nextButton.interactable = true;
        }
    }

    void UpdatePaginationIndicators()
    {
        for (int i = 0; i < paginationIndicators.Count; i++)
        {
            paginationIndicators[i].dot.color = (i == currentImageIndex) ? paginationIndicators[i].activeColor : paginationIndicators[i].inactiveColor;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 dragOffset = eventData.position - dragStartPos;
        displayImageRectTransform.anchoredPosition += dragOffset;
        dragStartPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float dragDistance = eventData.position.x - dragStartPos.x;

        if (Mathf.Abs(dragDistance) > displayImageRectTransform.rect.width / 4)
        {
            if (dragDistance > 0)
            {
                PreviousImage();
            }
            else
            {
                NextImage();
            }
        }

        displayImageRectTransform.anchoredPosition = Vector2.zero;
    }

    void NextImage()
    {
        StartCoroutine(TransitionImage(true));
    }

    void PreviousImage()
    {
        StartCoroutine(TransitionImage(false));
    }

    IEnumerator TransitionImage(bool isNext)
    {
        int newImageIndex = isNext ? currentImageIndex + 1 : currentImageIndex - 1;

        if (newImageIndex >= images.Count)
        {
            newImageIndex = 0; // Loop to first image
        }
        else if (newImageIndex < 0)
        {
            newImageIndex = images.Count - 1; // Loop to last image
        }

        RectTransform rectTransform = displayImageRectTransform;
        Vector2 initialPosition = rectTransform.anchoredPosition;
        Vector2 targetPosition = isNext ? new Vector2(-rectTransform.rect.width, 0) : new Vector2(rectTransform.rect.width, 0);

        float elapsedTime = 0f;
        float duration = 0.5f; // Transition duration

        // Transition out
        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = targetPosition;

        // Update image
        currentImageIndex = newImageIndex;
        displayImage.sprite = images[currentImageIndex];
        UpdatePaginationIndicators();

        // Transition in
        initialPosition = -targetPosition;
        rectTransform.anchoredPosition = initialPosition;
        elapsedTime = 0f;
        
        nextButton.interactable = false;
        prevButton.interactable = false;

        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(initialPosition, Vector2.zero, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        nextButton.interactable = true;
        prevButton.interactable = true;
        
        rectTransform.anchoredPosition = Vector2.zero;
    }
}
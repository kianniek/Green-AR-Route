using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SimpleRuntimeUI : MonoBehaviour
{
    public Button _snowButton;
    public Button _rainButton;
    public Button _sunButton;

    [SerializeField] private UnityEvent onSnowButtonClicked;
    [SerializeField] private UnityEvent onRainButtonClicked;
    [SerializeField] private UnityEvent onSunButtonClicked;
    
    private bool snowButtonClicked;
    private bool rainButtonClicked;
    private bool sunButtonClicked;
    
    [SerializeField] private UnityEvent onAllButtonsClicked;
    [SerializeField] private FMODStudioEventQueueManager queueManager;

    //Add logic that interacts with the UI controls in the `OnEnable` methods
    private void OnEnable()
    {
        _snowButton.onClick.AddListener(OnSnowButtonClicked);
        _rainButton.onClick.AddListener(OnRainButtonClicked);
        _sunButton.onClick.AddListener(OnSunButtonClicked);
    }

    private void OnDisable()
    {
        _snowButton.onClick.AddListener(OnSnowButtonClicked);
        _rainButton.onClick.AddListener(OnRainButtonClicked);
        _sunButton.onClick.AddListener(OnSunButtonClicked);
    }

    private void OnSnowButtonClicked()
    {
        if (snowButtonClicked)
        {
            return;
        }
        onSnowButtonClicked.Invoke();
        snowButtonClicked = true;
        CheckIfAllButtonsClicked();
    }
    private void OnRainButtonClicked()
    {
        if (rainButtonClicked)
        {
            return;
        }
        Debug.Log("Rain button clicked");
        onRainButtonClicked.Invoke();
        rainButtonClicked = true;
        CheckIfAllButtonsClicked();
    }
    private void OnSunButtonClicked()
    {
        if (sunButtonClicked)
        {
            return;
        }
        onSunButtonClicked.Invoke();
        sunButtonClicked = true;
        CheckIfAllButtonsClicked();
    }
    
    private void CheckIfAllButtonsClicked()
    {
        if (snowButtonClicked && rainButtonClicked && sunButtonClicked)
        {
            Debug.Log("All buttons clicked");
            StartCoroutine(WaitForQueue());
        }
    }
    
    private IEnumerator WaitForQueue()
    {
        yield return new WaitUntil(() => queueManager.QueueEmpty && !queueManager.IsPlaying);
        Debug.Log("Queue is empty");
        onAllButtonsClicked.Invoke();
    }

    public void TurnOffAllButtons()
    {
        _snowButton.interactable = false;
        _rainButton.interactable = false;
        _sunButton.interactable = false;
    }
    
    public void TurnOnAllButtons()
    {
        _snowButton.interactable = true;
        _rainButton.interactable = true;
        _sunButton.interactable = true;
    }
}
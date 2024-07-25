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
        onSnowButtonClicked.Invoke();
        snowButtonClicked = true;
        CheckIfAllButtonsClicked();
    }
    private void OnRainButtonClicked()
    {
        onRainButtonClicked.Invoke();
        rainButtonClicked = true;
        CheckIfAllButtonsClicked();
    }
    private void OnSunButtonClicked()
    {
        onSunButtonClicked.Invoke();
        sunButtonClicked = true;
        CheckIfAllButtonsClicked();
    }
    
    private void CheckIfAllButtonsClicked()
    {
        if (snowButtonClicked && rainButtonClicked && sunButtonClicked)
        {
            Debug.Log("All buttons clicked");
            onAllButtonsClicked.Invoke();
        }
    }
}
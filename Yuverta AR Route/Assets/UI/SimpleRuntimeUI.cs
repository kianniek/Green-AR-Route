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
    }
    private void OnRainButtonClicked()
    {
        onRainButtonClicked.Invoke();
    }
    private void OnSunButtonClicked()
    {
        onSunButtonClicked.Invoke();
    }
}
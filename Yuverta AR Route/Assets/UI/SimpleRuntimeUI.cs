using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

public class SimpleRuntimeUI : MonoBehaviour
{
    private Button _snowButton;
    private Button _rainButton;
    private Button _sunButton;

    [SerializeField] private UnityEvent onSnowButtonClicked;
    [SerializeField] private UnityEvent onRainButtonClicked;
    [SerializeField] private UnityEvent onSunButtonClicked;

    //Add logic that interacts with the UI controls in the `OnEnable` methods
    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        _snowButton = uiDocument.rootVisualElement.Q("button_Snow") as Button;
        _rainButton = uiDocument.rootVisualElement.Q("button_Rain") as Button;
        _sunButton = uiDocument.rootVisualElement.Q("button_Sun") as Button;

        _snowButton.RegisterCallback<ClickEvent>(OnSnowButtonClicked);
        _rainButton.RegisterCallback<ClickEvent>(OnRainButtonClicked);
        _sunButton.RegisterCallback<ClickEvent>(OnSunButtonClicked);
    }

    private void OnDisable()
    {
        _snowButton.UnregisterCallback<ClickEvent>(OnSnowButtonClicked);
        _rainButton.UnregisterCallback<ClickEvent>(OnRainButtonClicked);
        _sunButton.UnregisterCallback<ClickEvent>(OnSunButtonClicked);
    }

    private void OnSnowButtonClicked(ClickEvent evt)
    {
        onSnowButtonClicked.Invoke();
    }
    private void OnRainButtonClicked(ClickEvent evt)
    {
        onRainButtonClicked.Invoke();
    }
    private void OnSunButtonClicked(ClickEvent evt)
    {
        onSunButtonClicked.Invoke();
    }
}
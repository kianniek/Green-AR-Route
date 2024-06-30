using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [SerializeField] private UnityEvent onQuizFinished = new();
    [SerializeField] private UnityEvent onQuestionAnsweredCorrectly = new();

    [Header("Quiz UI Elements")]
    [Tooltip("The parent object containing all other objects")]
    [SerializeField] private GameObject parentObject;

    [Tooltip("The text element that will display the quiz question")]
    [SerializeField] private TextMeshPro questionText;

    [Tooltip("All the buttons that will be used to answer the quiz questions")]
    [SerializeField] private QuizButton[] choiceButtons;
    [SerializeField] private string buttonTag = "QuizButton";

    [Tooltip("The scriptable object containing the quiz questions")]
    [SerializeField] private QuizQuestions quizQuestions;

    [Tooltip("The distance the quiz elements should be from the camera")]
    [SerializeField] private float distanceFromCamera;

    [SerializeField] private bool keepInFrontOfCamera = true;

    [Header("Lerp Settings")]
    public float lerpSpeed = 5f;
    [SerializeField] private float lerpThreshold = 0.1f;

    [SerializeField] private InputActionReference tapStartClick;
    [SerializeField] private InputActionReference tapStartPosition;

    private List<Question> questions;
    private int currentQuestionIndex;
    private int correctQuestions;
    private int totalQuestions;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        tapStartClick.action.Enable();
        tapStartPosition.action.Enable();
        
        tapStartClick.action.started += OnTouchPerformed;
    }

    private void Start()
    {
        InitializeQuiz();
        StartCoroutine(KeepObjectInfrontOfCamera());
        DisplayQuestion();
    }

    private void InitializeQuiz()
    {
        PositionAndRotateParentObject();
        questions = new List<Question>(quizQuestions.questions);
        SetupChoiceButtons();
    }

    private void PositionAndRotateParentObject()
    {
        var newPosition = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera;
        parentObject.transform.position = newPosition;
        parentObject.transform.LookAt(mainCamera.transform);
    }

    private void SetupChoiceButtons()
    {
        if (choiceButtons == null || choiceButtons.Length == 0)
        {
            choiceButtons = new QuizButton[4];
            var buttons = GetComponentsInChildren<QuizButton>();
            foreach (var button in buttons)
            {
                choiceButtons[button.buttonIndex] = button;
            }
        }

        foreach (var button in choiceButtons)
        {
            button.gameObject.SetActive(false);
            button.onRaycastHit.AddListener(OnOptionSelected);
        }
    }

    private void DisplayQuestion()
    {
        if (currentQuestionIndex < questions.Count - 1)
        {
            Question question = questions[currentQuestionIndex];
            questionText.text = question.questionText;
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i < question.options.Count)
                {
                    choiceButtons[i].GetComponentInChildren<TextMeshPro>().text = question.options[i].answer;
                    choiceButtons[i].gameObject.SetActive(true);
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            foreach (var button in choiceButtons)
            {
                button.gameObject.SetActive(false);
            }
            Debug.Log($"Quiz finished. Correct answers: {correctQuestions}/{totalQuestions}");
            onQuizFinished.Invoke();
        }
    }

    public void OnOptionSelected(int index)
    {
        if (questions[currentQuestionIndex].options[index].isCorrect)
        {
            correctQuestions++;
            onQuestionAnsweredCorrectly.Invoke();
        }
        
        totalQuestions++;
        currentQuestionIndex++;
        DisplayQuestion();
    }

    public void ResetQuiz()
    {
        currentQuestionIndex = 0;
        correctQuestions = 0;
        totalQuestions = 0;
        DisplayQuestion();
    }

    private IEnumerator KeepObjectInfrontOfCamera()
    {
        while (keepInFrontOfCamera)
        {
            var newPosition = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera;
            if (Vector3.Distance(parentObject.transform.position, newPosition) > lerpThreshold)
            {
                parentObject.transform.position = Vector3.Slerp(parentObject.transform.position, newPosition, lerpSpeed * Time.deltaTime);
            }
            parentObject.transform.LookAt(mainCamera.transform);
            yield return null;
        }
    }

    private void OnTouchPerformed(InputAction.CallbackContext context)
    {
        var screenPosition = tapStartPosition.action.ReadValue<Vector2>();
        var ray = mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.collider.CompareTag(buttonTag))
            {
                SelectedObject(hit.collider.gameObject);
            }
        }
    }

    public void SelectedObject(GameObject obj)
    {
        obj.GetComponent<QuizButton>().OnClick();
    }

    private void OnDestroy()
    {
        tapStartClick.action.started -= OnTouchPerformed;
        tapStartClick.action.Disable();
    }
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class QuizManager : MonoBehaviour
{
    [SerializeField] private UnityEvent onQuizFinished = new();
    [SerializeField] private UnityEvent onQuestionAnsweredCorrectly = new();
    [SerializeField] private UnityEvent onQuizTotallyCorrect = new();
    [SerializeField] private UnityEvent onQuizPartiallyCorrect = new();
    [SerializeField] private UnityEvent onQuizTotallyWrong = new();

    [Header("Quiz UI Elements")] 
    [Tooltip("The text element that will display the quiz question")] 
    [SerializeField]
    private TMP_Text questionText;

    [Tooltip("All the buttons that will be used to answer the quiz questions")] 
    [SerializeField]
    private QuizButton[] choiceButtons;

    [SerializeField] private string buttonTag = "QuizButton";

    [Tooltip("The scriptable object containing the quiz questions")] 
    [SerializeField]
    private QuizQuestions quizQuestions;

    private List<Question> questions;
    private List<QuizAnswer> incorrectAnswers;
    private int currentQuestionIndex;
    private int correctQuestions;
    private int totalQuestions;

    private void Start()
    {
        // Hide all children of this object
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void StartQuiz()
    {
        InitializeQuiz();
        DisplayQuestion();
    }

    private void InitializeQuiz()
    {
        // Unhide all children of this object
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        
        questions = new List<Question>(quizQuestions.questions);
        incorrectAnswers = new List<QuizAnswer>();
        SetupChoiceButtons();
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
            // button.onRaycastHit.AddListener(OnOptionSelected);
            button.gameObject.SetActive(false);
        }
    }

    private void DisplayQuestion()
    {
        // Check if there are still questions left
        if (currentQuestionIndex < questions.Count)
        {
            // Disable all buttons
            foreach (var choiceButton in choiceButtons)
            {
                choiceButton.gameObject.SetActive(false);
            }

            var question = questions[currentQuestionIndex];
            questionText.text = question.questionText;

            for (var i = 0; i < choiceButtons.Length; i++)
            {
                if (i < question.options.Count)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    choiceButtons[i].ButtonText = question.options[i].answer;
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
            HandleQuizResult();
        }
    }

    private void HandleQuizResult()
    {
        if (correctQuestions == totalQuestions)
        {
            onQuizTotallyCorrect.Invoke();
        }
        else if (correctQuestions == 0)
        {
            onQuizTotallyWrong.Invoke();
        }
        else
        {
            onQuizPartiallyCorrect.Invoke();
        }
    }

    public void OnOptionSelected(QuizButton button)
    {
        int index = button.buttonIndex;
        var selectedAnswer = questions[currentQuestionIndex].options[index];
        if (selectedAnswer.isCorrect)
        {
            correctQuestions++;
            onQuestionAnsweredCorrectly.Invoke();
        }
        else
        {
            incorrectAnswers.Add(selectedAnswer);
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

        // Remove previously incorrect answers from the questions
        foreach (var question in questions)
        {
            question.options.RemoveAll(option => incorrectAnswers.Contains(option));
        }
    }

    public void RestartQuizWithoutIncorrectAnswers()
    {
        ResetQuiz();
        StartQuiz();
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                var ray = Camera.main.ScreenPointToRay(touch.position);

                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.collider.CompareTag(buttonTag))
                    {
                        SelectedObject(hit.collider.gameObject);
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.collider.CompareTag(buttonTag))
                {
                    SelectedObject(hit.collider.gameObject);
                }
            }
        }
    }

    public void SelectedObject(GameObject obj)
    {
        obj.GetComponent<QuizButton>().OnClick();
    }
}

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

    [Header("Quiz UI Elements")] [Tooltip("The text element that will display the quiz question")] [SerializeField]
    private TMP_Text questionText;

    [Tooltip(
        "The text element that will display the number of correct answers. Use {correctQuestions} as a placeholder for the actual number of correct answers")]
    [SerializeField]
    private TMP_Text correctAnswersText; // Add TMP_Text field for displaying correct answers count

    [SerializeField]
    private GameObject correctAnswersDisplay; // Add GameObject field for displaying correct answers count

    [Tooltip("All the buttons that will be used to answer the quiz questions")] [SerializeField]
    private QuizButton[] choiceButtons;

    [SerializeField] private string buttonTag = "QuizButton";

    [Tooltip("The scriptable object containing the quiz questions")] [SerializeField]
    private QuizQuestions quizQuestions;

    [SerializeField] private bool restartQuizWithoutIncorrectAnswers = true;

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
        UpdateCorrectAnswersText(); // Initialize the correct answers text display
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
            button.gameObject.SetActive(false);
        }
    }

    private void DisplayQuestion()
    {
        correctAnswersDisplay.SetActive(false); // Hide the correct answers count display
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
        correctAnswersDisplay.SetActive(true); // Show the correct answers count display
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
            UpdateCorrectAnswersText(); // Update the correct answers count whenever a correct answer is selected
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
        UpdateCorrectAnswersText(); // Reset the correct answers text display

        if (restartQuizWithoutIncorrectAnswers)
        {
            // Remove previously incorrect answers from the questions
            foreach (var question in questions)
            {
                question.options.RemoveAll(option => incorrectAnswers.Contains(option));
            }
        }
    }

    public void RestartQuiz()
    {
        ResetQuiz();
        StartQuiz();
    }

    private void UpdateCorrectAnswersText()
    {
        // Replace the placeholder {correctQuestions} with the current correctQuestions value
        correctAnswersText.text = correctAnswersText.text.Replace("{correctQuestions}", correctQuestions.ToString());
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
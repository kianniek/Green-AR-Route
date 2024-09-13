using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class QuizManager : MonoBehaviour
{
    [SerializeField] private UnityEvent onQuizFinished = new();
    [SerializeField] private UnityEvent onQuestionAnsweredCorrectly = new();
    [SerializeField] private UnityEvent onQuestionAnsweredIncorrect = new();
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

    private string quizEndText;
    private Camera _camera;

    private bool isWaitingForAnimation;

    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject prevButton;
    private int currentReviewIndex = 0;
    
    private List<int> selectedAnswers;  // To store the index of selected answers



    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Start()
    {
        // Hide all children of this object
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        quizEndText = correctAnswersText.text;
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
        totalQuestions = questions.Count;

        correctAnswersDisplay.SetActive(false); // Show the correct answers count display
        prevButton.SetActive(false);
        nextButton.SetActive(false);
        currentReviewIndex = questions.Count;
        questionText.gameObject.SetActive(true);
        
        selectedAnswers = new List<int>(new int[questions.Count]);  // Initialize with default values
        for (int i = 0; i < selectedAnswers.Count; i++)
        {
            selectedAnswers[i] = -1;  // Set all initially to -1 indicating no answer selected
        }

        SetupChoiceButtons();
    }

    private void SetupChoiceButtons()
    {
        if (choiceButtons == null || choiceButtons.Length == 0)
        {
            const int quizButtonAmount = 4;
            choiceButtons = new QuizButton[quizButtonAmount];
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
        prevButton.SetActive(false);
        nextButton.SetActive(false);

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
        questionText.gameObject.SetActive(false);

        if (correctQuestions == totalQuestions)
        {
            onQuizTotallyCorrect.Invoke();
        }
        else if (correctQuestions >= Mathf.CeilToInt(totalQuestions * 0.5f) + 1) // At least 50% correct
        {
            onQuizPartiallyCorrect.Invoke();
        }
        else
        {
            onQuizTotallyWrong.Invoke();
        }

        // Start review mode after the quiz finishes
        StartReviewMode();
    }

    private void StartReviewMode()
    {
        // Start the review mode from the last question + 1
        currentQuestionIndex = questions.Count;

        DisplayReviewQuestion();
    }


    public void OnOptionSelected(QuizButton button)
    {
        if (!isWaitingForAnimation)
        {
            StartCoroutine(OnOptionSelectedCoroutine(button));
            selectedAnswers[currentQuestionIndex] = button.buttonIndex;  // Store the selected button index
        }
    }


    IEnumerator OnOptionSelectedCoroutine(QuizButton button)
    {
        isWaitingForAnimation = true;
        var index = button.buttonIndex;
        var selectedAnswer = questions[currentQuestionIndex].options[index];
        const float flashDuration = 1f;

        if (selectedAnswer.isCorrect)
        {
            button.FlashButton(Color.green, flashDuration);
            correctQuestions++;
            onQuestionAnsweredCorrectly.Invoke();
            UpdateCorrectAnswersText(); // Update the correct answers count whenever a correct answer is selected
        }
        else
        {
            button.FlashButton(Color.red, flashDuration);
            incorrectAnswers.Add(selectedAnswer);
            onQuestionAnsweredIncorrect.Invoke();
        }

        yield return new WaitForSeconds(flashDuration);

        currentQuestionIndex++;
        DisplayQuestion();
        isWaitingForAnimation = false;
    }

    public void ResetQuiz()
    {
        currentQuestionIndex = 0;
        correctQuestions = 0;
        totalQuestions = 0;
        UpdateCorrectAnswersText(); // Reset the correct answers text display
        prevButton.SetActive(false);
        nextButton.SetActive(false);

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
        Debug.Log($"Quiz finished. Correct answers: {correctQuestions}/{totalQuestions}");
        var str = quizEndText;

        // Update the correct answers text display
        str = str.Replace("{correctQuestions}", correctQuestions.ToString());
        // Update the total questions text display
        str = str.Replace("{totalQuestions}", totalQuestions.ToString());
        // Update the correct answers text display
        correctAnswersText.text = str;
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

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
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

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
        // Check if the quiz manager is waiting for an animation to finish
        if (isWaitingForAnimation)
            return;

        obj.GetComponent<QuizButton>().OnClick();
    }

    private void DisplayReviewQuestion()
    {
        // Hide all buttons initially
        foreach (var choiceButton in choiceButtons)
        {
            choiceButton.gameObject.SetActive(false);
        }

        questionText.gameObject.SetActive(false);

        if (currentReviewIndex >= totalQuestions)
        {
            correctAnswersDisplay.SetActive(true);
            UpdateNavigationButtons();
            return;
        }

        correctAnswersDisplay.SetActive(false);

        var question = questions[currentReviewIndex];
        questionText.text = question.questionText;
        questionText.gameObject.SetActive(true);

        for (var i = 0; i < choiceButtons.Length; i++)
        {
            if (i < question.options.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].ButtonText = question.options[i].answer;

                // Highlight the selected answer
                if (i == selectedAnswers[currentReviewIndex])
                {
                    if (question.options[i].isCorrect)
                    {
                        choiceButtons[i].SetButtonColor(Color.green);  // User selected the correct answer
                    }
                    else
                    {
                        choiceButtons[i].SetButtonColor(Color.red);  // User selected the wrong answer
                    }
                }
                else
                {
                    choiceButtons[i].ResetVisualsColor();  // Reset to default color
                }
            }
        }

        UpdateNavigationButtons();
    }

    
    private void UpdateNavigationButtons()
    {
        prevButton.SetActive(currentReviewIndex > 0); // Hide if at first question
        nextButton.SetActive(currentReviewIndex < totalQuestions); // Hide if at last question
    }

    public void OnNextButtonPressed()
    {
        if (currentReviewIndex < questions.Count)
        {
            currentReviewIndex++;
            DisplayReviewQuestion();
        }
    }

    public void OnPrevButtonPressed()
    {
        if (currentReviewIndex > 0)
        {
            currentReviewIndex--;
            DisplayReviewQuestion();
        }
    }
}
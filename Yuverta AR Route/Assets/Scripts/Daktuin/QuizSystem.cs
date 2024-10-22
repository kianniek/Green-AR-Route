using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class QuizManager : MonoBehaviour
{
    public bool startQuizOnStart = false;
    [SerializeField] private UnityEvent onQuizFinished = new();
    [SerializeField] private UnityEvent onQuestionAnsweredCorrectly = new();
    [SerializeField] private UnityEvent onQuestionAnsweredIncorrect = new();
    [SerializeField] private UnityEvent onQuizTotallyCorrect = new();
    [SerializeField] private UnityEvent onQuizPartiallyCorrect = new();
    [SerializeField] private UnityEvent onQuizTotallyWrong = new();

    private FMOD.Studio.EventInstance eventInstance;

    [SerializeField] private EventReference[] eventReference;

    [Header("Quiz UI Elements")]
    [Tooltip("The text element that will display the quiz question")]
    [SerializeField] private TMP_Text questionText;

    [Tooltip("The text element that will display the number of correct answers. Use {correctQuestions} as a placeholder for the actual number of correct answers")]
    [SerializeField] private TMP_Text correctAnswersText; // Display correct answers count

    [SerializeField] private GameObject correctAnswersDisplay; // Display correct answers count
    [SerializeField] private GameObject nextButton; // The button to proceed to the next question
    [SerializeField] private GameObject submitButton; // The button to submit answers for multiple-choice questions

    [Tooltip("All the buttons that will be used to answer the quiz questions")]
    [SerializeField] private QuizButton[] choiceButtons;

    [SerializeField] private string buttonTag = "QuizButton";

    [Tooltip("The scriptable object containing the quiz questions")]
    [SerializeField] private QuizQuestions quizQuestions;

    [SerializeField] private bool restartQuizWithoutIncorrectAnswers = true;

    private List<Question> questions;
    private List<QuizAnswer> incorrectAnswers;
    private int currentQuestionIndex;
    private int correctQuestions;
    private int totalQuestions;

    private string quizEndText;
    private Camera _camera;

    private bool isWaitingForAnimation;

    [SerializeField] private GameObject prevButton;
    private int currentReviewIndex = 0;

    private List<List<int>> selectedAnswers;  // To store indices of selected answers for each question
    private int requiredCorrectAnswerCount; // Required number of correct answers for multiple choice
    private bool isMultipleChoiceQuestion; // Flag to indicate if the current question is multiple choice

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Start()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        quizEndText = correctAnswersText.text;
        
        if (startQuizOnStart)
        {
            StartQuiz();
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
        submitButton.SetActive(false); // Initially hide the submit button
        currentReviewIndex = questions.Count;
        questionText.gameObject.SetActive(true);
        
        selectedAnswers = new List<List<int>>();
        for (int i = 0; i < questions.Count; i++)
        {
            selectedAnswers.Add(new List<int>());  // Initialize empty list for each question
        }

        SetupChoiceButtons();
    }

    private void SetupChoiceButtons()
    {
        if (choiceButtons == null || choiceButtons.Length == 0)
        {
            const int quizButtonAmount = 7;
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
        submitButton.SetActive(false); // Hide the submit button for each new question

        if (currentQuestionIndex < questions.Count)
        {
            foreach (var choiceButton in choiceButtons)
            {
                choiceButton.gameObject.SetActive(false);
            }

            var question = questions[currentQuestionIndex];
            questionText.text = question.questionText;

            if (eventInstance.isValid())
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release();
            }

            if (eventReference[currentQuestionIndex].Path != "")
            {
                eventInstance = FMODUnity.RuntimeManager.CreateInstance(eventReference[currentQuestionIndex]);
                eventInstance.start();
            }

            // Determine if the current question is multiple choice
            isMultipleChoiceQuestion = question.options.Count(answer => answer.isCorrect) > 1;
            requiredCorrectAnswerCount = 0; // Reset for the current question

            for (var i = 0; i < question.options.Count; i++)
            {
                if (question.options[i].isCorrect)
                {
                    requiredCorrectAnswerCount++; // Count how many correct answers are needed
                }
            }

            // Show choice buttons and set their text
            for (var i = 0; i < choiceButtons.Length; i++)
            {
                if (i < question.options.Count)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    choiceButtons[i].ButtonText = question.options[i].answer;
                }
            }

            // If it's a multiple choice question, show the submit button
            if (isMultipleChoiceQuestion)
            {
                submitButton.SetActive(true);
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

        StartReviewMode(); // Start review mode after the quiz finishes
    }

    private void StartReviewMode()
    {
        currentQuestionIndex = questions.Count; // Start from the last question + 1
        DisplayReviewQuestion();
    }

    public void OnOptionSelected(QuizButton button)
    {
        if (!isWaitingForAnimation)
        {
            StartCoroutine(OnOptionSelectedCoroutine(button));
        }
    }

    IEnumerator OnOptionSelectedCoroutine(QuizButton button)
    {
        isWaitingForAnimation = true;
        var index = button.buttonIndex;
        var selectedForThisQuestion = selectedAnswers[currentQuestionIndex];
        const float flashDuration = 1f;

        if (selectedForThisQuestion.Contains(index))
        {
            // Deselect the answer
            selectedForThisQuestion.Remove(index);
            button.ResetVisualsColor();  // Reset to default visuals
        }
        else
        {
            // Select the answer
            selectedForThisQuestion.Add(index);
            button.SetButtonColor(Color.yellow);  // Indicate selection with a color
        }

        // Check if the number of selected answers matches the required correct answers
        if (isMultipleChoiceQuestion)
        {
            // Enable the submit button if enough answers have been selected
            Debug.Log($"Selected answers: {selectedForThisQuestion.Count}/{requiredCorrectAnswerCount}");
            submitButton.SetActive(selectedForThisQuestion.Count >= 1);
        }
        else
        {
            if (questions[currentQuestionIndex].options[selectedForThisQuestion[0]].isCorrect)
            {
                button.FlashButton(Color.green, flashDuration);
                correctQuestions++;
                onQuestionAnsweredCorrectly.Invoke();
                UpdateCorrectAnswersText(); // Update the correct answers count whenever a correct answer is selected
            }
            else
            {
                button.FlashButton(Color.red, flashDuration);
                incorrectAnswers.Add(questions[currentQuestionIndex].options[selectedForThisQuestion[0]]);
                onQuestionAnsweredIncorrect.Invoke();
            }

            yield return new WaitForSeconds(flashDuration);

            currentQuestionIndex++;
            DisplayQuestion();
            isWaitingForAnimation = false;
            
            if (eventInstance.isValid())
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release();
            }
        }

        yield return new WaitForSeconds(0.1f); // Wait briefly for visual feedback

        isWaitingForAnimation = false;
    }

    public void OnSubmitButtonPressed()
    {
        // Validate answers and move to the next question
        ValidateCurrentQuestionAnswers();
        currentQuestionIndex++;
        DisplayQuestion();
    }

    private void ValidateCurrentQuestionAnswers()
    {
        var question = questions[currentQuestionIndex];
        var selectedForThisQuestion = selectedAnswers[currentQuestionIndex];

        // Validate the selected answers
        if (ValidateMultipleChoiceAnswers(question, selectedForThisQuestion))
        {
            correctQuestions++;
            onQuestionAnsweredCorrectly.Invoke();
            UpdateCorrectAnswersText();
        }
        else
        {
            onQuestionAnsweredIncorrect.Invoke();
        }

        // Reset next button state
        submitButton.SetActive(false);
    }

    private bool ValidateMultipleChoiceAnswers(Question question, List<int> selectedIndices)
    {
        var correctIndices = new List<int>();
        for (int i = 0; i < question.options.Count; i++)
        {
            if (question.options[i].isCorrect)
            {
                correctIndices.Add(i);
            }
        }

        // Check if the selected answers match the correct ones (ignoring order)
        return !selectedIndices.Except(correctIndices).Any() && !correctIndices.Except(selectedIndices).Any();
    }

    public void ResetQuiz()
    {
        currentQuestionIndex = 0;
        correctQuestions = 0;
        totalQuestions = 0;
        UpdateCorrectAnswersText();
        prevButton.SetActive(false);
        nextButton.SetActive(false);
        submitButton.SetActive(false); // Reset the submit button
    }

    public void RestartQuiz()
    {
        ResetQuiz();
        StartQuiz();
    }

    private void UpdateCorrectAnswersText()
    {
        Debug.Log($"Quiz finished. Correct answers: {correctQuestions}/{totalQuestions}");
        var str = quizEndText;

        str = str.Replace("{correctQuestions}", correctQuestions.ToString());
        str = str.Replace("{totalQuestions}", totalQuestions.ToString());
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
        if (isWaitingForAnimation) return;

        obj.GetComponent<QuizButton>().OnClick();
    }

    private void DisplayReviewQuestion()
    {
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
                if (selectedAnswers[currentReviewIndex].Contains(i))
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

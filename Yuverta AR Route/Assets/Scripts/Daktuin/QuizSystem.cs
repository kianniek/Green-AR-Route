using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

    [Header("Quiz UI Elements")] [Tooltip("The text element that will display the quiz question")] [SerializeField]
    private TMP_Text questionText;

    [Tooltip(
        "The text element that will display the number of correct answers. Use {correctQuestions} as a placeholder for the actual number of correct answers")]
    [SerializeField]
    private TMP_Text correctAnswersText; // Display correct answers count

    [SerializeField] private float answerFeedbackDelay = 2f; // Delay time in seconds


    [SerializeField] private GameObject correctAnswersDisplay; // Display correct answers count
    [SerializeField] private GameObject nextButton; // The button to proceed to the next question
    [SerializeField] private GameObject submitButton; // The button to submit answers for multiple-choice questions

    [Tooltip("All the buttons that will be used to answer the quiz questions")] [SerializeField]
    private QuizButton[] choiceButtons;

    [SerializeField] private Color correctAnswerColor = Color.green;
    [SerializeField] private Color incorrectAnswerColor = Color.red;
    [SerializeField] private Color multipleChoiceAnswerColor = Color.yellow;
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

    [SerializeField] private GameObject prevButton;
    private int currentReviewIndex = 0;

    private List<List<int>> selectedAnswers; // To store indices of selected answers for each question
    private int requiredCorrectAnswerCount; // Required number of correct answers for multiple choice
    private bool isMultipleChoiceQuestion; // Flag to indicate if the current question is multiple choice

    private void Awake()
    {
        //if we are not in the editor diable the start quiz on start
        #if !UNITY_EDITOR
            startQuizOnStart = false;
        #endif
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
        if (quizQuestions == null || quizQuestions.questions == null || quizQuestions.questions.Count == 0)
        {
            Debug.LogError("No quiz questions available!");
            return;
        }

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
        questionText.transform.parent.gameObject.SetActive(true);

        selectedAnswers = new List<List<int>>();
        for (var i = 0; i < questions.Count; i++)
        {
            selectedAnswers.Add(new List<int>()); // Initialize empty list for each question
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

            if (eventReference[currentQuestionIndex].ToString() != "")
            {
                eventInstance = FMODUnity.RuntimeManager.CreateInstance(eventReference[currentQuestionIndex]);
                eventInstance.start();
                Debug.Log("FMOD event started");
                Debug.Log(eventReference[currentQuestionIndex].ToString());
                Debug.Log(eventInstance.isValid());
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
        questionText.transform.parent.gameObject.SetActive(false);

        if (eventInstance.isValid())
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }

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

        if (selectedForThisQuestion.Contains(index))
        {
            // Deselect the answer
            selectedForThisQuestion.Remove(index);
            button.ResetVisualsColor(); // Reset to default visuals
        }
        else
        {
            // Select the answer
            selectedForThisQuestion.Add(index);
            button.SetButtonColor(multipleChoiceAnswerColor); // Indicate selection with a color
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
            // Validate the user's selection for single choice
            bool isCorrect = questions[currentQuestionIndex].options[selectedForThisQuestion[0]].isCorrect;

            if (isCorrect)
            {
                button.FlashButton(correctAnswerColor, answerFeedbackDelay);
                correctQuestions++;
                onQuestionAnsweredCorrectly.Invoke();
                UpdateCorrectAnswersText();
            }
            else
            {
                button.FlashButton(incorrectAnswerColor, answerFeedbackDelay);
                incorrectAnswers.Add(questions[currentQuestionIndex].options[selectedForThisQuestion[0]]);
                onQuestionAnsweredIncorrect.Invoke();
            }

            // Wait for the feedback duration before revealing correct answers
            ShowCorrectAnswers(selectedForThisQuestion); // Call to show correct answers;
            yield return new WaitForSeconds(answerFeedbackDelay);
            currentQuestionIndex++;
            DisplayQuestion();
            isWaitingForAnimation = false;
        }

        yield return new WaitForSeconds(0.1f); // Wait briefly for visual feedback
        isWaitingForAnimation = false;
    }

    private void ShowCorrectAnswers(List<int> selectedForThisQuestion, bool reviewMode = false)
    {
        var question = reviewMode ? questions[currentReviewIndex] : questions[currentQuestionIndex];

        for (int i = 0; i < question.options.Count; i++)
        {
            if (question.options[i].isCorrect && !selectedForThisQuestion.Contains(i))
            {
                // Highlight the correct answer that was not selected
                choiceButtons[i].SetButtonColor(Color.Lerp(correctAnswerColor, multipleChoiceAnswerColor, 0.75f));
            }
            
            if (!question.options[i].isCorrect && selectedForThisQuestion.Contains(i))
            {
                // Highlight the incorrect answer that was selected
                choiceButtons[i].SetButtonColor(incorrectAnswerColor);
            }
            
            if (question.options[i].isCorrect && selectedForThisQuestion.Contains(i))
            {
                // Highlight the correct answer that was selected
                choiceButtons[i].SetButtonColor(correctAnswerColor);
            }
        }
    }

    public void OnSubmitButtonPressed()
    {
        // Validate answers and move to the next question
        StartCoroutine(ValidateCurrentQuestionAnswers());
    }

    IEnumerator ValidateCurrentQuestionAnswers()
    {
        var question = questions[currentQuestionIndex];
        var selectedForThisQuestion = selectedAnswers[currentQuestionIndex];

        // Validate the selected answers
        if (ValidateMultipleChoiceAnswers(question, selectedForThisQuestion))
        {
            // Wait for the feedback duration before revealing correct answers
            ShowCorrectAnswers(selectedForThisQuestion); // Call to show correct answers;
            yield return new WaitForSeconds(answerFeedbackDelay);
            correctQuestions++;
            onQuestionAnsweredCorrectly.Invoke();
            UpdateCorrectAnswersText();
        }
        else
        {
            // Wait for the feedback duration before revealing correct answers
            ShowCorrectAnswers(selectedForThisQuestion); // Call to show correct answers;
            yield return new WaitForSeconds(answerFeedbackDelay);
            onQuestionAnsweredIncorrect.Invoke();
        }

        // Reset next button state
        submitButton.SetActive(false);
        
        currentQuestionIndex++;
        DisplayQuestion();

        yield return null;
    }

    private bool ValidateMultipleChoiceAnswers(Question question, List<int> selectedIndices)
    {
        var correctIndices = question.options
            .Select((option, index) => option.isCorrect ? index : -1)
            .Where(index => index != -1)
            .ToList();

        return selectedIndices.Count == correctIndices.Count && !selectedIndices.Except(correctIndices).Any();
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
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            HandleTouchInput(Input.GetTouch(0));
        }
        else if (Input.GetMouseButtonDown(0))
        {
            HandleMouseInput();
        }
    }

    private void HandleTouchInput(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            CheckForQuizButtonHit(ray);
        }
    }

    private void HandleMouseInput()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        CheckForQuizButtonHit(ray);
    }

    private void CheckForQuizButtonHit(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag(buttonTag))
        {
            SelectedObject(hit.collider.gameObject);
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

        questionText.transform.parent.gameObject.SetActive(false);

        if (currentReviewIndex >= totalQuestions)
        {
            correctAnswersDisplay.SetActive(true);
            UpdateNavigationButtons();
            return;
        }

        correctAnswersDisplay.SetActive(false);
        var question = questions[currentReviewIndex];
        questionText.text = question.questionText;
        questionText.transform.parent.gameObject.SetActive(true);

        for (var i = 0; i < choiceButtons.Length; i++)
        {
            if (i < question.options.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].ButtonText = question.options[i].answer;

                // Highlight the selected answer
                // if (selectedAnswers[currentReviewIndex].Contains(i))
                // {
                //     // if (question.options[i].isCorrect)
                //     // {
                //     //     choiceButtons[i].SetButtonColor(correctAnswerColor); // User selected the correct answer
                //     // }
                //     // else
                //     // {
                //     //     choiceButtons[i].SetButtonColor(incorrectAnswerColor); // User selected the wrong answer
                //     // }
                //     
                // }
                // else
                // {
                //     choiceButtons[i].ResetVisualsColor(); // Reset to default color
                // }
                
                ShowCorrectAnswers(selectedAnswers[currentReviewIndex], true);
            }
        }

        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        prevButton.SetActive(true);
        nextButton.SetActive(true);
        prevButton.GetComponent<Button>().interactable = (currentReviewIndex > 0); // Hide if at first question
        nextButton.GetComponent<Button>().interactable =
            (currentReviewIndex < totalQuestions); // Hide if at last question
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
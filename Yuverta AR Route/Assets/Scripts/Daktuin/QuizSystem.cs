using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class QuizManager : MonoBehaviour
{
    [SerializeField] private UnityEvent onQuizFinished = new();
    [SerializeField] private UnityEvent onQuestionAnsweredCorrectly = new();

    [Header("Quiz UI Elements")]
    [Tooltip("The text element that will display the quiz question")]
    [SerializeField] private TextMeshPro questionText;

    [Tooltip("All the buttons that will be used to answer the quiz questions")]
    [SerializeField] private QuizButton[] choiceButtons;
    [SerializeField] private string buttonTag = "QuizButton";

    [Tooltip("The scriptable object containing the quiz questions")]
    [SerializeField] private QuizQuestions quizQuestions;

    private List<Question> questions;
    private int currentQuestionIndex;
    private int correctQuestions;
    private int totalQuestions;

    private void Start()
    {
        InitializeQuiz();
        DisplayQuestion();
    }

    private void InitializeQuiz()
    {
        questions = new List<Question>(quizQuestions.questions);
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
            button.onRaycastHit.AddListener(OnOptionSelected);
        }
    }

    private void DisplayQuestion()
    {
        if (currentQuestionIndex < questions.Count)
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
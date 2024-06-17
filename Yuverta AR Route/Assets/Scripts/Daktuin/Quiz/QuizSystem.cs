using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public UnityEvent onQuizFinished;
    public UnityEvent onQuestionAnsweredCorrectly;

    [Header("Quiz UI Elements")] 
    [Tooltip("The parent object containing all other objects")]
    public GameObject parentObject;
    
    [Tooltip("The text element that will display the quiz question")]
    public TextMeshPro questionText;
    
    [Tooltip("All the buttons that will be used to answer the quiz questions")]
    private QuizButton[] choiceButtons;
    
    [Tooltip("The scriptable object containing the quiz questions")]
    public QuizQuestions quizQuestions;
    
    [Tooltip("The distance the quiz elements should be from the camera")]
    public float distanceFromCamera;
    private List<Question> questions;
    private int currentQuestionIndex;
    private int correctQuestions;
    private int totalQuestions;
    
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        // Calculate the position in front of the camera
        Vector3 newPosition = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera;

        // Set the position of the object
        parentObject.transform.position = newPosition;

        // Make the camera the parent of the object
        parentObject.transform.SetParent(mainCamera.transform);
        
        questions = new List<Question>();
        foreach (var quiz in quizQuestions.questions)
        {
            questions.Add(quiz);
        }
        
        if (choiceButtons.Length == 0)
        {
            choiceButtons = new QuizButton[4];
            var buttons = FindObjectsOfType<QuizButton>();
            foreach (var button in buttons)
            {
                choiceButtons[button.buttonIndex] = button;
            }
        }
        
        foreach (var buttons in choiceButtons)
        {
            buttons.gameObject.SetActive(false);
        }

        foreach (var buttons in choiceButtons)
        {
            buttons.onRaycastHit.AddListener(OnOptionSelected);
        }
        DisplayQuestion();
    }

    void DisplayQuestion()
    {
        if (currentQuestionIndex < questions.Count)
        {
            Question question = questions[currentQuestionIndex];
            questionText.text = question.questionText;;
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
            onQuizFinished.Invoke();
            Debug.Log("Quiz Finished");
        }
    }

    public void OnOptionSelected(int index)
    {
        if (index == questions[currentQuestionIndex].options.FindIndex(option => option.isCorrect))
        {
            correctQuestions++;
            totalQuestions++;
            currentQuestionIndex++;
            onQuestionAnsweredCorrectly.Invoke();
            DisplayQuestion();
            Debug.Log("Correct Answer");
        }
        else
        {
            totalQuestions++;
            Debug.Log("Wrong Answer");
        }
    }

    public void ResetQuiz()
    {
        currentQuestionIndex = 0;
        correctQuestions = 0;
        totalQuestions = 0;
        DisplayQuestion();
    }
}
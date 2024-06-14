using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public List<string> options;
        public int correctAnswerIndex;
    }

    public TextMeshProUGUI questionText;
    public List<Button> optionButtons;
    public List<Question> questions;
    private int currentQuestionIndex;

    void Start()
    {
        DisplayQuestion();
    }

    void DisplayQuestion()
    {
        if (currentQuestionIndex < questions.Count)
        {
            Question question = questions[currentQuestionIndex];
            questionText.text = question.questionText;
            for (int i = 0; i < optionButtons.Count; i++)
            {
                if (i < question.options.Count)
                {
                    optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.options[i];
                    optionButtons[i].gameObject.SetActive(true);
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.Log("Quiz Finished");
        }
    }

    public void OnOptionSelected(int index)
    {
        if (index == questions[currentQuestionIndex].correctAnswerIndex)
        {
            Debug.Log("Correct Answer");
        }
        else
        {
            Debug.Log("Wrong Answer");
        }
        currentQuestionIndex++;
        DisplayQuestion();
    }
}
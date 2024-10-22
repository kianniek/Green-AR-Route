using System;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

[Serializable]
public struct QuizAnswer
{
    public string answer;
    public bool isCorrect;

    public QuizAnswer(string answer, bool isCorrect)
    {
        this.answer = answer;
        this.isCorrect = isCorrect;
    }
}

[Serializable]
public class Question
{
    public string questionText;
    public List<QuizAnswer> options;
}

[CreateAssetMenu]
public class QuizQuestions : ScriptableObject
{
    public List<Question> questions;
}


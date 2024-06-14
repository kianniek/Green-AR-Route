using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public enum PointValues
{
    Building = 0,
     KeyTarget = 3000
    // Add more types as needed
}

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private int planeScore;
    private int totalScore = 0;
    private SplatManager splatManager;

    private void Start()
    {
        splatManager = FindObjectOfType<SplatManager>();
        scoreText.text = "Score: " + totalScore;
    }

    public void AddScore(PointValues pointValues = PointValues.Building)
    {
        var newScore = splatManager.scores.x - planeScore;
        if (newScore <= 0) return;
        planeScore = (int)splatManager.scores.x;
        totalScore += (int)newScore * 100
                      + Random.Range(0, 100) 
                      + (int)pointValues;
        scoreText.text = "Score: " + totalScore;
    }
}

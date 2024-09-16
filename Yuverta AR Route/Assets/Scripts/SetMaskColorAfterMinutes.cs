using System.Collections;
using TMPro;
using UnityEngine;

public class SetMaskColorAfterMinutes : MonoBehaviour
{
    public float minutes = 3f;
    public int paintColorIndexToSet;
    public PaintColors paintColors;
    [SerializeField] private TMP_Text timerText;

    private Paintable[] paintables;
    private float remainingTime;

    private void Start()
    {
        paintables = FindObjectsOfType<Paintable>();
        remainingTime = minutes * 60; // Convert minutes to seconds

        if (timerText != null)
        {
            UpdateTimerText(remainingTime); // Initial display of the timer
        }
    }

    private void OnEnable()
    {
        StartCoroutine(SetMaskColor());
    }

    private void OnDisable()
    {
        StopCoroutine(SetMaskColor());
    }

    private IEnumerator SetMaskColor()
    {
        remainingTime = minutes * 60; // Convert minutes to seconds
        Debug.Log($"1: {remainingTime}");
        while (remainingTime > 0f)
        {
            yield return new WaitForSeconds(1f);

            remainingTime -= 1f;
            Debug.Log($"2: {remainingTime}");

            UpdateTimerText(remainingTime);

            yield return null;
        }

        foreach (var p in paintables)
        {
            yield return new WaitForEndOfFrame();
            Paintable.SetMaskToColor(p, paintColors.colors[paintColorIndexToSet], paintColorIndexToSet);
            p.OnCovered.Invoke(paintColorIndexToSet);
        }
    }

    private void UpdateTimerText(float timeLeft)
    {
        // Convert time left to minutes and seconds format
        var minutesLeft = Mathf.FloorToInt(timeLeft / 60);
        var secondsLeft = Mathf.FloorToInt(timeLeft % 60);

        // Update the text to show the time in mm:ss format
        timerText.text = string.Format("{0:00}:{1:00}", minutesLeft, secondsLeft);

        Debug.Log("Time left: " + timerText.text);
    }
}
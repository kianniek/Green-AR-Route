using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SetMaskColorAfterMinutes : MonoBehaviour
{
    public float minutes = 3f;
    public int paintColorIndexToSet;
    public PaintColors paintColors;
    [SerializeField] private TMP_Text timerText;

    public List<ObjectGrower> objectGrower;
    private Paintable[] paintables;

    private bool isTimerRunning;

    public float RemainingTime { private set; get; }

    public float timeBetweenObjectToSetMask = 1f;

    private void Start()
    {
        paintables = FindObjectsOfType<Paintable>();
        RemainingTime = minutes * 60; // Convert minutes to seconds

        if (timerText != null)
        {
            UpdateTimerText(RemainingTime); // Initial display of the timer
        }

        //get all object grower scripts
        var objectGrowers = FindObjectsOfType<ObjectGrower>();

        foreach (var objGrower in objectGrowers)
        {
            if (!objectGrower.Contains(objGrower))
            {
                objectGrower.Add(objGrower);
            }
        }
    }

    public void StartTimer()
    {
        //check if the timer is already running
        if (isTimerRunning)
        {
            return;
        }

        StartCoroutine(SetMaskColor());
    }

    private void OnDisable()
    {
        StopCoroutine(SetMaskColor());
    }

    private IEnumerator SetMaskColor()
    {
        RemainingTime = minutes * 60; // Convert minutes to seconds
        Debug.Log($"1: {RemainingTime}");
        while (RemainingTime > 0f)
        {
            isTimerRunning = true;
            yield return new WaitForSeconds(1f);

            RemainingTime -= 1f;

            UpdateTimerText(RemainingTime);

            yield return null;
        }

        isTimerRunning = false;

        foreach (var p in paintables)
        {
            if (timeBetweenObjectToSetMask > 0)
            {
                yield return new WaitForSeconds(timeBetweenObjectToSetMask);
            }

            Paintable.SetMaskToColor(p, paintColors.colors[paintColorIndexToSet], paintColorIndexToSet);
            p.OnCovered.Invoke(paintColorIndexToSet);
        }

        foreach (var obj in objectGrower)
        {
            obj.GrowChildObjects(paintColorIndexToSet);
        }
    }

    private void UpdateTimerText(float timeLeft)
    {
        // Convert time left to minutes and seconds format
        var minutesLeft = Mathf.FloorToInt(timeLeft / 60);
        var secondsLeft = Mathf.FloorToInt(timeLeft % 60);

        // Update the text to show the time in mm:ss format
        timerText.text = string.Format("{0:00}:{1:00}", minutesLeft, secondsLeft);
    }

    public float GetTimeLeft01()
    {
        return RemainingTime / (minutes * 60);
    }
}
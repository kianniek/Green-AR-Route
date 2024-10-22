using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ImageRecognitionData", menuName = "ScriptableObjects/ImageRecognitionData", order = 1)]
public class ImageRecognitionData : ScriptableObject
{
    public int recognitionCount = 0; // Count of recognized images
    public int recognitionThreshold = 5; // Threshold for triggering the event

    // Method to check the threshold and invoke the event
    public bool CheckThreshold()
    {
        if (recognitionCount >= recognitionThreshold)
        {
            return true;
        }

        return false;
    }

    // Method to increment the recognition count
    public void IncrementCount()
    {
        recognitionCount++;
        CheckThreshold(); // Check if the threshold is met after incrementing
    }
}
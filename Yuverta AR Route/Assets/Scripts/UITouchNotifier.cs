using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpriteTouchNotifier : MonoBehaviour
{
    public GraphicRaycaster raycaster; // Reference to the Canvas's GraphicRaycaster
    public EventSystem eventSystem; // Reference to the EventSystem
    public GameObject[] targetSprites; // Array of specific UI elements you want to detect touches on

    public bool CheckIfTouchBeganCoveredByUI(Touch touch)
    {
        // Create a PointerEventData to use with the Raycaster
        var pointerEventData = new PointerEventData(eventSystem)
        {
            position = touch.position
        };

        // Raycast using the GraphicRaycaster
        var raycastResults = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, raycastResults);

        // Iterate over the results to see if any match the target UI elements
        foreach (var result in raycastResults)
        {
            foreach (var targetSprite in targetSprites)
            {
                if (result.gameObject != targetSprite) continue;
                Debug.Log($"Target UI element {targetSprite.name} touched!");

                // Add any additional logic here
                return true;
            }
        }
        
        // If we reach this point, no target UI elements were touched
        return false;
    }
}
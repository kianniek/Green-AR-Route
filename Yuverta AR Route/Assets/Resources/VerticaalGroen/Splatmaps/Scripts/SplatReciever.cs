using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatReciever : MonoBehaviour
{
    // need to add all the renderers before Start of Splat Manager
    private void Awake()
    {
        AddToSplatManager();
    }
    
    private void AddToSplatManager()
    {
        // Add the renderer to the SplatManager
        var thisRenderer = gameObject.GetComponent<Renderer>();
        if (thisRenderer != null)
        {
            SplatManagerSystem.instance.AddRenderer(thisRenderer);
        }
    }
}
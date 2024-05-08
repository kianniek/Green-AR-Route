using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is to be used to adjust all settings that are different for every scene
public class BaseSceneScript : MonoBehaviour
{
    private enum Orientation
    {
        Portrait,
        Landscape
    }

    [SerializeField]
    private Orientation screenOrientation;

    void Start()
    {
        switch (screenOrientation)
        {
            case Orientation.Portrait:
                Screen.orientation = ScreenOrientation.Portrait;
                break;
            case Orientation.Landscape:
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                break;
        }
    }
}

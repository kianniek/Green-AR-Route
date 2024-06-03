using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//add editor to center objects vertically
[CustomEditor(typeof(CenterVertically))]
public class CenterVerticallyEditor : Editor
{
    //add a button to the inspector to center the objects vertically
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CenterVertically centerVertically = (CenterVertically) target;
        if (GUILayout.Button("Center Vertically"))
        {
            centerVertically.CenterObjects();
        }

        if (GUILayout.Button("Move Centered Objects Back"))
        {
            centerVertically.MoveCenteredObjectsBack();
        }
    }
}

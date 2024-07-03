using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectLogic))]
[CanEditMultipleObjects]
public class ObjectLogicEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get the target object
        var script = (ObjectLogic)target;
        
        if (GUILayout.Button("Shake Object"))
        {
            script.ShakeObject();
        }
        if (GUILayout.Button("Stop Shake Object"))
        {
            script.StopShaking();
        }
    }
}
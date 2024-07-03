using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LerpedObjectMovement))]
[CanEditMultipleObjects]
public class LerpedObjectMovementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get the target object
        var script = (LerpedObjectMovement)target;
    }
}
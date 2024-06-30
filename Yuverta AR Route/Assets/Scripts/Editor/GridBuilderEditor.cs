using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridBuilder))]
public class GridBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Draw the default inspector

        GridBuilder gridBuilder = (GridBuilder)target; // Cast the target to GridBuilder

        // Add a button to the inspector
        if (GUILayout.Button("Build Grid"))
        {
            // Call the BuildGrid method when the button is clicked
            gridBuilder.BuildGrid();
        }

        if (GUILayout.Button("Clear Grid"))
        {
            // Optionally add a clear button to remove the grid
            gridBuilder.ClearGrid();
        }
        
        if (GUILayout.Button("Converge Grid"))
        {
            // Optionally add a clear button to remove the grid
            gridBuilder.MoveGridPointsToConvergedPosition();
        }
        
        if (GUILayout.Button("Unconverge Grid"))
        {
            // Optionally add a clear button to remove the grid
            gridBuilder.MoveGridPointsToOriginalPosition();
        }
    }
}

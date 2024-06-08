using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(WeatherHandler))]
public class WeatherCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(20f);
        
        var weatherHandler = (WeatherHandler)target;
        EditorGUILayout.IntField("Snow particles:", weatherHandler.snow.aliveParticleCount);
        EditorGUILayout.IntField("Rain particles:", weatherHandler.rain.aliveParticleCount);

        if (GUILayout.Button("Stop particles"))
        {
            weatherHandler.Stop();
        }
    }
}
#endif


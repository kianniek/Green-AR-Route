using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowShaderController : MonoBehaviour
{
    public Shader snowShader;
    [SerializeField]
    [Range(0, 1)] private float snowAmount = 0.0f;
    private static readonly int SnowAmount = Shader.PropertyToID("_SnowAmount");

    private void Update()
    {
        Shader.SetGlobalFloat(SnowAmount, snowAmount);
    }
}

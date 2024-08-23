using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxShaderController : MonoBehaviour
{
    public string valueName;
    
    private float value;
    private Material skyboxMaterial;

    private void Awake()
    {
        skyboxMaterial = GetComponent<Renderer>().material;
    }

    // Start is called before the first frame update
    public void SetFloat(float value)
    {
        this.value = value;
        skyboxMaterial.SetFloat(valueName, value);
    }
}

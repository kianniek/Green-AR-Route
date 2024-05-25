using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SnowMaterialController : MonoBehaviour
{
    private bool isSnowing;

    public bool IsSnowing
    {
        get => isSnowing;
        set
        {
            isSnowing = value;
            SetSnowing(isSnowing);
        }
    }

    [Header("Snow Material")]
    [SerializeField] private Material snowMaterial;  // Assign your material in the Inspector

    [Header("Material Properties")]
    [SerializeField] private string normalThresholdProperty = "_NormalThreshold_snow";
    [SerializeField] private string contrastProperty = "_contrast_snow";
    [SerializeField] private string noiseStrengthProperty = "_NoiseStrength_snow";
    [SerializeField] private string displacementStrengthProperty = "_DisplacementStrength_snow";

    [Header("Snow Settings")]
    public float normalThreshold;
    public float contrast;
    public float noiseStrength;
    public float displacementStrength;
    
    [Header("Snow Animator")]
    [SerializeField] private Animator snowAnimator;
    private void Awake()
    {
        if(snowAnimator == null)
        {
            snowAnimator = GetComponent<Animator>();
        }
        if (snowMaterial == null)
        {
            // Get the material of the object this script is attached to (if not assigned in the Inspector
            snowMaterial = GetComponent<Renderer>().material;
        }
        
        // Set the default values
        normalThreshold = snowMaterial.GetFloat(normalThresholdProperty);
        contrast = snowMaterial.GetFloat(contrastProperty);
        noiseStrength = snowMaterial.GetFloat(noiseStrengthProperty);
        displacementStrength = snowMaterial.GetFloat(displacementStrengthProperty);
    }

    void Update()
    {
        snowMaterial.SetFloat(normalThresholdProperty, normalThreshold);
        snowMaterial.SetFloat(contrastProperty, contrast);
        snowMaterial.SetFloat(noiseStrengthProperty, noiseStrength);
        snowMaterial.SetFloat(displacementStrengthProperty, displacementStrength);
    }
    
    public void ToggleSnowing()
    {
        IsSnowing = !IsSnowing;
    }
    private void SetSnowing(bool isSnowing)
    {
        snowAnimator.SetBool("isSnowing", isSnowing);
    }
}
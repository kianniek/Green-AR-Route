using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MaterialController : MonoBehaviour
{
    private bool isSnowing;
    private bool isRaining;

    public bool IsSnowing
    {
        get => isSnowing;
        set
        {
            isSnowing = value;
            SetSnowing(isSnowing);
        }
    }
    
    public bool IsRaining
    {
        get => isRaining;
        set
        {
            isRaining = value;
            SetRaining(isRaining);
        }
    }

    [Header("Snow Material")]
    [SerializeField] private Material snowMaterial;  // Assign your material in the Inspector

    [Header("Snow Material Properties")]
    [SerializeField] private string normalThresholdProperty = "_NormalThreshold_snow";
    [SerializeField] private string contrastProperty = "_contrast_snow";
    [SerializeField] private string noiseStrengthProperty = "_NoiseStrength_snow";
    [SerializeField] private string displacementStrengthProperty = "_DisplacementStrength_snow";
    
    [Header("Rain Material Properties")]
    [SerializeField] private string rainNormalThresholdProperty = "_NormalThreshold_rain";
    [SerializeField] private string rainContrastProperty = "_contrast_rain";

    [Header("Snow Settings")]
    public float normalThreshold;
    public float contrast;
    public float noiseStrength;
    public float displacementStrength;
    
    [Header("Rain Settings")]
    public float normalThresholdRain;
    public float contrastRain;
    
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
        
        normalThresholdRain = snowMaterial.GetFloat(rainNormalThresholdProperty);
        contrastRain = snowMaterial.GetFloat(rainContrastProperty);
    }

    void Update()
    {
        snowMaterial.SetFloat(normalThresholdProperty, normalThreshold);
        snowMaterial.SetFloat(contrastProperty, contrast);
        snowMaterial.SetFloat(noiseStrengthProperty, noiseStrength);
        snowMaterial.SetFloat(displacementStrengthProperty, displacementStrength);
        
        snowMaterial.SetFloat(rainNormalThresholdProperty, normalThresholdRain);
        snowMaterial.SetFloat(rainContrastProperty, contrastRain);
    }
    
    public void ToggleSnowing()
    {
        IsSnowing = !IsSnowing;
    }
    private void SetSnowing(bool isSnowing)
    {
        snowAnimator.SetBool("isSnowing", isSnowing);
    }
    
    public void ToggleRaining()
    {
        IsRaining = !IsRaining;
    }
    
    private void SetRaining(bool isRaining)
    {
        snowAnimator.SetBool("isRaining", isRaining);
    }
}
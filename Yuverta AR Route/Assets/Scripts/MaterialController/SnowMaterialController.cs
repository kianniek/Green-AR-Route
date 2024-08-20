using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class MaterialController : MonoBehaviour
{
    [Serializable]
    public struct MaterialColorChange
    {
        [SerializeField]public Material material;
        [SerializeField]public string colorProperty;
        [SerializeField]public Color targetColor;
        [SerializeField]public Color originalColor;
        [SerializeField][Range(-1f,1f)]
        public float transition;
    }
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

    [FormerlySerializedAs("snowMaterial")] [Header("Snow Material")] [SerializeField]
    private Material[] snowMaterials; // Assign your material in the Inspector
    
    [SerializeField]
    private float masterSliderValue = 1f;
    [SerializeField]
    private MaterialColorChange[] snowMaterialSpecial; // Assign your material in the Inspector

    [Header("Snow Material Properties")] [SerializeField]
    private string normalThresholdProperty = "_NormalThreshold_snow";

    [SerializeField] private string contrastProperty = "_contrast_snow";
    [SerializeField] private string noiseStrengthProperty = "_NoiseStrength_snow";
    [SerializeField] private string displacementStrengthProperty = "_DisplacementStrength_snow";

    [Header("Rain Material Properties")] [SerializeField]
    private string rainNormalThresholdProperty = "_NormalThreshold_rain";

    [SerializeField] private string rainContrastProperty = "_contrast_rain";
    [SerializeField] private string rainFresnelMinProperty = "_contrast_rain";
    [SerializeField] private string rainFresnelMaxProperty = "_contrast_rain";

    [Header("Snow Settings")] public float normalThreshold;
    public float contrast;
    public float noiseStrength;
    public float displacementStrength;

    [Header("Rain Settings")] public float normalThresholdRain;
    public float contrastRain;
    [Range(0, 2)] public float fresnelMinRain;
    [Range(0, 2)] public float fresnelMaxRain;

    [Header("Snow Animator")] [SerializeField]
    private Animator snowAnimator;

    private void Awake()
    {
        if (snowAnimator == null)
        {
            snowAnimator = GetComponent<Animator>();
        }

        foreach (Material snowMaterial in snowMaterials)
        {
            // Set the default values
            normalThreshold = snowMaterial.GetFloat(normalThresholdProperty);
            contrast = snowMaterial.GetFloat(contrastProperty);
            noiseStrength = snowMaterial.GetFloat(noiseStrengthProperty);
            displacementStrength = snowMaterial.GetFloat(displacementStrengthProperty);

            normalThresholdRain = snowMaterial.GetFloat(rainNormalThresholdProperty);
            contrastRain = snowMaterial.GetFloat(rainContrastProperty);
            fresnelMinRain = snowMaterial.GetFloat(rainFresnelMinProperty);
            fresnelMaxRain = snowMaterial.GetFloat(rainFresnelMaxProperty);
        }
    }

    private void LateUpdate()
    {
        foreach (var snowMaterial in snowMaterials)
        {
            snowMaterial.SetFloat(normalThresholdProperty, normalThreshold);
            snowMaterial.SetFloat(contrastProperty, contrast);
            snowMaterial.SetFloat(noiseStrengthProperty, noiseStrength);
            snowMaterial.SetFloat(displacementStrengthProperty, displacementStrength);

            snowMaterial.SetFloat(rainNormalThresholdProperty, normalThresholdRain);
            snowMaterial.SetFloat(rainContrastProperty, contrastRain);
            snowMaterial.SetFloat(rainFresnelMinProperty, fresnelMinRain);
            snowMaterial.SetFloat(rainFresnelMaxProperty, fresnelMaxRain);
        }
        
        foreach (var materialColorChange in snowMaterialSpecial)
        {
            materialColorChange.material.SetColor(materialColorChange.colorProperty, Color.Lerp(materialColorChange.originalColor, materialColorChange.targetColor, masterSliderValue + materialColorChange.transition));
        }
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LerpedLight : MonoBehaviour
{
    public Light light;
    public float minIntensity;
    public float maxIntensity;
    public float duration;
    private float t = 0;
    private bool isIncreasing;
    
    public UnityEvent onLightIncreased;
    public UnityEvent onLightDecreased;

    public void ToggleLight()
    {
        isIncreasing = !isIncreasing;
        if (isIncreasing)
        {
            IncreaseLight();
        }
        else
        {
            DecreaseLight();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();
    }

    public void IncreaseLight()
    {
        StartCoroutine(LerpLight(maxIntensity));
        onLightIncreased.Invoke();
    }

    public void DecreaseLight()
    {
        StartCoroutine(LerpLight(minIntensity));
        onLightDecreased.Invoke();
    }

    IEnumerator LerpLight(float targetIntensity)
    {
        var startIntensity = light.intensity;
        while (true)
        {
            t += Time.deltaTime / duration;
            light.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            if (t >= 1)
            {
                t = 0;
                break;
            }

            yield return null;
        }
    }
}
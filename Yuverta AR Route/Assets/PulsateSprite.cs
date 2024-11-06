using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulsateSprite : MonoBehaviour
{
    public bool triggerPulse;
    public float scale = 1.2f;
    public AnimationCurve pulsateCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    public void Pulsate(float duration)
    {
        StartCoroutine(PulsateCoroutine(duration, scale));
    }
    
    private IEnumerator PulsateCoroutine(float duration, float scale)
    {
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float currentScale = pulsateCurve.Evaluate(t) * scale;
            transform.localScale = startScale * currentScale;
            
            yield return null;
        }
        
        transform.localScale = startScale * scale;
    }
    
    private void Update()
    {
        if (triggerPulse)
        {
            triggerPulse = false;
            Pulsate(1f);
        }
    }
}

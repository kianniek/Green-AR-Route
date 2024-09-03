using UnityEngine;
using System.Collections;

public class WetDryMaterial : MonoBehaviour
{
    public float wetSmoothness = 1f;
    public float drySmoothness = 0f;
    public float transitionDuration = 1f;

    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Material material;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        material = objectRenderer.material;
    }

    public void MakeWet()
    {
        StopAllCoroutines();
        StartCoroutine(TransitionSmoothness(wetSmoothness));
    }

    public void MakeDry()
    {
        StopAllCoroutines();
        StartCoroutine(TransitionSmoothness(drySmoothness));
    }

    private IEnumerator TransitionSmoothness(float targetSmoothness)
    {
        float startSmoothness = material.GetFloat("_Smoothness");
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);
            float currentSmoothness = Mathf.Lerp(startSmoothness, targetSmoothness, t);

            material.SetFloat("_Smoothness", currentSmoothness);

            yield return null;
        }

        material.SetFloat("_Smoothness", targetSmoothness);
    }
}
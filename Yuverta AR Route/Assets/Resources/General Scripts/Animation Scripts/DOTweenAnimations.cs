using UnityEngine;
using DG.Tweening;

public class DOTweenAnimations : MonoBehaviour
{
    private Vector3 initialPosition;
    private Tween shakeTween;

    // Exposed parameters for the shake effect
    [Header("Shake Parameters")]
    [Tooltip("If shakeDuration is 0, the shake will be infinite")]
    public float shakeDuration = 0.5f;
    public float shakeStrength = 0.1f;
    public int shakeVibrato = 10;
    public float shakeRandomness = 90f;
    public bool shakeSnapping = false;
    public bool shakeFadeOut = true;

    // Function to create a bounce effect when an object spawns
    public void SpawnBounce(Transform objectToAnimate)
    {
        // Ensure the object starts at a scaled-down size
        objectToAnimate.localScale = Vector3.zero;

        // Animate the scale to create a bounce effect
        objectToAnimate.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBounce);
    }

    // Function to create a shake effect when an object is tapped
    public void TapShake(Transform objectToAnimate)
    {
        // Store the initial position if it's not already stored
        initialPosition = objectToAnimate.position;

        // Shake the position of the object
        objectToAnimate.DOShakePosition(shakeDuration, new Vector3(shakeStrength, shakeStrength, shakeStrength), shakeVibrato, shakeRandomness, shakeSnapping, shakeFadeOut)
            .OnComplete(() => objectToAnimate.position = initialPosition); // Reset position after shake
    }

    // Function to create a scale effect when an object is selected
    public void SelectionScale(Transform objectToAnimate)
    {
        // Ensure the object starts at normal scale
        objectToAnimate.localScale = Vector3.one;

        // Scale up and then scale back to normal
        objectToAnimate.DOScale(Vector3.one * 1.2f, 0.3f)
            .SetLoops(2, LoopType.Yoyo);
    }

    // Function to start an infinite shake
    public void StartInfiniteShake(Transform objectToAnimate)
    {
        // Create an infinite shake
        shakeTween = objectToAnimate.DOShakePosition(
            shakeDuration == 0 ? float.MaxValue : shakeDuration, 
            new Vector3(shakeStrength, shakeStrength, shakeStrength),
            shakeVibrato, shakeRandomness, shakeSnapping, shakeFadeOut)
            .OnComplete(() => objectToAnimate.position = initialPosition); // Reset position after shake;
    }

    // Function to stop the infinite shake
    public void StopInfiniteShake(Transform objectToAnimate)
    {
        // Kill the shake tween if it exists
        if (shakeTween != null && shakeTween.IsActive())
        {
            shakeTween.Kill();
            shakeTween = null;
        }
    }
}

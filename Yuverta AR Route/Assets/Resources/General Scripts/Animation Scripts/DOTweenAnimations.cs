using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DOTweenAnimations : MonoBehaviour
{
    private Vector3 initialPosition;
    private Tween shakeTween;

    // Exposed parameters for the shake effect
    [Header("Shake Parameters")] [Tooltip("If shakeDuration is 0, the shake will be infinite")]
    public float shakeDuration = 0.5f;

    public float shakeStrength = 0.1f;
    public int shakeVibrato = 10;
    public float shakeRandomness = 90f;
    public bool shakeSnapping = false;
    public bool shakeFadeOut = true;

    //dictionaty that stores a transform and its initial position
    private Dictionary<Transform, Vector3> initialPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Tween> Tweens = new Dictionary<Transform, Tween>();

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
        objectToAnimate.DOShakePosition(shakeDuration, new Vector3(shakeStrength, shakeStrength, shakeStrength),
                shakeVibrato, shakeRandomness, shakeSnapping, shakeFadeOut)
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

    // Function to create an infinite shake effect
    public void InfiniteShake(Transform objectToAnimate)
    {
        //if the object is not in the dictionary, add it
        if (!initialPositions.ContainsKey(objectToAnimate))
        {
            initialPositions.Add(objectToAnimate, objectToAnimate.position);
        }

        //if the object is not in the dictionary, add it
        if (!Tweens.ContainsKey(objectToAnimate))
        {
            Tween currentShakeTween;

            // Shake the position of the object infinitely
            currentShakeTween = objectToAnimate.DOShakePosition(shakeDuration, new
                        Vector3(shakeStrength, shakeStrength, 0), shakeVibrato, shakeRandomness, shakeSnapping,
                    shakeFadeOut)
                .SetLoops(-1, LoopType.Restart).OnComplete(() => objectToAnimate.position = initialPositions[objectToAnimate]);

            Tweens.Add(objectToAnimate, currentShakeTween);
        }
    }

    public void StopInfiniteShake(Transform objectToAnimate)
    {
        //if the object is in the dictionary, stop the shake
        if (Tweens.ContainsKey(objectToAnimate))
        {
            Tweens[objectToAnimate].Kill();
            Tweens.Remove(objectToAnimate);
        }

        //if the object is in the dictionary, set its position to the initial position
        if (!initialPositions.ContainsKey(objectToAnimate))
            return;

        objectToAnimate.position = initialPositions[objectToAnimate];

        //remove the object from the dictionary
        initialPositions.Remove(objectToAnimate);
    }
}
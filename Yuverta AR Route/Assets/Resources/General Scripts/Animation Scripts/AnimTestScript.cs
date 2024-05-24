using UnityEngine;

public class AnimTestScript : MonoBehaviour
{
    private DOTweenAnimations dotweenAnimations;

    public enum AnimationType
    {
        SpawnBounce,
        TapShake,
        SelectionScale
    }

    public AnimationType selectedAnimation; // Select the animation in the Inspector

    void Start()
    {
        // Find the DOTweenAnimations component in the scene
        dotweenAnimations = FindObjectOfType<DOTweenAnimations>();
    }

    void OnMouseDown()
    {
        // Play the selected animation when the object is tapped
        switch (selectedAnimation)
        {
            case AnimationType.SpawnBounce:
                dotweenAnimations.SpawnBounce(transform);
                break;
            case AnimationType.TapShake:
                dotweenAnimations.TapShake(transform);
                break;
            case AnimationType.SelectionScale:
                dotweenAnimations.SelectionScale(transform);
                break;
        }
    }
}

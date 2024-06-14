using UnityEngine;
using DG.Tweening;

public class ReloadAnimation : MonoBehaviour
{
    public Transform weaponTransform; // The weapon model transform
    public float reloadDuration = 1f; // The duration of the reload animation

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        // Save the original position and rotation of the weapon
        originalPosition = weaponTransform.localPosition;
        originalRotation = weaponTransform.localRotation;
    }

    public void TriggerReload()
    {
        // Create a sequence for the reload animation
        Sequence reloadSequence = DOTween.Sequence();

        // Add rotation animation to the sequence
        reloadSequence.Append(weaponTransform.DOLocalRotate(new Vector3(0, 0, 45), reloadDuration / 2));
        reloadSequence.Append(weaponTransform.DOLocalRotate(new Vector3(0, 0, 0), reloadDuration / 2));

        // Add position animation to the sequence
        reloadSequence.Insert(0, weaponTransform.DOLocalMoveY(originalPosition.y - 0.2f, reloadDuration / 2));
        reloadSequence.Insert(reloadSequence.Duration() - (reloadDuration / 2), weaponTransform.DOLocalMoveY(originalPosition.y, reloadDuration / 2));

        // Play the sequence
        reloadSequence.Play();
    }
}
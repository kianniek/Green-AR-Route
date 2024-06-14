using UnityEngine;
using DG.Tweening;

public class RecoilAnimation : MonoBehaviour
{
    public Transform weaponTransform; // The weapon model transform
    public float recoilDistance = 0.1f; // The distance of the recoil effect
    public float recoilDuration = 0.1f; // The duration of the recoil effect

    private Vector3 originalPosition;

    void Start()
    {
        // Save the original position of the weapon
        originalPosition = weaponTransform.localPosition;
    }

    public void TriggerRecoil()
    {
        // Move the weapon backward for the recoil effect and then back to its original position
        weaponTransform.DOLocalMoveZ(originalPosition.z - recoilDistance, recoilDuration)
            .OnComplete(() => weaponTransform.DOLocalMoveZ(originalPosition.z, recoilDuration));
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    [Header("References")]
    public GameObject weaponModel;
    public Ammo weaponAmmo;
    
    [Header("Weapon Variables")]
    public WeaponType weaponType;
    public float fireRate;
    public float fireRateCooldownTimer { get; set; }

    // Pistol-specific attributes
    [Header("Pistol Only")]
    public int paintColorIndex; // For painter-style pistols

    // Slingshot-specific attributes
    [Header("Slingshot Only")]
    public int maxCharge;
    public float chargeRate;
    public float launchForce;
}

public enum WeaponType
{
    Pistol,
    Slingshot
}
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class Weapon : ScriptableObject
{
    [Header("Refrences")]
    public GameObject weaponModel;
    public Ammo weaponAmmo;
    
    [Header("Weapon Variables")]
    public WeaponType weaponType;
    public float fireRate;
    public float fireRateCooldownTimer;
    public float reloadTime;
    
    [Header("Ammo Variables")]
    public int magazineSize;
    
    [Header("Spread Only")]
    public int spreadCount; 
    
    //Catapult fields
    [Header("Catapult Only")]
    public int maxCharge;
    public float chargeRate;
    public float launchForce;
    
    [Header("Painter Variables")]
    public int paintColorIndex;
}

public enum WeaponType
{
    Single,
    Spread,
    Automatic,
    Catapult
}

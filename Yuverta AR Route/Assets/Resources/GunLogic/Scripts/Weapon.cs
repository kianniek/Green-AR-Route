using System;
using UnityEngine;

[CreateAssetMenu]
public class Weapon : ScriptableObject
{
    public GameObject prefab;
    public Ammo ammo;
    public int magazineSize;
    public WeaponType weaponType;
    //Serialized in the custom GUI
    [NonSerialized] public float fireRate;
    // This field will only be visible if weaponType is Burst
    [NonSerialized] public float burstRate;
    [NonSerialized] public int burstCount; 
}

public enum WeaponType
{
    Single,
    Burst,
    Automatic
}

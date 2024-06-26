using UnityEngine;

[CreateAssetMenu]
public class Weapon : ScriptableObject
{
    //All serialized in WeaponEditorGUI so add the variables there too
    public GameObject prefab;
    public Ammo ammo;
    public int magazineSize;
    public WeaponType weaponType;
    public int roundsPerMinute;
    // This field will only be visible if weaponType is Burst because of the GUI
    public int burstCount; 
    
    //Catapult fields
    public int maxCharge;
    public float chargeRate;
    public float launchForce;
}

public enum WeaponType
{
    Single,
    Burst,
    Automatic,
    Catapult
}

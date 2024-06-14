using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Weapon))]
public class WeaponEditor : Editor
{
    private int bulletsPerSecond;
    public override void OnInspectorGUI()
    {
        Weapon weapon = (Weapon)target;

        // Custom GUI for Weapon class
        weapon.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", weapon.prefab, typeof(GameObject), false);
        weapon.ammo = (Ammo)EditorGUILayout.ObjectField("Ammo", weapon.ammo, typeof(Ammo), false);
        weapon.weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", weapon.weaponType);
        switch (weapon.weaponType)
        {
            case WeaponType.Burst:
                weapon.magazineSize = EditorGUILayout.IntField("Magazine Size", weapon.magazineSize);
                weapon.roundsPerMinute = EditorGUILayout.IntField("Bursts Per Minute", weapon.roundsPerMinute);
                weapon.burstCount = EditorGUILayout.IntField("Burst Count", weapon.burstCount);
                break;
            case WeaponType.Catapult:
                weapon.maxCharge = EditorGUILayout.IntField("Maximum charge", weapon.maxCharge);
                weapon.chargeRate = EditorGUILayout.FloatField("Charge rate per second", weapon.chargeRate);
                weapon.launchForce = EditorGUILayout.FloatField("Launch force", weapon.launchForce);
                break;
            default:
                weapon.magazineSize = EditorGUILayout.IntField("Magazine Size", weapon.magazineSize);
                weapon.roundsPerMinute = EditorGUILayout.IntField("Rounds Per Minute", weapon.roundsPerMinute);
                break;
        }
    }
}

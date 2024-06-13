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
        weapon.magazineSize = EditorGUILayout.IntField("Magazine Size", weapon.magazineSize);
        weapon.weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", weapon.weaponType);
        weapon.roundsPerMinute = EditorGUILayout.IntField("Rounds Per Minute", weapon.roundsPerMinute);
        if (weapon.weaponType == WeaponType.Burst) weapon.burstCount = EditorGUILayout.IntField("Burst Count", weapon.burstCount);
    }
}

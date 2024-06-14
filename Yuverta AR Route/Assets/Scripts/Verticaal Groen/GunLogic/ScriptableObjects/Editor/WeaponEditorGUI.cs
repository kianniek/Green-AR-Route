using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Weapon))]
public class WeaponEditor : Editor
{
    SerializedProperty prefab;
    SerializedProperty ammo;
    SerializedProperty weaponType;
    SerializedProperty magazineSize;
    SerializedProperty roundsPerMinute;
    SerializedProperty burstCount;
    SerializedProperty maxCharge;
    SerializedProperty chargeRate;
    SerializedProperty launchForce;

    void OnEnable()
    {
        // Link the properties with the serialized object
        prefab = serializedObject.FindProperty("prefab");
        ammo = serializedObject.FindProperty("ammo");
        weaponType = serializedObject.FindProperty("weaponType");
        magazineSize = serializedObject.FindProperty("magazineSize");
        roundsPerMinute = serializedObject.FindProperty("roundsPerMinute");
        burstCount = serializedObject.FindProperty("burstCount");
        maxCharge = serializedObject.FindProperty("maxCharge");
        chargeRate = serializedObject.FindProperty("chargeRate");
        launchForce = serializedObject.FindProperty("launchForce");
    }

    public override void OnInspectorGUI()
    {
        // Start tracking changes to the serialized object
        serializedObject.Update();

        // Draw the fields using serialized properties
        EditorGUILayout.PropertyField(prefab);
        EditorGUILayout.PropertyField(ammo);
        EditorGUILayout.PropertyField(weaponType);

        // Draw fields based on the weapon type
        WeaponType currentWeaponType = (WeaponType)weaponType.enumValueIndex;

        switch (currentWeaponType)
        {
            case WeaponType.Burst:
                EditorGUILayout.PropertyField(magazineSize);
                EditorGUILayout.PropertyField(roundsPerMinute, new GUIContent("Bursts Per Minute"));
                EditorGUILayout.PropertyField(burstCount);
                break;

            case WeaponType.Catapult:
                EditorGUILayout.PropertyField(maxCharge);
                EditorGUILayout.PropertyField(chargeRate);
                EditorGUILayout.PropertyField(launchForce);
                break;

            default:
                EditorGUILayout.PropertyField(magazineSize);
                EditorGUILayout.PropertyField(roundsPerMinute);
                break;
        }

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}

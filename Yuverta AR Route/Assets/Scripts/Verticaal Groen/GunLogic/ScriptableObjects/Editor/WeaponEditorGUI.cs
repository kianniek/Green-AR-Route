using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Weapon))]
public class WeaponEditor : Editor
{
    private int bulletsPerSecond;
    public override void OnInspectorGUI()
    {
        Weapon weapon = (Weapon)target;

        // Draw the default inspector
        DrawDefaultInspector();

        switch (weapon.weaponType)
        {
            case WeaponType.Single:
                weapon.fireRate = CustomFloatField("Fire Rate", weapon.fireRate, 0.75f, 2f);
                break;
            case WeaponType.Burst:
                weapon.burstCount = EditorGUILayout.IntSlider("Burst Count", weapon.burstCount, 3, 10);
                bulletsPerSecond = EditorGUILayout.IntSlider("Bullets per Second", bulletsPerSecond, 1, 10);/*CustomFloatField("Fire Rate", weapon.fireRate, 0.75f, 2f);*/
                weapon.fireRate = bulletsPerSecond / 60f;
                weapon.burstRate = weapon.burstCount / weapon.fireRate;
                break;
            case WeaponType.Automatic:
                weapon.fireRate = CustomFloatField("Fire Rate", weapon.fireRate, 0.0275f, 2f);
                break;
        }
    }

    private float CustomFloatField(string label, float value, float min, float max)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(167.5f));
        value = GUILayout.HorizontalSlider(value, min, max, GUILayout.ExpandWidth(true));
        value = EditorGUILayout.FloatField(value, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();

        return Mathf.Clamp(value, min, max);
    }
}

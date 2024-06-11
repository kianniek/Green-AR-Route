using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Weapon : ScriptableObject
{
    public GameObject prefab;
    public Ammo ammo;
    public float fireRate;
    public int magazineSize;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Ammo : ScriptableObject
{
    public GameObject bulletPrefab;
    public float projectileSpeed;
    public float bulletDrop;
}

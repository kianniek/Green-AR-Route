using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticaalGroenManager : BaseManager
{
    [SerializeField] private List<GameObject> weapons;
    private int currentWeaponIndex = 0;
    private BaseGunScript currentWeapon;
    // Start is called before the first frame update
    void Start()
    {
        SwipeDetection.Instance.currentManager = this;
        SwipeDetection.Instance.tagToCheck = "UI";
        currentWeapon = weapons[currentWeaponIndex].GetComponent<BaseGunScript>();
    }

    /*public override void UpdateObject()
    {
        currentWeapon.Shoot();
    }*/
}

using System.Collections;
using System.Collections.Generic;
using ShellanderGames.WeaponWheel;
using UnityEngine;
using UnityEngine.Events;

public class VerticaalGroenManager : BaseManager
{
    [SerializeField] private List<Weapon> weapons;
    [SerializeField] private SgWeaponWheel wheel;
    [SerializeField] private GameObject gunController;
    private int currentWeaponIndex = 0;
    private BaseGunScript currentWeapon;
    // Start is called before the first frame update
    void Start()
    {
        SwipeDetection.Instance.currentManager = this;
        SwipeDetection.Instance.tagToCheck = "UI";
        if (Camera.main!.transform.childCount > 0) Destroy(Camera.main!.transform.GetChild(0).gameObject);
        var gunControllerInstance = Instantiate(gunController, Camera.main!.transform);
        currentWeapon = gunControllerInstance.GetComponent<BaseGunScript>();
        currentWeapon.ChangeWeapon(weapons[currentWeaponIndex]);
        wheel.OnSliceSelected.AddListener(OnSliceSelected);
        wheel.OnSliceSelected.Invoke(wheel.sliceContents[currentWeaponIndex]);
    }

    private void OnSliceSelected(SgSliceController slice)
    {
        currentWeaponIndex = slice.sliceIndex;
        currentWeapon.ChangeWeapon(weapons[currentWeaponIndex]);
    }
}

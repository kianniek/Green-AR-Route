using System;
using System.Collections;
using System.Collections.Generic;
using ShellanderGames.WeaponWheel;
using UnityEngine;
using UnityEngine.Events;

public class VerticaalGroenManager : BaseManager
{
    public static VerticaalGroenManager Instance;
    [SerializeField] private List<Weapon> weapons;
    [SerializeField] private SgWeaponWheel wheel;
    [SerializeField] private GameObject gunController;
    public ScoreManager scoreManager;
    private int currentWeaponIndex = 0;
    private BaseGunScript currentWeapon;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SwipeDetection.Instance.currentManager = this;
        SwipeDetection.Instance.tagToCheck = "UI";
        if (!scoreManager) scoreManager = FindObjectOfType<ScoreManager>();
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

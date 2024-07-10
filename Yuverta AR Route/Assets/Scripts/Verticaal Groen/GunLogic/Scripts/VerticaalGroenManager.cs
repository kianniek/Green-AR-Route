using ShellanderGames.WeaponWheel;
using UnityEngine;

public class VerticaalGroenManager : Singleton<VerticaalGroenManager>
{
    [SerializeField] private SgWeaponWheel wheel;
    [SerializeField] private GameObject gunController;
    private int currentWeaponIndex = 0;
    private GunController currentWeapon;

    // Start is called before the first frame update
    void Start()
    {
        var gunControllerInstance = gunController;
        
        currentWeapon = gunControllerInstance.GetComponent<GunController>();
        currentWeapon.ChangeWeapon(currentWeaponIndex);
        
        wheel.OnSliceSelected.AddListener(OnSliceSelected);
        wheel.OnSliceSelected.Invoke(wheel.sliceContents[currentWeaponIndex]);
    }

    private void OnSliceSelected(SgSliceController slice)
    {
        currentWeaponIndex = slice.sliceIndex;
        currentWeapon.ChangeWeapon(currentWeaponIndex);
    }
}

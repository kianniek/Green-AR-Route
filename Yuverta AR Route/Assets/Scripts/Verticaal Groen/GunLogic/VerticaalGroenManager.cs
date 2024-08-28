using ShellanderGames.WeaponWheel;
using UnityEngine;

public class VerticaalGroenManager : Singleton<VerticaalGroenManager>
{
    [SerializeField] private GameObject gunController;
    
    private int currentWeaponIndex = 0;
    private GunController currentWeapon;

    // Start is called before the first frame update
    void Start()
    {
        currentWeapon = gunController.GetComponent<GunController>();
    }

    public void ChangeCurrentWeapon(int index)
    {
        currentWeaponIndex = index;
        currentWeapon.ChangeWeapon(currentWeaponIndex);
    }
}
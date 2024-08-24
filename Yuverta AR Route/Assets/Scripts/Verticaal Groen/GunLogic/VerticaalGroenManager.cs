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
        
        wheel.AddEventCallback(OnEvent);
        ChangeCurrentWeapon(currentWeaponIndex);
    }
    
    private void OnEvent(SgWeaponWheelEvent wheelEvent)
    {
        if (wheelEvent.type == SgWeaponWheelEventType.Select)
        {
            ChangeCurrentWeapon(wheelEvent.slice.sliceIndex);
        }
    }
    
    private void ChangeCurrentWeapon(int index)
    {
        currentWeaponIndex = index;
        currentWeapon.ChangeWeapon(currentWeaponIndex);
    }
}

using ShellanderGames.WeaponWheel;
using UnityEngine;

public class VerticaalGroenManager : BaseManager
{
    public static VerticaalGroenManager Instance;
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
        var gunControllerInstance = gunController;
        currentWeapon = gunControllerInstance.GetComponent<BaseGunScript>();
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

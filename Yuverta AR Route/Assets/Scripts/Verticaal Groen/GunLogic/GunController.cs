using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GunController : MonoBehaviour
{
    [Header("Weapon Variables")] [SerializeField]
    private List<Weapon> weapons;

    [Header("Bullet Variables")] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;

    [Header("Ammunition Variables")] private Ammo currentAmmo;

    private Weapon currentWeapon;
    private WeaponType weaponType;
    private bool firing;

    [Header("Catapult Variables")] private float chargeRate;
    private float maxCharge;
    private float launchForce;
    private float elapsedTime = 0;

    [Header("Miscellaneous")] private Camera mainCamera;

    private GameObject weaponInstance;

    public RectTransform weaponWheel;

    [SerializeField] private UnityEvent onSingleFireShot = new();
    private bool invokeOneShotEvent = false;
    [SerializeField] private UnityEvent onSlingshotCharging = new();
    [SerializeField] private UnityEvent onSlingshotShot = new();

    [Tooltip("This variable is set to <b>True</b> in a UI element to prevent a fire event when touching UI")]
    private bool isSwitchingWeaponThisPress = false;

    public bool IsSwitchingWeaponThisPress
    {
        get => isSwitchingWeaponThisPress;
        set => isSwitchingWeaponThisPress = value;
    }

    protected virtual void Start()
    {
        mainCamera = Camera.main;

        ChangeWeapon(0);
    }

    void Update()
    {
        // Decrease the fire rate timer
        if (currentWeapon.fireRateCooldownTimer > 0)
        {
            currentWeapon.fireRateCooldownTimer -= Time.deltaTime;
        }
        else if (currentWeapon.fireRateCooldownTimer < 0)
        {
            currentWeapon.fireRateCooldownTimer = 0;
            firing = false;
        }

        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);


            if (touch.phase == TouchPhase.Began)
            {
                LoadingModelCatapult(true);
                return;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                LoadingModelCatapult(false);
                Shoot(elapsedTime);

                elapsedTime = 0;
                invokeOneShotEvent = false;
            }
            else
            {
                LoadingModelCatapult(true);
                elapsedTime += Time.deltaTime;
            }
        }
    }

    public void ChangeWeapon(int weaponIndex)
    {
        Debug.Log("Changing weapon to " + weaponIndex);
        if (currentWeapon == weapons[weaponIndex])
        {
            Debug.Log("Already using this weapon");
            return;
        }

        isSwitchingWeaponThisPress = false;

        if (gameObject.activeInHierarchy) // We cant run a coroutine if the object is not active
        {
            StartCoroutine(RotateWeaponWheel());
        }

        // Destroy the old gun
        if (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }

        // Spawn the new gun
        currentWeapon = weapons[weaponIndex];
        weaponInstance = Instantiate(currentWeapon.weaponModel, transform);

        // Set ammo and weapon variables
        currentAmmo = currentWeapon.weaponAmmo;
        bulletPrefab = currentAmmo.bulletPrefab;

        weaponType = currentWeapon.weaponType;

        // Find the bullet spawn point
        foreach (Transform child in weaponInstance.transform)
        {
            if (child.name == "BulletSpawnPoint")
            {
                bulletSpawnPoint = child;
                break;
            }
        }

        // Set catapult variables if applicable
        if (weaponType == WeaponType.Slingshot)
        {
            maxCharge = currentWeapon.maxCharge;
            chargeRate = currentWeapon.chargeRate;
            launchForce = currentWeapon.launchForce;
            bulletSpawnPoint.transform.Rotate(new Vector3(-60, 0, 0));
        }

        firing = false;
    }

    public IEnumerator RotateWeaponWheel()
    {
        if (!weaponWheel)
            yield return null;

        float turnAmount = 180f;
        float amountTurned = 0f;
        float rotationSpeed = 180f; // Degrees per second
        Quaternion initialRotation = weaponWheel.rotation;
        Quaternion targetRotation = Quaternion.Euler(weaponWheel.rotation.eulerAngles + new Vector3(0, 0, turnAmount));

        while (amountTurned < turnAmount)
        {
            float turnAddition = rotationSpeed * Time.deltaTime;
            amountTurned += turnAddition;

            // Interpolating the rotation
            weaponWheel.rotation = Quaternion.Slerp(initialRotation, targetRotation, amountTurned / turnAmount);

            yield return null;
        }

        // Ensure final rotation is exactly at the target rotation
        weaponWheel.rotation = targetRotation;

        yield return null;
    }


    public void Shoot(float elapsedTime)
    {
        if (firing || currentWeapon.fireRateCooldownTimer > 0 || isSwitchingWeaponThisPress)
        {
            return;
        }

        StartCoroutine(Shooting(elapsedTime));
    }

    private IEnumerator Shooting(float elapsedTime)
    {
        switch (weaponType)
        {
            case WeaponType.Pistol:
                yield return StartCoroutine(SingleShot());
                break;
            case WeaponType.Slingshot:
                yield return StartCoroutine(CatapultShot(elapsedTime));
                break;
        }
    }

    private IEnumerator SingleShot()
    {
        onSingleFireShot.Invoke();
        ShootBullet();
        yield return new WaitForSeconds(currentWeapon.fireRate);
    }

    private IEnumerator CatapultShot(float elapsedTime)
    {
        onSlingshotShot.Invoke();
        // Calculate the current charge based on the elapsed time
        float currentCharge = Mathf.Clamp(elapsedTime * chargeRate, 0, maxCharge);

        // Launch the projectile with the calculated charge
        LaunchProjectile(currentCharge);

        yield return null;
    }

    private void LoadingModelCatapult(bool isActive)
    {
        if (currentWeapon.weaponType != WeaponType.Slingshot)
            return;

        if (!invokeOneShotEvent)
        {
            onSlingshotCharging.Invoke();
            invokeOneShotEvent = true;
        }

        var activeState = weaponInstance.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
        var unactiveState = weaponInstance.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>();

        unactiveState.enabled = !isActive;
        activeState.enabled = isActive;
    }

    private void LaunchProjectile(float currentCharge)
    {
        GameObject projectileInstance = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        CatapultProjectile catapultProjectile = projectileInstance.GetComponent<CatapultProjectile>();
        float projectileSpeed = currentAmmo.projectileSpeed + (currentCharge * launchForce);

        catapultProjectile.Launch(bulletSpawnPoint.forward * projectileSpeed);
    }

    public void ShootBullet()
    {
        firing = true;

        currentWeapon.fireRateCooldownTimer = currentWeapon.fireRate;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        BulletLogic bL = bullet.GetComponent<BulletLogic>();
        bL.ammo = currentAmmo;
        bL.collisionPainter.paintColorIndex = currentWeapon.paintColorIndex;
    }
}
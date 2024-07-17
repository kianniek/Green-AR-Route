using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GunController : MonoBehaviour
{
    [Header("Bullet Variables")] private GameObject bulletPrefab;
    private Transform bulletSpawnPoint;

    [Header("Ammunition Variables")] private int magazineSize;
    private int currentAmmunition;
    private bool isReloading = false;
    private Ammo currentAmmo;

    [Header("Weapon Variables")] [SerializeField]
    private List<Weapon> weapons;

    private float fireRate;
    private float fireRateCooldown;
    private Weapon currentWeapon;
    [SerializeField] private Vector3 weaponOffset = new Vector3(1, -0.8f, 1f);
    private WeaponType weaponType;
    private int burstCount; // Visible only if weaponType is Burst
    private float burstRate;
    private bool firing;

    [Header("Catapult Variables")] private float chargeRate;
    private float maxCharge;
    private float launchForce;

    [Header("Miscellaneous")] private Camera mainCamera;
    private bool isTouchingUI;

    protected virtual void Start()
    {
        currentAmmunition = magazineSize;
        fireRateCooldown = 0;
        mainCamera = Camera.main;
        transform.parent = mainCamera.transform;

        ChangeWeapon(0);
    }

    void Update()
    {
        // Decrease the fire rate timer
        if (fireRateCooldown > 0)
        {
            fireRateCooldown -= Time.deltaTime;
        }

        //if Touch started was over UI return
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (IsTouchOverUI(Input.GetTouch(0).fingerId))
            {
                return;
            }
        }

        // Check for touch input
        if (Input.touchCount > 0)
        {
            isTouchingUI = IsTouchOverUI(Input.GetTouch(0).fingerId);
            
            if (!isTouchingUI && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                Debug.Log("Touch detected");
                Shoot();
            }
        }
    }

    private bool IsTouchOverUI(int fingerId)
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void ChangeWeapon(int weaponIndex)
    {
        // Destroy the old gun
        if (transform.childCount > 0) Destroy(transform.GetChild(0).gameObject);

        // Spawn the new gun
        currentWeapon = weapons[weaponIndex];
        var weaponInstance = Instantiate(currentWeapon.prefab, transform);

        // Set ammo and weapon variables
        currentAmmo = currentWeapon.ammo;
        bulletPrefab = currentAmmo.bulletPrefab;

        // Start with a full magazine
        currentAmmunition = magazineSize = currentWeapon.magazineSize;

        fireRate = 60f / currentWeapon.roundsPerMinute;
        weaponType = currentWeapon.weaponType;

        if (weaponType == WeaponType.Burst)
        {
            burstCount = currentWeapon.burstCount;
            burstRate = fireRate / burstCount;
        }

        // Find the bullet spawn point
        for (int i = 0; i < weaponInstance.transform.childCount; i++)
        {
            var child = weaponInstance.transform.GetChild(i).GetChild(0);
            if (child.name == "BulletSpawnPoint")
            {
                bulletSpawnPoint = child;
                break;
            }
        }

        // Set catapult variables if applicable
        if (weaponType == WeaponType.Catapult)
        {
            maxCharge = currentWeapon.maxCharge;
            chargeRate = currentWeapon.chargeRate;
            launchForce = currentWeapon.launchForce;
            bulletSpawnPoint.transform.Rotate(new Vector3(-60, 0, 0));
        }

        firing = false;
    }

    void LateUpdate()
    {
        // Keep the position of the object in front of the camera
        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.rotation * weaponOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, 0.1f);

        // Follow camera rotation
        transform.rotation = mainCamera.transform.rotation;
    }

    public void Shoot()
    {
        if (firing)
            return;

        StartCoroutine(Shooting());
    }

    private IEnumerator Shooting()
    {
        firing = true;
        switch (weaponType)
        {
            case WeaponType.Single:
                yield return StartCoroutine(SingleShot());
                break;
            case WeaponType.Burst:
                yield return StartCoroutine(BurstShot());
                break;
            case WeaponType.Automatic:
                yield return StartCoroutine(AutomaticShot());
                break;
            case WeaponType.Catapult:
                yield return StartCoroutine(CatapultShot());
                break;
        }
    }

    private IEnumerator SingleShot()
    {
        ShootBullet();
        yield return new WaitForSeconds(fireRate);
        firing = false;

        yield return null;
    }

    private IEnumerator BurstShot()
    {
        var currentBurstCount = burstCount;
        while (currentBurstCount > 0)
        {
            ShootBullet();
            yield return new WaitForSeconds(burstRate);
            currentBurstCount--;
        }

        yield return new WaitForSeconds(fireRate);
        firing = false;
    }

    private IEnumerator AutomaticShot()
    {
        while (true)
        {
            ShootBullet();
            yield return new WaitForSeconds(fireRate);
            if (Input.touchCount == 0 || Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                break;
            }
        }

        firing = false;
    }

    private IEnumerator CatapultShot()
    {
        var startTime = Time.time;
        while (Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Ended)
        {
            yield return new WaitForSeconds(0.1f);
        }

        var currentCharge = Mathf.Clamp((Time.time - startTime) * chargeRate, 0, maxCharge);
        firing = false;
        StartCoroutine(LaunchProjectile(currentCharge));
    }

    private IEnumerator LaunchProjectile(float currentCharge)
    {
        var projectileInstance = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity)
            .GetComponent<CatapultProjectile>();
        currentAmmo.projectileSpeed = currentCharge * launchForce;
        projectileInstance.ammo = currentAmmo;
        projectileInstance.Launch(bulletSpawnPoint.forward * (currentCharge * launchForce));
        yield return null;
    }

    public void ShootBullet()
    {
        if (isReloading)
            return;

        if (currentAmmunition > 0)
        {
            var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.GetComponent<BulletLogic>().ammo = currentAmmo;
            currentAmmunition--;
        }
        else if (currentAmmunition <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        currentAmmunition = magazineSize;
        isReloading = false;

        yield return null;
    }
}
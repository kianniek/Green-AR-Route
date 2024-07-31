using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GunController : MonoBehaviour
{
    [Header("Weapon Variables")]
    [SerializeField]
    private List<Weapon> weapons;
    
    [Header("Bullet Variables")]
    private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;

    [Header("Ammunition Variables")]
    private Ammo currentAmmo;

    private Weapon currentWeapon;
    private WeaponType weaponType;
    private bool firing;

    [Header("Catapult Variables")]
    private float chargeRate;
    private float maxCharge;
    private float launchForce;

    [Header("Miscellaneous")]
    private Camera mainCamera;

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
        }else if (currentWeapon.fireRateCooldownTimer < 0)
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
            if (touch.phase == TouchPhase.Began && IsTouchOverUI(touch.fingerId))
            {
                return;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                Shoot();
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Shoot();
        }
#endif
    }

    private bool IsTouchOverUI(int fingerId)
    {
        return EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    public void ChangeWeapon(int weaponIndex)
    {
        // Destroy the old gun
        if (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }

        // Spawn the new gun
        currentWeapon = weapons[weaponIndex];
        var weaponInstance = Instantiate(currentWeapon.weaponModel, transform);

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

    public void Shoot()
    {
        if (firing || currentWeapon.fireRateCooldownTimer > 0)
        {
            return;
        }

        StartCoroutine(Shooting());
    }

    private IEnumerator Shooting()
    {
        switch (weaponType)
        {
            case WeaponType.Pistol:
                yield return StartCoroutine(SingleShot());
                break;
            case WeaponType.Slingshot:
                yield return StartCoroutine(CatapultShot());
                break;
        }
    }

    private IEnumerator SingleShot()
    {
        ShootBullet();
        yield return new WaitForSeconds(currentWeapon.fireRate);
    }

    private IEnumerator CatapultShot()
    {
        float startTime = Time.time;

        while (Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Ended)
        {
            yield return null;
        }

#if UNITY_EDITOR
        while (Input.GetMouseButton(0))
        {
            yield return null;
        }
#endif

        float currentCharge = Mathf.Clamp((Time.time - startTime) * chargeRate, 0, maxCharge);
        yield return LaunchProjectile(currentCharge);
    }

    private IEnumerator LaunchProjectile(float currentCharge)
    {
        var projectileInstance = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity)
            .GetComponent<CatapultProjectile>();
        currentAmmo.projectileSpeed = currentCharge * launchForce;
        projectileInstance.ammo = currentAmmo;
        projectileInstance.Launch(bulletSpawnPoint.forward * currentCharge * launchForce);
        yield return null;
    }

    public void ShootBullet()
    {
        firing = true;
        
        currentWeapon.fireRateCooldownTimer = currentWeapon.fireRate;

        var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        var bL = bullet.GetComponent<BulletLogic>();
        bL.ammo = currentAmmo;
        bL.collisionPainter.paintColorIndex = currentWeapon.paintColorIndex;
    }
}

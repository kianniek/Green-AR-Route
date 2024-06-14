using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseGunScript : MonoBehaviour
{
    [Header("Animator")]
    public RecoilAnimation recoilAnimation;
    public ReloadAnimation reloadAnimation;
    [Space(10)]
    
    [Header("Bullet variables")]
    private GameObject bulletPrefab;
    private Transform bulletSpawnPoint;
    [Space(10)]
    
    [Header("Ammunition variables")]
    private int magazineSize;
    private int currentAmmunition;
    private bool isReloading = false;
    private Ammo currentAmmo;
    
    [Header("Weapon variables")]
    [SerializeField] private List<Weapon> weapons;
    private float fireRate;
    private float fireRateCooldown;
    private Weapon currentWeapon;
    private readonly Vector3 weaponOffset = new Vector3(1, -0.8f, 1f);
    private WeaponType weaponType;
    private int burstCount; // This field will only be visible if weaponType is Burst
    private float burstRate;
    private bool firing;
    
    [Header("Catapult variables")]
    private float chargeRate;
    private float maxCharge;
    private float launchForce;
    
    [Header("Normal variables")]
    private Camera mainCamera;
    
    protected virtual void Start()
    {
        currentAmmunition = magazineSize;
        fireRateCooldown = 0;
        mainCamera = Camera.main;
        
        transform.parent = mainCamera.transform;
        
        
    }
    
    void Update()
    {
        // Decrease the fire rate timer
        if (fireRateCooldown > 0)
        {
            fireRateCooldown -= Time.deltaTime;
        }
    }

    public void ChangeWeapon(int weaponIndex)
    {
        //Destroying old gun
        if (transform.childCount > 0) Destroy(transform.GetChild(0).gameObject);
        //Spawning the new gun
        currentWeapon = weapons[weaponIndex];
        var weaponInstance = Instantiate(currentWeapon.prefab, transform);
        weaponInstance.transform.localPosition = weaponOffset;
        
        //Setting and activating the animations validation
        if (!recoilAnimation || !reloadAnimation)
        {
            Debug.LogError("Recoil or reload animation not set");
        }
        
        
        //Setting the ammo and weapon variables
        currentAmmo = currentWeapon.ammo;
        bulletPrefab = currentAmmo.bulletPrefab;
        FindObjectOfType<SplatMakerExample>().splatScale = currentAmmo.splatScale;
        
        //Start with a full magazine
        currentAmmunition = magazineSize = currentWeapon.magazineSize;
        
        fireRate = 60f / currentWeapon.roundsPerMinute;
        weaponType = currentWeapon.weaponType;
        
        if (currentWeapon.weaponType == WeaponType.Burst)
        {
            burstCount = currentWeapon.burstCount;
            burstRate = fireRate / burstCount;
        }
        
        for (int i = 0; i < weaponInstance.transform.childCount; i++)
        {
            if (weaponInstance.transform.GetChild(i).name == "BulletSpawnPoint")
            {
                bulletSpawnPoint = weaponInstance.transform.GetChild(i);
                break;
            }
        }
        
        if (currentWeapon.weaponType == WeaponType.Catapult)
        {
            maxCharge = currentWeapon.maxCharge;
            chargeRate = currentWeapon.chargeRate;
            launchForce = currentWeapon.launchForce;
            bulletSpawnPoint.transform.Rotate(new Vector3(-60, 0,0 ));
        }
        firing = false;
    }
    
    void LateUpdate()
    {
        // Keep the position of the object in front of the camera even when the camera rotates
        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.rotation * weaponOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, 0.1f);
        
        // Follow camera rotation
        transform.rotation = mainCamera.transform.rotation;
    }

    public virtual void Shoot()
    {
        if (firing) return;
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        if (Application.isEditor)
        {
            StartCoroutine(Shooting(() => Input.GetMouseButton(0)));
        }
        else if (Input.touchCount > 0)
        {
            StartCoroutine(Shooting(() => Input.GetTouch(0).phase != TouchPhase.Ended));
        }
    }
    
    private IEnumerator Shooting(Func<bool> isPressed)
    {
        firing = true;
        switch (weaponType)
        {
            case WeaponType.Single:
                yield return StartCoroutine(SingleShot(isPressed));
                break;
            case WeaponType.Burst:
                yield return StartCoroutine(BurstShot(isPressed));
                break;
            case WeaponType.Automatic:
                yield return StartCoroutine(AutomaticShot(isPressed));
                break;
            case WeaponType.Catapult:
                yield return StartCoroutine(CatapultShot(isPressed));
                break;
        }
    }

    private IEnumerator SingleShot(Func<bool> isPressed)
    {
        while (isPressed())
        {
            ShootBullet();
            yield return new WaitForSeconds(fireRate);
        }
        firing = false;
    }

    private IEnumerator BurstShot(Func<bool> isPressed)
    {
        var currentBurstCount = burstCount;
        while (isPressed())
        {
            if (currentBurstCount > 0)
            {
                ShootBullet();
                yield return new WaitForSeconds(burstRate);
                currentBurstCount--;
                continue;
            }
            
            currentBurstCount = burstCount;
            yield return new WaitForSeconds(fireRate);
        }
        firing = false;
    }

    private IEnumerator AutomaticShot(Func<bool> isPressed)
    {
        while (isPressed())
        {
            ShootBullet();
            yield return new WaitForSeconds(fireRate);
        }
        firing = false;
    }

    private IEnumerator CatapultShot(Func<bool> isPressed)
    {
        //var currentCharge = 0f;
        var time = Time.time;
        while (isPressed())
        {
            yield return new WaitForSeconds(0.1f);
        }
        var currentCharge = Mathf.Clamp((Time.time - time) * chargeRate, 0, maxCharge);
        StartCoroutine(LaunchProjectile(currentCharge));
        firing = false;
    }
    
    IEnumerator LaunchProjectile(float currentCharge)
    {
        CatapultProjectile projectileInstance = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity).GetComponent<CatapultProjectile>();
        currentAmmo.projectileSpeed = currentCharge * launchForce;
        projectileInstance.ammo = currentAmmo;
        projectileInstance.Launch(bulletSpawnPoint.forward * (currentCharge * launchForce));
        
        recoilAnimation.TriggerRecoil();
        Debug.Log("Launched");

        //The animation has to be triggerd to get the length of the animation which takes time
        yield return new WaitForSeconds(recoilAnimation.recoilDuration);
        
        // Get the length of the ReloadAnimation animation
        reloadAnimation.TriggerReload();

        yield return new WaitForSeconds(reloadAnimation.reloadDuration);

        StartCoroutine(Reload());
    }

    private IEnumerator CheckFire(float totalTime, float interval, Func<bool> isPressed)
    {
        yield return new WaitForSeconds(interval);
        totalTime -= interval;
        if (!isPressed() || totalTime <= 0) yield break; 
        yield return StartCoroutine(CheckFire(totalTime, interval, isPressed));
    }

    public virtual void ShootBullet()
    {
        if (isReloading) return;
        if (currentAmmunition > 0 /*&& fireRateCooldown <= 0*/)
        {
            var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.GetComponent<BulletLogic>().ammo = currentAmmo;
            currentAmmunition--;
            
            recoilAnimation.TriggerRecoil();
            
            /*// Reset the fire rate timer
            fireRateCooldown = fireRate;*/
        }
        else if (currentAmmunition <= 0)
        {
            reloadAnimation.TriggerReload();
        }
    }
    
    
    
    private IEnumerator Reload()
    {
        isReloading = true;
        reloadAnimation.TriggerReload();

        //The animation has to be triggerd to get the length of the animation which takes time
        yield return new WaitForSeconds(0.1f);
        
        // Get the length of the ReloadAnimation animation

        yield return new WaitForSeconds(reloadAnimation.reloadDuration);

        // After the reload animation, refill the ammunition
        // Replace 10 with the actual ammunition count after reloading
        currentAmmunition = magazineSize;
        isReloading = false;
    }
}

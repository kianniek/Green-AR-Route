using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

//[CreateAssetMenu(fileName = "GunScript", menuName = "ScriptableObjects/GunScript", order = 1)]
public class BaseGunScript : MonoBehaviour
{
    [Header("Animator")]
    private Animator animator;
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
    }
    
    void Update()
    {
        // Decrease the fire rate timer
        if (fireRateCooldown > 0)
        {
            fireRateCooldown -= Time.deltaTime;
        }
    }

    public void ChangeWeapon(Weapon newWeapon)
    {
        //Destroying old gun
        if (transform.childCount > 0) Destroy(transform.GetChild(0).gameObject);
        //Spawning the new gun
        currentWeapon = newWeapon;
        var weaponInstance = Instantiate(currentWeapon.prefab, transform);
        weaponInstance.transform.localPosition = weaponOffset;
        
        //Setting and activating the animator
        animator = weaponInstance.GetComponent<Animator>();
        animator.SetTrigger("Equip");
        
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
        
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

        //The animation has to be triggerd to get the length of the animation which takes time
        yield return new WaitForSeconds(0.1f);
        
        // Get the length of the Reload animation
        float shootAnimationLength = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(shootAnimationLength);

        
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
            if (animator != null)
            {
                animator.SetTrigger("Shoot");
            }
            
            /*// Reset the fire rate timer
            fireRateCooldown = fireRate;*/
        }
        else if (currentAmmunition <= 0)
        {
            if (animator != null)
            {
                StartCoroutine(Reload());
            }
        }
    }
    
    
    
    private IEnumerator Reload()
    {
        isReloading = true;
        animator.SetTrigger("Reload");

        //The animation has to be triggerd to get the length of the animation which takes time
        yield return new WaitForSeconds(0.1f);
        
        // Get the length of the Reload animation
        float reloadAnimationLength = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(reloadAnimationLength);

        // After the reload animation, refill the ammunition
        // Replace 10 with the actual ammunition count after reloading
        currentAmmunition = magazineSize;
        isReloading = false;
    }
}
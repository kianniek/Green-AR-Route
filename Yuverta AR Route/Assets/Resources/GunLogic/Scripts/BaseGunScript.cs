using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

//[CreateAssetMenu(fileName = "GunScript", menuName = "ScriptableObjects/GunScript", order = 1)]
public class BaseGunScript : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] protected Animator animator;
    [Space(10)]
    
    [Header("Bullet variables")]
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform bulletSpawnPoint;
    [Space(10)]
    
    [Header("Ammunition variables")]
    [SerializeField] protected int magazineSize;
    [Min(0.1f)] [SerializeField] 
    protected float fireRate;
    protected float fireRateCooldown;
    private int currentAmmunition;
    private bool isReloading = false;

    private Ammo currentAmmo;
    private Weapon currentWeapon;
    private readonly Vector3 weaponOffset = new Vector3(1, -1f, 1f);
    private WeaponType weaponType;
    private int burstCount; // This field will only be visible if weaponType is Burst
    private float burstRate;
    private bool firing;
    
    protected virtual void Start()
    {
        currentAmmunition = magazineSize;
        fireRateCooldown = 0;
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
        if (transform.childCount > 0) Destroy(transform.GetChild(0).gameObject);
        currentWeapon = newWeapon;
        var weaponInstance = Instantiate(currentWeapon.prefab, transform);
        weaponInstance.transform.localPosition = weaponOffset;
        animator = weaponInstance.GetComponent<Animator>();
        animator.SetTrigger("Equip");
        currentAmmo = currentWeapon.ammo;
        currentAmmunition = magazineSize = currentWeapon.magazineSize;
        fireRate = currentWeapon.fireRate;
        weaponType = currentWeapon.weaponType;
        if (currentWeapon.weaponType == WeaponType.Burst)
        {
            burstCount = currentWeapon.burstCount;
            burstRate = currentWeapon.burstRate;
        }
        for (int i = 0; i < weaponInstance.transform.childCount; i++)
        {
            if (weaponInstance.transform.GetChild(i).name == "BulletSpawnPoint")
            {
                bulletSpawnPoint = weaponInstance.transform.GetChild(i);
                break;
            }
        }

        firing = false;
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
        Debug.Log(burstCount);
        var currentBurstCount = burstCount;
        while (isPressed())
        {
            if (currentBurstCount > 0)
            {
                Debug.Log(currentBurstCount);
                ShootBullet();
                currentBurstCount--;
                yield return new WaitForSeconds(burstRate);
            }
            else
            {
                Debug.Log("Cooldown");
                currentBurstCount = burstCount;
                yield return new WaitForSeconds(fireRate);
            }
            /*for (int i = 0; i < burstCount; i++)
            {
                ShootBullet();
                if (!isPressed()) break;
                yield return new WaitForSeconds(burstRate);
            }
            yield return new WaitForSeconds(fireRate);*/
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

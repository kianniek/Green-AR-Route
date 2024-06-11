using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        for (int i = 0; i < weaponInstance.transform.childCount; i++)
        {
            if (weaponInstance.transform.GetChild(i).name == "BulletSpawnPoint")
            {
                bulletSpawnPoint = weaponInstance.transform.GetChild(i);
            }
        }
    }

    public virtual void Shoot()
    {
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
        while (isPressed())
        {
            ShootBullet();
            yield return new WaitForSeconds(fireRate);
        }
    }

    public virtual void ShootBullet()
    {
        if (isReloading) return;
        if (currentAmmunition > 0 && fireRateCooldown <= 0)
        {
            var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.GetComponent<BulletLogic>().ammo = currentAmmo;
            currentAmmunition--;
            if (animator != null)
            {
                animator.SetTrigger("Shoot");
            }
            
            // Reset the fire rate timer
            fireRateCooldown = fireRate;
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

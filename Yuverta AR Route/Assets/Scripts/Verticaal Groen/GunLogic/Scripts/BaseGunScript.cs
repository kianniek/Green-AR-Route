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
    private int currentAmmunition;
    private bool isReloading = false;
    private Ammo currentAmmo;
    
    [Header("Weapon variables")]
    [Min(0.1f)] [SerializeField] 
    protected float fireRate;
    protected float fireRateCooldown;
    private Weapon currentWeapon;
    private readonly Vector3 weaponOffset = new Vector3(1, -0.8f, 1f);
    private WeaponType weaponType;
    private int burstCount; // This field will only be visible if weaponType is Burst
    private float burstRate;
    private bool firing;
    
    [Header("Catapult variables")]
    [Tooltip("The mask containing all the objects that the catapultProjectile can collide with.")]
    [SerializeField] private LayerMask collisionMask;
    private LineRenderer trajectoryLine;
    private readonly int trajectoryResolution = 10;
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
        //Start with a full magazine
        currentAmmunition = magazineSize = currentWeapon.magazineSize;
        
        fireRate = 60f / currentWeapon.roundsPerMinute;
        weaponType = currentWeapon.weaponType;
        
        if (currentWeapon.weaponType == WeaponType.Burst)
        {
            burstCount = currentWeapon.burstCount;
            burstRate = fireRate / burstCount;
        }
        
        if (currentWeapon.weaponType == WeaponType.Catapult)
        {
            trajectoryLine = GetComponent<LineRenderer>();
            trajectoryLine.positionCount = trajectoryResolution;
            maxCharge = currentWeapon.maxCharge;
            chargeRate = currentWeapon.chargeRate;
            launchForce = currentWeapon.launchForce;
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
        var currentCharge = 0f;
        while (isPressed())
        {
            // Charge up the catapult
            currentCharge += chargeRate;
            currentCharge = Mathf.Clamp(currentCharge, 0, maxCharge);

            // Calculate and display the trajectory
            RenderTrajectory(currentCharge);
            Debug.Log(currentCharge);
            yield return StartCoroutine(CheckFire(1f, 0.1f, isPressed));
        }
        StartCoroutine(LaunchProjectile(currentCharge));
        firing = false;
    }
    
    IEnumerator LaunchProjectile(float currentCharge)
    {
        CatapultProjectile projectileInstance = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity).GetComponent<CatapultProjectile>();
        projectileInstance.Launch(bulletSpawnPoint.forward * (currentCharge * launchForce));
        currentAmmo.projectileSpeed = currentCharge * launchForce;
        projectileInstance.ammo = currentAmmo;
        
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
    
    void RenderTrajectory(float currentCharge)
    {
        Vector3[] trajectoryPoints = CalculateTrajectoryPoints(bulletSpawnPoint.position, bulletSpawnPoint.forward * (launchForce * currentCharge));
        trajectoryLine.positionCount = trajectoryPoints.Length;
        trajectoryLine.SetPositions(trajectoryPoints);
    }

    Vector3[] CalculateTrajectoryPoints(Vector3 startPoint, Vector3 initialVelocity)
    {
        var maxPoints = 100;
        var timeBetweenPoints = 0.1f;
        Vector3[] points = new Vector3[trajectoryResolution];
        Vector3 currentPosition = startPoint;
        Vector3 velocity = initialVelocity;

        points[0] = currentPosition;

        for (int i = 1; i < trajectoryResolution; i++)
        {
            // Calculate the next position
            Vector3 nextPosition = currentPosition + velocity * timeBetweenPoints;

            // Check for collision using Raycast
            RaycastHit hit;
            if (Physics.Raycast(currentPosition, velocity, out hit, (nextPosition - currentPosition).magnitude, collisionMask))
            {
                // If we hit something, set the final point and break
                nextPosition = hit.point;
                points[i] = nextPosition;
                break;
            }

            // Check for coordinate difference of 50 units
            if (Vector3.Distance(startPoint, nextPosition) > 50f)
            {
                points[i] = nextPosition;
                break;
            }

            points[i] = nextPosition;
            currentPosition = nextPosition;
            velocity += Physics.gravity * timeBetweenPoints;
        }

        // Trim the points array to the actual number of calculated points
        int trimmedPointsCount = Array.FindIndex(points, p => p == Vector3.zero);
        if (trimmedPointsCount > 0)
        {
            Array.Resize(ref points, trimmedPointsCount);
        }

        return points;
    }

    /*void CalculateTrajectory(float currentCharge)
    {
        Vector3 velocity = bulletSpawnPoint.forward * (currentCharge * launchForce);
        float flightTime = (2 * velocity.y) / currentAmmo.bulletDrop;

        trajectoryLine.positionCount = trajectoryResolution + 1;
        for (int i = 0; i <= trajectoryResolution; i++)
        {
            float t = i / (float)trajectoryResolution;
            // Reverse the order in which the positions are set
            trajectoryLine.SetPosition(trajectoryResolution  - i, CalculatePosition(t, flightTime, velocity));
        }
    }

    Vector3 CalculatePosition2(float t, float flightTime, Vector3 velocity)
    {
        float x = velocity.x * (t * flightTime);
    }

    Vector3 CalculatePosition(float t, float flightTime, Vector3 velocity)
    {
        float x = velocity.x * (t * flightTime);
        float y = velocity.y * (t * flightTime) - 1f * currentAmmo.bulletDrop * Mathf.Pow(t * flightTime, 2);
        float z = velocity.z * (t * flightTime);
        return bulletSpawnPoint.position + new Vector3(x, y, z);
    }*/

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

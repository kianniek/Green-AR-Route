using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GunController : MonoBehaviour
{
    [Header("Weapon Variables"), NonReorderable] [SerializeField]
    private List<Weapon> weapons;

    [Header("Bullet Variables")] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;

    [Header("Ammunition Variables")] private Ammo currentAmmo;

    private Weapon currentWeapon;
    private WeaponType weaponType;
    private bool firing;

    [Header("Catapult Variables")] private float shootForce;

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

    [SerializeField] private int startingWeaponIndex = 1;

    [SerializeField] bool useTouchInput = true;

    [SerializeField] private Button fireButton;

    public bool IsSwitchingWeaponThisPress
    {
        get => isSwitchingWeaponThisPress;
        set => isSwitchingWeaponThisPress = value;
    }

    protected virtual void Start()
    {
        mainCamera = Camera.main;

        ChangeWeapon(startingWeaponIndex);
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

        if (useTouchInput)
        {
            HandleInput();
        }
        
        UpdateFireButtonFill();
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
                isSwitchingWeaponThisPress = false;
            }
            else
            {
                LoadingModelCatapult(true);
                elapsedTime += Time.deltaTime;
            }
        }
    }

    public void OnFireButtonPressed(PointerEventData eventData)
    {
        if (useTouchInput)
            return;

        // Start the firing process
        LoadingModelCatapult(true);
        elapsedTime = 0; // Reset elapsed time
        invokeOneShotEvent = false; // Reset event flag
    }

    public void OnFireButtonReleased(PointerEventData eventData)
    {
        if (useTouchInput)
            return;

        // Handle the fire button release
        LoadingModelCatapult(false);
        Shoot(elapsedTime); // Trigger shooting with the current elapsed time

        elapsedTime = 0; // Reset elapsed time
        invokeOneShotEvent = false; // Reset event flag
        isSwitchingWeaponThisPress = false; // Reset weapon switch flag
    }

    public void ChangeWeapon(int weaponIndex)
    {
        Debug.Log("Changing weapon to " + weaponIndex);
        if (currentWeapon == weapons[weaponIndex])
        {
            Debug.Log("Already using this weapon");
            return;
        }

        isSwitchingWeaponThisPress = true;

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
            if (child.CompareTag("BulletSpawnpoint"))
            {
                bulletSpawnPoint = child;
                break;
            }
        }

        switch (weaponType)
        {
            // Set catapult variables if applicable
            case WeaponType.Slingshot:
                maxCharge = currentWeapon.maxCharge;
                chargeRate = currentWeapon.chargeRate;
                launchForce = currentWeapon.launchForce;
                break;
            case WeaponType.Pistol:
                shootForce = currentWeapon.shootForce;
                break;
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

        // Start de zoekactie
        FindBulletSpawnPoint(weaponInstance.transform);

// Controleer of bulletSpawnPoint is gevonden
        if (bulletSpawnPoint == null)
        {
            Debug.LogWarning("Geen BulletSpawnpoint gevonden in de hiërarchie.");
            // Eventueel hier een fallback actie ondernemen
        }

        //check in all children of the weaponInstance for the bulletSpawnPoint


        StartCoroutine(Shooting(elapsedTime));
    }

    void FindBulletSpawnPoint(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag("BulletSpawnpoint"))
            {
                bulletSpawnPoint = child;
                return; // Stop zodra het spawnpoint is gevonden
            }

            // Recursief zoeken in de kinderen van het huidige child-object
            FindBulletSpawnPoint(child);

            // Als bulletSpawnPoint gevonden is, stoppen met zoeken
            if (bulletSpawnPoint != null)
                return;
        }
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
        currentWeapon.fireRateCooldownTimer = currentWeapon.fireRate;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        CatapultProjectile projectile = bullet.GetComponent<CatapultProjectile>();
        float projectileSpeed = currentAmmo.projectileSpeed + (currentCharge * launchForce);

        projectile.Launch(bulletSpawnPoint.forward * projectileSpeed);
    }

    public void ShootBullet()
    {
        firing = true;

        currentWeapon.fireRateCooldownTimer = currentWeapon.fireRate;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        BulletLogic projectile = bullet.GetComponent<BulletLogic>();
        float projectileSpeed = currentAmmo.projectileSpeed + shootForce;

        projectile.Launch(bulletSpawnPoint.forward * projectileSpeed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateFireButtonFill()
    {
        if (fireButton)
        {
            if (fireButton.image.type == Image.Type.Filled)
            {
                fireButton.image.fillAmount = Mathf.Lerp(fireButton.image.fillAmount, 1 - (currentWeapon.fireRateCooldownTimer / currentWeapon.fireRate), 0.9f);
            }
        }
    }
}
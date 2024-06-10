using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] protected int fullAmmunition;
    [Min(0.01f)] [SerializeField] 
    protected float fireRateLimit;
    protected float fireRate;
    private int ammunition;
    private bool isReloading = false;
    
    public int Ammunition
    {
        get { return ammunition; }
        set { ammunition = value; }
    }
    
    protected virtual void Start()
    {
        ammunition = fullAmmunition;
        fireRate = 0;
    }
    
    void Update()
    {
        // Decrease the fire rate timer
        if (fireRate > 0)
        {
            fireRate -= Time.deltaTime;
        }
    }
    
    public virtual void Shoot()
    {
        if (isReloading) return;
        if (ammunition > 0 && fireRate <= 0)
        {
            var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            ammunition--;
            if (animator != null)
            {
                animator.SetTrigger("Shoot");
            }
            
            // Reset the fire rate timer
            fireRate = fireRateLimit;
        }
        else if (ammunition <= 0)
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
        ammunition = fullAmmunition;
        isReloading = false;
    }
}

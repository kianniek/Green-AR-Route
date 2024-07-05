using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BulletLogic : MonoBehaviour
{
    public Ammo ammo;
    public UnityEvent onImpact;
    private float spawnTimer;
    public delegate void BulletHitEvent(Vector3 hitPosition);
    public static event BulletHitEvent OnBulletHit;
    
    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        //Get all components
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void Start()
    {
        OnSpawn();
    }

    public void OnSpawn()
    {
        // Set the spawn time to 0
        spawnTimer = 0f;
        
        // Set inpulse force to the bullet
        rb.AddForce(transform.forward * ammo.projectileSpeed, ForceMode.Impulse);
        
    }
    
    // Update is called once per frame
    void Update()
    {
        //add a constant force in the -grafity direction
        rb.AddForce(-Physics.gravity, ForceMode.Force);
        
        rb.AddForce(Vector3.down * ammo.bulletDrop, ForceMode.Force);
        
        spawnTimer += Time.deltaTime;
        if (spawnTimer > 5f)
        {
            // Destroy the bullet after 5 seconds
            Destroy(gameObject);
        }
    }
}

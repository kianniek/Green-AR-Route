using UnityEngine;

public class CatapultProjectile : MonoBehaviour
{
    public Ammo ammo;
    private Vector3 initialVelocity;
    private Vector3 currentVelocity;
    private float startTime;
    private float maxTime = 5f;

    public void Launch(Vector3 velocity)
    {
        initialVelocity = velocity;
        currentVelocity = initialVelocity;
        startTime = Time.time;
    }

    void Update()
    {
        Collide();
        if (Time.time - startTime > maxTime)
        {
            Destroy(gameObject);
            Destroy(this);
            return;
        }

        // Calculate the elapsed time
        float elapsedTime = Time.time - startTime;

        // Update the velocity
        currentVelocity.y = initialVelocity.y + (Physics.gravity.y * elapsedTime);

        // Calculate the new position
        Vector3 newPosition = transform.position + (currentVelocity * Time.deltaTime);

        // Update the position
        transform.position = newPosition;

        // Rotate the projectile to face the direction of travel
        transform.rotation = Quaternion.LookRotation(currentVelocity.normalized);
    }

    private void OnCollisionEnter(Collision other)
    {
        Collide();
    }

    private void Collide()
    {
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hit,
                ammo.projectileSpeed * Time.deltaTime) && !hit.collider.gameObject.CompareTag("Bullet"))
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            FindObjectOfType<SplatMakerExample>().OnHit(hit);
            
            VerticaalGroenManager.Instance.scoreManager.AddScore();
            // Destroy the bullet
            Destroy(gameObject, 1f);
            Destroy(this);
        }
    }
}
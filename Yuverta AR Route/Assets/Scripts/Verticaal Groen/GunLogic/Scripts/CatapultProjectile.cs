using UnityEngine;

public class CatapultProjectile : MonoBehaviour
{
    public Ammo ammo;
    private Vector3 initialVelocity;
    private float startTime;
    private float maxTime = 5f;

    public void Launch(Vector3 velocity)
    {
        initialVelocity = velocity;
        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time - startTime > maxTime)
        {
            Destroy(gameObject);
            Destroy(this);
            return;
        }
        // Calculate the elapsed time
        float elapsedTime = Time.time - startTime;

        // Calculate the new position
        Vector3 newPosition = initialVelocity * elapsedTime;
        newPosition.y -= 0.5f * ammo.bulletDrop * Mathf.Pow(elapsedTime, 2);

        // Update the position
        transform.position += newPosition;

        // Rotate the projectile to face the direction of travel
        transform.rotation = Quaternion.LookRotation(newPosition.normalized);
    }
}

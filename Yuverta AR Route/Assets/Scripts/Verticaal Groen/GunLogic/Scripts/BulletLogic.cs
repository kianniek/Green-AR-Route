using UnityEngine;
using UnityEngine.Events;

public class BulletLogic : MonoBehaviour
{
    public Ammo ammo;
    public UnityEvent onImpact;
    private float spawnTimer;
    public delegate void BulletHitEvent(Vector3 hitPosition);
    public static event BulletHitEvent OnBulletHit;

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hit,
                ammo.projectileSpeed * Time.deltaTime) && !hit.collider.gameObject.CompareTag("Bullet"))
        {
            // Invoke the onImpact event when the bullet hits something
            onImpact.Invoke();
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            FindObjectOfType<SplatMakerExample>().OnHit(hit);
            OnBulletHit?.Invoke(hit.point);
            // Destroy the bullet
            Destroy(gameObject, 1f);
            Destroy(this);
        }
        else if (spawnTimer > 5f)
        {
            // Destroy the bullet after 5 seconds
            Destroy(gameObject);
        }
        else
        {
            // Move the bullet forward
            transform.Translate((Vector3.forward * ammo.projectileSpeed - new Vector3(0, ammo.bulletDrop * spawnTimer, 0)) * Time.deltaTime);
        }
    }
}

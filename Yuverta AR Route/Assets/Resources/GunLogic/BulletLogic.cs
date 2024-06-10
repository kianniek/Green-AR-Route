using UnityEngine;
using UnityEngine.Events;

public class BulletLogic : MonoBehaviour
{
    public Ammo ammo;
    public UnityEvent onImpact;

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hit, ammo.projectileSpeed * Time.deltaTime))
        {
            // Invoke the onImpact event when the bullet hits something
            onImpact.Invoke();
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            // Destroy the bullet
            Destroy(gameObject, 1f);
            Destroy(this);
        }
        else
        {
            // Move the bullet forward
            transform.Translate(Vector3.forward * ammo.projectileSpeed * Time.deltaTime);
        }
    }
}

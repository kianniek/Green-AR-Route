using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CatapultProjectile : MonoBehaviour
{
    public Ammo ammo;
    private Rigidbody rb;

    private const float SEC_UNTIL_SHRIKING = 2f;

    public void Launch(Vector3 velocity)
    {
        rb = GetComponent<Rigidbody>();

        rb.AddForce(velocity, ForceMode.Impulse);
    }

    private IEnumerator ShrinkObject()
    {
        yield return new WaitForSeconds(SEC_UNTIL_SHRIKING);
        while(gameObject.transform.localScale.magnitude > 0.01f)
        {
            gameObject.transform.localScale *= 0.9f;
            yield return null;
        }

        //If the object is shrunken all the way we can delete it
        Destroy(gameObject);

        yield break;
    }

    void Update()
    {
        // Rotate the projectile to face the direction of travel
        transform.rotation = Quaternion.LookRotation(rb.velocity.normalized);
    }

    private void OnCollisionEnter(Collision other)
    {
        StartCoroutine(ShrinkObject());
    }
}
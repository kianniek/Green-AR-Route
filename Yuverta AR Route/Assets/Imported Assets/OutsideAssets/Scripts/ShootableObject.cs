using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableObject : MonoBehaviour
{
    public GameObject bird; // Drag het vogelobject hier in de Inspector
    private bool birdFlown = false;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet" && !birdFlown)
        {
            bird.SetActive(true);
            bird.GetComponent<Animator>().SetTrigger("Fly");
            birdFlown = true;
        }
    }
}

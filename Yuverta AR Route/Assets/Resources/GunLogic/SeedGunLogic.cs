using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedGunLogic : BaseGunScript
{
    public override void Shoot()
    {
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
        Debug.Log("Hi" + isPressed());
        while (isPressed())
        {
            base.Shoot();
            yield return new WaitForSeconds(fireRateLimit);
        }
    }
}

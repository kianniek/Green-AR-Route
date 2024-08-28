using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnAwake : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        // Disable the GameObject this script is attached to
        gameObject.SetActive(false);
        
        // Remove this script from the GameObject
        Destroy(this);
    }
}

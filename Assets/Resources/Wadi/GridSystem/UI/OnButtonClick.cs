using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnButtonClick : MonoBehaviour
{
    public GameObject border;
    
    // Start is called before the first frame update
    void Start()
    {
        border.SetActive(false);
    }

    public void OnClick()
    {
        border.SetActive(!border.activeSelf);
    }
}

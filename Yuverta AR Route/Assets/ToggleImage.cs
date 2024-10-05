using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ToggleImage : MonoBehaviour
{
   private Image image;
   
   private void Awake()
   {
      image = GetComponent<Image>();
   }
   
   public void ToggleVisibility()
   {
      image.enabled = !image.enabled;
   }
}

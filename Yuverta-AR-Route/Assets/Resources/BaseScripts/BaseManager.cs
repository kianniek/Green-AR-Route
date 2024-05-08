using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is to be inherited by all managers
public class BaseManager : MonoBehaviour
{
    //This function handles when a new object is placed
    public virtual void NewObjectPlaced()
    {
        
    }
    
    //This function handles when an object is selected
    public virtual void SelectedObject(GameObject gameObject)
    {
        
    }

    //This function handles when an object is destroyed
    public virtual void DestroyObject()
    {
        
    }
}

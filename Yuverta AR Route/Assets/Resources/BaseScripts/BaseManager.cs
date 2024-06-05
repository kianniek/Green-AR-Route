using UnityEngine;

//This class is to be inherited by all managers
public class BaseManager : MonoBehaviour
{
    //This function handles when a new object is placed
    public virtual void NewObjectPlaced()
    {
        
    }
    
    //This function handles when an object is selected
    public virtual void SelectedObject(GameObject obj)
    {
        
    }

    public virtual void UpdateObject()
    {
        
    }

    //This function handles when an object is destroyed
    public virtual void DestroyObject(GameObject objectToDestroy = null)
    {
        
    }
}

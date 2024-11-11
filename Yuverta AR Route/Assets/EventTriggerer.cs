using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventTriggerer : MonoBehaviour
{
    GridManager gridManager;
    
    [SerializeField] private UnityEvent<bool> onBottomLayerFilled = new UnityEvent<bool>();
    
    bool hasAssignedToEvent = false;

    [SerializeField]
    private List<GameObject> objectsToActivate = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        gridManager ??= FindObjectOfType<GridManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasAssignedToEvent) 
            return;
        
        gridManager ??= FindObjectOfType<GridManager>();
        if (gridManager == null) return;

        gridManager.onBottomLayerFilled.AddListener(onBottomLayerFilled.Invoke);
        hasAssignedToEvent = true;
    }
    
     public void ActivateObjects(bool value)
    {
        if(!value)
            return;
        
        foreach (var obj in objectsToActivate)
        {
            obj.SetActive(true);
        }
        
        gridManager.onBottomLayerFilled.RemoveListener(onBottomLayerFilled.Invoke);
    }
}

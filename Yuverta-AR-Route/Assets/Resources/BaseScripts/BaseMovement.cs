using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMovement : MonoBehaviour
{
    protected virtual BaseManager currentManager { get; set; }
    public virtual bool animationActive { get; set; }

    protected virtual IEnumerator TrackTouchPosition()
    {
        while (Input.GetMouseButton(0) || Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Ended)
        {
            SwipeDetection.Instance.trackingObject = true;
            Vector3 newPosition = SharedFunctionality.GetTouchWorldPosition();
            newPosition.y = gameObject.transform.position.y;
            gameObject.transform.position = newPosition;
            
            yield return new WaitForFixedUpdate();
        }

        var closestGridPosition = GridManager.Instance.SnapToGrid(gameObject);
        
        SwipeDetection.Instance.trackingObject = false;

        animationActive = true;
        while (Vector3.Distance(gameObject.transform.position, closestGridPosition) > 0.002 && animationActive)
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position,closestGridPosition , 0.05f);
            yield return new WaitForSeconds(0.01f);

            if (!Input.GetMouseButtonDown(0) || Input.touchCount == 0) continue;

            SwipeDetection.Instance.CollideWithObject(out var collidedObject);
            if (!collidedObject || collidedObject != gameObject || collidedObject.GetComponent<ObjectMovement>() != this) continue;
            
            currentManager.SelectedObject(collidedObject);
            animationActive = false;
            StopCoroutine(TrackTouchPosition());
        }
        
        gameObject.transform.position = closestGridPosition;
    }
}

using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CollisionAndTriggerEvents : MonoBehaviour
{
    // Collision events
    [Header("Collision Events")]
    public UnityEvent<Collision> OnCollisionEnterEvent;
    public UnityEvent<Collision> OnCollisionStayEvent;
    public UnityEvent<Collision> OnCollisionExitEvent;

    // Trigger events
    [Header("Trigger Events")]
    public UnityEvent<Collider> OnTriggerEnterEvent;
    public UnityEvent<Collider> OnTriggerStayEvent;
    public UnityEvent<Collider> OnTriggerExitEvent;

    // Called when a collision starts
    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEnterEvent?.Invoke(collision);
    }

    // Called every frame while in a collision
    private void OnCollisionStay(Collision collision)
    {
        OnCollisionStayEvent?.Invoke(collision);
    }

    // Called when a collision ends
    private void OnCollisionExit(Collision collision)
    {
        OnCollisionExitEvent?.Invoke(collision);
    }

    // Called when a trigger is entered
    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterEvent?.Invoke(other);
    }

    // Called every frame while inside a trigger
    private void OnTriggerStay(Collider other)
    {
        OnTriggerStayEvent?.Invoke(other);
    }

    // Called when exiting a trigger
    private void OnTriggerExit(Collider other)
    {
        OnTriggerExitEvent?.Invoke(other);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class SwipeDetection : MonoBehaviour 
{
	public static SwipeDetection Instance;

	public bool trackingObject;
	public delegate void Swipe(Vector2 direction);
	public event Swipe swipePerformed;
	[SerializeField] private InputAction position, press;

	[SerializeField] private float swipeResistance = 100;
	private Vector2 initialPos;
	private Vector2 currentPos => position.ReadValue<Vector2>();
	private void Awake () 
	{
		Instance = this;
		position.Enable();
		press.Enable();	
		press.performed += _ => CheckPressLocation();
		press.canceled += _ => DetectSwipe();
	}

	private void CheckPressLocation()
	{
		if (SharedFunctionality.Instance.TouchUI()) return;
		initialPos = currentPos;
		
		GameObject collidedObject = null;
		if (trackingObject || CollideWithObject(out collidedObject))
		{
			try
			{
				if (collidedObject) GridManager.Instance.SelectedObject(collidedObject);
			}
			catch (Exception e)
			{
				FindObjectOfType<GridManager>().objectMovement.MoveObject();
			}
			return;
		}
	}

	private void DetectSwipe() 
	{
		if (trackingObject || SharedFunctionality.Instance.TouchUI()) return;
		Vector2 delta = currentPos - initialPos;
		Vector2 direction = Vector2.zero;
		if(Mathf.Abs(delta.x) > swipeResistance)
		{
			direction.x = Mathf.Clamp(delta.x, -1, 1);
		}
		if(Mathf.Abs(delta.y) > swipeResistance)
		{
			direction.y = Mathf.Clamp(delta.y, -1, 1);
		}
		if(direction != Vector2.zero & swipePerformed != null)
			swipePerformed(direction);
	}

	public bool CollideWithObject(out GameObject collidedObject)
	{
		var hits = SharedFunctionality.Instance.TouchToRay();
		foreach (var hit in hits)
		{
			if (!hit.collider.gameObject.CompareTag("MoveableObject")) continue;
			
			collidedObject = hit.collider.gameObject;
			return true;
		}

		collidedObject = null;
		return false;
	}
}

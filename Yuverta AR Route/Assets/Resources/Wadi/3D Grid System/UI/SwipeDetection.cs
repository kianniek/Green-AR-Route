using System;
using System.Collections.Generic;
using UnityEngine;
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

	private float clickTime;
	private const float ClickDelay = 0.5f;
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
		Debug.Log("CheckPressLocation");
		if (Conditions() || trackingObject) return;
		
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
				Debug.LogError(e);
				if (trackingObject) return;
				GridManager.Instance.objectMovement.MoveObject();
			}
		}
	}

	private void DetectSwipe() 
	{
		if (trackingObject || Conditions()) return;
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
		if(direction != Vector2.zero & swipePerformed != null && !trackingObject)
			swipePerformed(direction);
	}

	public bool CollideWithObject(out GameObject collidedObject)
	{
		List<GameObject> collidedObjects = new List<GameObject>();
		var hits = SharedFunctionality.Instance.TouchToRay();
		foreach (var hit in hits)
		{
			if (!hit.collider.gameObject.CompareTag("MoveableObject")) continue;
			
			if (Time.time - clickTime < ClickDelay && Time.time - clickTime > 0.1f)
			{
				GridManager.Instance.DestroyObject();
				collidedObject = null;
				continue;
			}
		
			clickTime = Time.time;
			collidedObject = hit.collider.gameObject;
			collidedObjects.Add(collidedObject);
		}
		
		switch (collidedObjects.Count)
		{
			case > 1:
				collidedObject = DistanceChecker(collidedObjects);
				return true;
			case 1:
				collidedObject = collidedObjects[0];
				return true;
			default:
				collidedObject = null;
				return false;
		}
	}

	private GameObject DistanceChecker(List<GameObject> checkedObjects)
	{
		Vector3 user = Camera.main!.transform.position;
		float closestDistance = Mathf.NegativeInfinity;
		GameObject closestObject = null;

		foreach (var obj in checkedObjects)
		{
			var currentDistance = Vector3.Distance(user, obj.transform.position);
			if (currentDistance < closestDistance)
			{
				closestDistance = currentDistance;
				closestObject = obj;
			}
		}

		return closestObject;
	}

	private bool Conditions()
	{
		return SharedFunctionality.Instance.TouchUI() || GridManager.Instance.uiMenu.isDragging;
	}
}

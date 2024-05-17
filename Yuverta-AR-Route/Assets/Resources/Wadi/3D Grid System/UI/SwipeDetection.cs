using System;
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
		if (Conditions()) return;
		
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
		if(direction != Vector2.zero & swipePerformed != null)
			swipePerformed(direction);
	}

	public bool CollideWithObject(out GameObject collidedObject)
	{
		var hits = SharedFunctionality.Instance.TouchToRay();
		foreach (var hit in hits)
		{
			if (!hit.collider.gameObject.CompareTag("MoveableObject")) continue;
			
			if (Time.time - clickTime < ClickDelay && Time.time - clickTime > 0.1f)
			{
				GridManager.Instance.DestroyObject();
				collidedObject = null;
				return false;
			}
		
			clickTime = Time.time;
			collidedObject = hit.collider.gameObject;
			return true;
		}

		collidedObject = null;
		return false;
	}

	private bool Conditions()
	{
		return SharedFunctionality.Instance.TouchUI() || GridManager.Instance.uiMenu.isDragging;
	}
}

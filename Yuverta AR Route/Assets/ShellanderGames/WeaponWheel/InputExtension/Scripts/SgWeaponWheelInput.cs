using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Weapon wheel input handling. Reacts to input and highlights or selects appropiate 
	/// slice.
	/// </summary>
	public class SgWeaponWheelInput : SgGenericInput
	{
		[Tooltip("Unique input action map for this weapon wheel")]
		public InputActionMap actionMap;
		[Tooltip("Use your own existing player input")]
		public bool useExistingPlayerInput = false;
		[Tooltip("Your own existing player input")]
		public PlayerInput playerInput;
		[Tooltip("Action map name in your player input settings")]
		public string playerInputActionMapName;

		[HideInInspector]
		public InputActionMap currentActionMap;

		private static readonly string m_GamepadConfirmActionName = "Confirm";
		private static readonly string m_MousePointerActionName = "PointerPosition";
		private static readonly string m_GamepadStickActionName = "RightStick";
		private static readonly string m_ShowWeaponWheelActionName = "ToggleWeaponWheel";

		private InputAction m_GamepadConfirmAction;
		private InputAction m_ToggleWheelAction;
		private InputAction m_PointerAction;
		private InputAction m_StickAction;

		/// <summary>
		/// Resets the components. Readd action map to the input parser.
		/// </summary>
		public override void ResetComponent()
		{
			base.ResetComponent();
			currentActionMap = null;
			if(playerInput == null && useExistingPlayerInput) {
				playerInput = FindObjectOfType<PlayerInput>();
			}
			if (playerInput != null) {
				foreach (UnityEngine.InputSystem.InputActionMap map in playerInput.actions.actionMaps)
				{
					if (map.name == playerInputActionMapName)
					{
						currentActionMap = map.Clone();
						break;
					}
				}
				if(currentActionMap == null && playerInput.currentActionMap != null) {
					playerInputActionMapName = playerInput.currentActionMap.name;
					currentActionMap = playerInput.currentActionMap.Clone();
				}
			}
			if (currentActionMap == null) {
				currentActionMap = actionMap.Clone(); //doesn't seem to work otherwise after first reset.
			}
			m_GamepadConfirmAction = FindAction(m_GamepadConfirmActionName);
			m_ToggleWheelAction = FindAction(m_ShowWeaponWheelActionName);
			m_PointerAction = FindAction(m_MousePointerActionName);
			m_StickAction = FindAction(m_GamepadStickActionName);
			currentActionMap.Enable();
		}

		private InputAction FindAction(string name) {
			InputAction action = currentActionMap.FindAction(name, false);
			if(action == null) {
				Debug.Log("Ignoring input action: " + name);
			}
			return action;
		}

		protected override bool IsToggleWheelButtonPressed => m_ToggleWheelAction != null && m_ToggleWheelAction.IsPressed();
		protected override bool WasToggleWheelButtonPressedNow => m_ToggleWheelAction != null && m_ToggleWheelAction.WasPressedThisFrame();
		protected override bool WasGamepadConfirmButtonPressedNow => m_GamepadConfirmAction != null && m_GamepadConfirmAction.WasPressedThisFrame();
		protected override bool WasGamepadConfirmButtonReleasedNow => m_GamepadConfirmAction != null && m_GamepadConfirmAction.WasReleasedThisFrame();
		protected override bool IsPointerActionEnabled => m_PointerAction != null;
		protected override Vector2 CurrentPointerValue => IsPointerActionEnabled ? m_PointerAction.ReadValue<Vector2>() : Vector2.zero;
		protected override Vector2 CurrentStickValue => m_StickAction != null ? m_StickAction.ReadValue<Vector2>() : Vector2.zero;
		protected override bool WasDigitPressedNow(int digit)
		{
			Keyboard keyboard = Keyboard.current;
			if(keyboard == null) {
				return false;
			}
			KeyControl keyControl;
			switch (digit) {
				case 1:
					keyControl = keyboard.digit1Key;
					break;
				case 2:
					keyControl = keyboard.digit2Key;
					break;
				case 3:
					keyControl = keyboard.digit3Key;
					break;
				case 4:
					keyControl = keyboard.digit4Key;
					break;
				case 5:
					keyControl = keyboard.digit5Key;
					break;
				case 6:
					keyControl = keyboard.digit6Key;
					break;
				case 7:
					keyControl = keyboard.digit7Key;
					break;
				case 8:
					keyControl = keyboard.digit8Key;
					break;
				case 9:
					keyControl = keyboard.digit9Key;
					break;
				case 0:
					keyControl = keyboard.digit0Key;
					break;
				default:
					return false;
			}
			return keyControl.wasPressedThisFrame;
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(SgWeaponWheelInput))]
	public class SgWeaponWheelInputInspector : Editor
	{
		private SgWeaponWheelInput m_MainObject;
		private SerializedProperty[] m_MainProperties;
		private SerializedProperty[] m_PlayerInputProperties;
		private SerializedProperty[] m_NonPlayerInputProperties;


		protected void OnEnable()
		{
			m_MainObject = (SgWeaponWheelInput)serializedObject.targetObject;
			m_MainProperties = FindProperties("useExistingPlayerInput", "holdButtonToShow", "hideOnSelected", "showAgainCooldown",
				"highlightSliceOnDigitPress", "selectSliceOnDigitPress", "firstSliceOffset", "confirmOnButtonRelease");
			m_PlayerInputProperties = FindProperties("playerInput", "playerInputActionMapName");
			m_NonPlayerInputProperties = FindProperties("actionMap");
		}

		public override void OnInspectorGUI()
		{
			bool doUsePlayerInput = serializedObject.FindProperty("useExistingPlayerInput").boolValue;
			
			DrawProperties(m_MainProperties);

			if(doUsePlayerInput) 
			{
				DrawProperties(m_PlayerInputProperties);
			}
			else
			{
				DrawProperties(m_NonPlayerInputProperties);
			}

			bool hasChanged = serializedObject.hasModifiedProperties;
			serializedObject.ApplyModifiedProperties();

			if(hasChanged) {
				m_MainObject.ResetComponent();
			}
		}

		private void DrawProperties(SerializedProperty[] properties)
		{
			foreach (SerializedProperty property in properties)
			{
				EditorGUILayout.PropertyField(property);
			}
		}

		private SerializedProperty[] FindProperties(params string[] propertyNames)
		{
			SerializedProperty[] properties = new SerializedProperty[propertyNames.Length];
			for (int i = 0; i < propertyNames.Length; i++)
			{
				properties[i] = serializedObject.FindProperty(propertyNames[i]);
			}
			return properties;
		}
	}
#endif
}
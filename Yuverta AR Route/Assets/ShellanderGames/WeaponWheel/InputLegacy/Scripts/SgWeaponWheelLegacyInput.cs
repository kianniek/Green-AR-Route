using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Input handling for the old input system.
	/// </summary>
	public class SgWeaponWheelLegacyInput : SgGenericInput
	{
		public string confirmButtonName = "Fire1";
		public string toggleButtonName = "Fire3";
		public string gamepadXAxisName = "Horizontal";
		public string gamepadYAxisName = "Vertical";

		private Vector2 m_CachedStickValue;

		protected override bool IsToggleWheelButtonPressed => Input.GetButton(toggleButtonName);
		protected override bool WasToggleWheelButtonPressedNow => Input.GetButtonDown(toggleButtonName);
		protected override bool WasGamepadConfirmButtonPressedNow => Input.GetButtonDown(confirmButtonName);
		protected override bool WasGamepadConfirmButtonReleasedNow => Input.GetButtonUp(confirmButtonName);
		protected override bool IsPointerActionEnabled => true;
		protected override Vector2 CurrentPointerValue => Input.mousePosition;
		protected override Vector2 CurrentStickValue {
			get {
				m_CachedStickValue.x = Input.GetAxis(gamepadXAxisName);
				m_CachedStickValue.y = Input.GetAxis(gamepadYAxisName);
				return m_CachedStickValue;
			}
		}

		protected override bool WasDigitPressedNow(int digit)
		{
			KeyCode keyCode;
			switch(digit) {
				case 1:
					keyCode = KeyCode.Alpha1;
					break;
				case 2:
					keyCode = KeyCode.Alpha2;
					break;
				case 3:
					keyCode = KeyCode.Alpha3;
					break;
				case 4:
					keyCode = KeyCode.Alpha4;
					break;
				case 5:
					keyCode = KeyCode.Alpha5;
					break;
				case 6:
					keyCode = KeyCode.Alpha6;
					break;
				case 7:
					keyCode = KeyCode.Alpha7;
					break;
				case 8:
					keyCode = KeyCode.Alpha8;
					break;
				case 9:
					keyCode = KeyCode.Alpha9;
					break;
				case 0:
					keyCode = KeyCode.Alpha0;
					break;
				default:
					return false;
			}
			return Input.GetKeyDown(keyCode);
		}
	}
}

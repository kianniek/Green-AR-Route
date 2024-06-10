using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Generic input handling. Base class for SgWeaponWheelInput and SgWeaponWheelLegacyInput.
	/// </summary>
	public abstract class SgGenericInput : MonoBehaviour
	{
		[Tooltip("If true, the ToggleWeaponWheel must be pressed down to show weapon wheel. If false, the weapon wheel will toggle on button presses.")]
		public bool holdButtonToShow = true;
		[Tooltip("Hide weapon wheel when an item has been selected.")]
		public bool hideOnSelected = true;
		[Tooltip("Delay before weapon wheel can be displayed again after selecting (hold button mode only).")]
		public float showAgainCooldown = 0.4f;
		[Tooltip("Highlight slice when a keyboard digit is pressed.")]
		public bool highlightSliceOnDigitPress = false;
		[Tooltip("Select slice when a keyboard digit is pressed. Works even when the weapon wheel is invisible.")]
		public bool selectSliceOnDigitPress = false;
		[Tooltip("Changes which slice will be considered the first, the slice that will be selected when 1 is pressed. This can be a negative value.")]
		public int firstSliceOffset = 0;
		[Tooltip("Confirm when gamepad button is released instead of when it's pressed")]
		public bool confirmOnButtonRelease = false;

		private SgWeaponWheel m_WeaponWheel;
		private float m_CurrentCooldown = 0f;
		private bool m_HasReleasedButtonAfterSelect = true;
		private Vector2 m_PrevStickValue;

		protected virtual void Start()
		{
			m_WeaponWheel = GetComponent<SgWeaponWheel>();
			m_WeaponWheel.AddEventCallback(OnEvent);

			ResetComponent();
		}

		private void OnEvent(SgWeaponWheelEvent wheelEvent)
		{
			if (hideOnSelected && wheelEvent.type == SgWeaponWheelEventType.Select)
			{
				m_WeaponWheel.IsVisible = false;
				m_CurrentCooldown = showAgainCooldown;
				m_HasReleasedButtonAfterSelect = false;
			}
		}

		/// <summary>
		/// Resets the components. Readd action map to the input parser.
		/// </summary>
		public virtual void ResetComponent()
		{
			m_HasReleasedButtonAfterSelect = true;
			m_CurrentCooldown = 0;
		}


		protected virtual bool IsToggleWheelButtonPressed => false;
		protected virtual bool WasToggleWheelButtonPressedNow => false;
		protected virtual bool WasGamepadConfirmButtonPressedNow => false;
		protected virtual bool WasGamepadConfirmButtonReleasedNow => false;
		protected virtual bool IsPointerActionEnabled => false;
		protected virtual Vector2 CurrentPointerValue => Vector2.zero;
		protected virtual Vector2 CurrentStickValue => Vector2.zero;
		protected virtual bool WasDigitPressedNow(int digit)
		{
			return false;
		}

		protected virtual void Update()
		{
			if (highlightSliceOnDigitPress || selectSliceOnDigitPress)
			{
				for (int i = 1; i <= 10; i++)
				{
					int digit = i == 10 ? 0 : i;
					if (WasDigitPressedNow(digit))
					{
						if (m_WeaponWheel.sliceContents.Count < i)
						{
							break;
						}
						int index = i - 1 + firstSliceOffset;
						if (index < 0)
						{
							index = m_WeaponWheel.sliceContents.Count + index;
						}
						SgSliceController sliceController = m_WeaponWheel.sliceContents[index];

						if (!m_WeaponWheel.IsVisible)
						{
							m_WeaponWheel.IsVisible = true;
						}


						foreach (SgSliceController aSliceController in m_WeaponWheel.sliceContents)
						{
							aSliceController.SetHighlighted(aSliceController == sliceController, false);
						}
						if (sliceController != null && selectSliceOnDigitPress)
						{
							sliceController.Select();
						}

						return;
					}
				}
			}

			if (holdButtonToShow)
			{
				bool isVisible = m_WeaponWheel.IsVisible;
				bool willBeVisible = IsToggleWheelButtonPressed && m_CurrentCooldown == 0 && m_HasReleasedButtonAfterSelect;
				if (isVisible && !willBeVisible)
				{
					m_WeaponWheel.SelectHighlighted();
				}
				m_WeaponWheel.IsVisible = willBeVisible;
				if (!m_HasReleasedButtonAfterSelect)
				{
					m_HasReleasedButtonAfterSelect = !IsToggleWheelButtonPressed;
				}
			}
			else
			{
				if (WasToggleWheelButtonPressedNow)
				{
					m_WeaponWheel.IsVisible = !m_WeaponWheel.IsVisible;
				}
			}

			if (m_CurrentCooldown > 0)
			{
				m_CurrentCooldown -= Time.deltaTime;
			}
			else
			{
				m_CurrentCooldown = 0;
			}


			if (m_WeaponWheel.IsVisible)
			{
				if (IsPointerActionEnabled)
				{
					Vector2 mousePosition = CurrentPointerValue;
					m_WeaponWheel.PrioritizeClosest(mousePosition);
				}

				Vector2 stickValue = CurrentStickValue;
				bool hasValue = stickValue.x != 0 || stickValue.y != 0;
				bool hadValue = m_PrevStickValue.x != 0 || m_PrevStickValue.y != 0;
				bool wasReleasedNow = !hasValue && hadValue;
				if (hasValue)
				{
					float angle = SgMiscUtil.Angle(stickValue);
					m_WeaponWheel.HighlightClosest(angle);
				}
				else if (wasReleasedNow)
				{
					m_WeaponWheel.DehighlightAll(false);
				}

				m_PrevStickValue = stickValue;
			}
			else
			{
				m_PrevStickValue = Vector2.zero;
			}



			bool gamepadOrMobileConfirm = (!confirmOnButtonRelease && WasGamepadConfirmButtonPressedNow) ||
				(confirmOnButtonRelease && WasGamepadConfirmButtonReleasedNow);
			if (gamepadOrMobileConfirm)
			{
				m_WeaponWheel.SelectHighlighted();
			}
		}

		protected virtual void OnDestroy()
		{
			m_WeaponWheel?.RemoveEventCallback(OnEvent);
		}
	}

}

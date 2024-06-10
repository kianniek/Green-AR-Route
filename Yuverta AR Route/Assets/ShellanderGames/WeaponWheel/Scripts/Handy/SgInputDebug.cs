using System;
using UnityEngine;
using UnityEngine.UI;

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Updates a text component to display SgWeaponWheelEvents from an attached SgWeaponWheel.
	/// </summary>
	public class SgInputDebug : MonoBehaviour
	{
		public SgWeaponWheel weaponWheel;
		private Text m_Text;

		private void Awake()
		{
			if (weaponWheel == null)
			{
				weaponWheel = FindObjectOfType<SgWeaponWheel>();
			}

			weaponWheel.AddEventCallback(OnEvent);
			this.m_Text = GetComponent<Text>();
		}

		private void OnEvent(SgWeaponWheelEvent wheelEvent)
		{
			if (wheelEvent.type == SgWeaponWheelEventType.Highlight || wheelEvent.type == SgWeaponWheelEventType.Select)
			{
				m_Text.text = wheelEvent.ToString();
			}
		}

		private void OnDestroy()
		{
			weaponWheel.RemoveEventCallback(OnEvent);
		}
	}
}

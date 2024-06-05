using TMPro;
using UnityEngine;

namespace ShellanderGames.WeaponWheel
{
	public class SgWeaponInfoDemo : MonoBehaviour
	{
		public GameObject content;
		public TextMeshProUGUI textComponent;
		public SgWeaponWheel weaponWheel;
		public string prefix = "Item: ";

		private void Awake()
		{
			if (weaponWheel == null)
			{
				weaponWheel = FindObjectOfType<SgWeaponWheel>();
			}
			weaponWheel.AddEventCallback(EventCallback);
		}

		private void EventCallback(SgWeaponWheelEvent wheelEvent)
		{
			switch (wheelEvent.type)
			{
				case SgWeaponWheelEventType.WheelVisible:
					content.SetActive(true);
					break;
				case SgWeaponWheelEventType.WheelInvisible:
					content.SetActive(false);
					break;
				case SgWeaponWheelEventType.Highlight:
					OnHighlight(wheelEvent.slice.sliceName);
					break;
				case SgWeaponWheelEventType.Dehighlight:
					textComponent.text = "";
					break;
			}
		}

		private void OnHighlight(string name)
		{
			string newText = prefix + name;
			switch (name)
			{
				case "Pistol":
					newText += "\nAmmo: 10";
					break;
			}
			textComponent.text = newText;
		}
	}
}

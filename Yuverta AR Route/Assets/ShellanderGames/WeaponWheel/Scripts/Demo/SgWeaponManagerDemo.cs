using ShellanderGames.WeaponWheel;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShellanderGames.WeaponWheel
{
	public class SgWeaponManagerDemo : MonoBehaviour
	{
		public SgDemoWeapon[] weapons;
		public SgWeaponWheel weaponWheel;
		public TextMeshProUGUI textComponent;

		private SgDemoWeapon currentWeapon;

		private void Start()
		{
			currentWeapon = weapons[0];

			foreach(SgSliceController slice in weaponWheel.sliceContents) {
				if(slice.sliceName != "") {
					SgDemoWeapon weapon = FindWeapon(slice.sliceName);
					weapon.ammoTextComponent = slice.GetComponentInChildren<TextMeshProUGUI>();
				}
			}

			UpdateUi();
		}

		/// <summary>
		/// Is plugged as a unity event in the Weapon Wheel inspector.
		/// </summary>
		/// <param name="slice">selected slice</param>
		public void OnSliceSelected(SgSliceController slice)
		{
			string weaponName = slice.sliceName;
			if (weaponName == "")
			{
				return;
			}
			SgDemoWeapon selectedWeapon = FindWeapon(weaponName);
			currentWeapon = selectedWeapon;
			UpdateUi();
		}

		private void UpdateUi()
		{
			textComponent.text = "Selected weapon is " + currentWeapon.name + ", ammo left: " + currentWeapon.ammoLeft;
			foreach (SgDemoWeapon weapon in weapons)
			{
				if (weapon.ammoTextComponent != null)
				{
					weapon.ammoTextComponent.text = weapon.ammoLeft + " / " + weapon.maxAmmo;
				}
			}
		}

		private SgDemoWeapon FindWeapon(string name)
		{
			foreach (SgDemoWeapon weapon in weapons)
			{
				if (weapon.name == name)
				{
					return weapon;
				}
			}
			throw new System.Exception("Couldn't find weapon:" + name);
		}

		private void Update()
		{
			bool fireButtonWasPressedNow = Keyboard.current.spaceKey.wasPressedThisFrame;
			if (fireButtonWasPressedNow)
			{
				if (currentWeapon.ammoLeft > 0)
				{
					Fire();
				}
			}
		}

		private void Fire()
		{
			currentWeapon.ammoLeft -= 1;
			UpdateUi();
		}
	}

	[System.Serializable]
	public class SgDemoWeapon
	{
		public string name;
		public int maxAmmo;
		public int ammoLeft;
		public TextMeshProUGUI ammoTextComponent;
	}
}

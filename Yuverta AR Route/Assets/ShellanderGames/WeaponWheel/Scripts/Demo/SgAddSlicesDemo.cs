using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Demo script that shows how a weapon wheel can be populated during runtime.
	/// </summary>
	public class SgAddSlicesDemo : MonoBehaviour
	{
		[Tooltip("Weapon wheel to update")]
		public SgWeaponWheel weaponWheel;
		[Tooltip("Slices to populate the weapon wheel with")]
		public SgSliceController[] sliceContents;
		[Tooltip("Delay between updates")]
		public float delay;

		private float m_ScheduledDelay;
		private int m_CurrentIndex;
		private int m_ChangeDirection;

		private void Start()
		{
			m_CurrentIndex = 0;
			m_ScheduledDelay = delay;
			m_ChangeDirection = 1;
		}

		private void FixedUpdate()
		{
			if (m_ScheduledDelay > 0)
			{
				//Wait until the delay is over
				m_ScheduledDelay -= Time.deltaTime;
				return;
			}

			if(m_ChangeDirection == 1) {
				if (m_CurrentIndex < sliceContents.Length)
				{
					//Add a slice to the weapon wheel.
					weaponWheel.sliceContents.Add(sliceContents[m_CurrentIndex]);
					//Refresh the weapon wheel.
					weaponWheel.Generate(false, true);
				} else {
					m_ChangeDirection = -1;
				}
			} else if(m_ChangeDirection == -1) {
				if (m_CurrentIndex >= 0)
				{
					//Remove a slice from the weapon wheel.
					weaponWheel.sliceContents.Remove(sliceContents[m_CurrentIndex]);
					//Keep the slice as a child to this object.
					sliceContents[m_CurrentIndex].transform.SetParent(this.transform);
					//Refresh the weapon wheel.
					weaponWheel.Generate(false, true);
				}
				else
				{
					m_ChangeDirection = 1;
				}
			}
			

			m_CurrentIndex += m_ChangeDirection;

			m_ScheduledDelay = delay;
		}
	}
}
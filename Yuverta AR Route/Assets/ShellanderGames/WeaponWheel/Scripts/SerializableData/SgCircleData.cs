using System;
using UnityEngine;

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Serializable circle data.
	/// </summary>
	[Serializable]
	public class SgCircleData
	{
		[Tooltip("It's important to mark the main content as main using the checkbox to align the content graphics properly. There can only be one.")]
		public bool isMain = false;
		[Tooltip("Outer diameter (circle size).")]
		public float diameter = 1000;
		public float thickness = 200;

		public SgPatternSelectableData colors = new SgPatternSelectableData();
		
		[Tooltip("If you want unique colors for each slice.")]
		public SgOverridingSliceSelectableData[] overridingSliceColors;
		[Tooltip("If you want unique circle sprites for this circle.")]
		public SgCircleSpriteData overridingSprites = new SgCircleSpriteData();

		public float Circumference => diameter * Mathf.PI;
		public float HoleDiameter => diameter - thickness;

		public float Radius => diameter / 2f;
		public float CenterRadius => (diameter - (thickness/2f))/2f;
		public float InvertedThickness => diameter - thickness;
		public float Scale => diameter / InvertedThickness;
		public float Offset => (Scale - 1) * -0.5f;

		/// <summary>
		/// Returns overriding selectable data for a specified slice index if there are any.
		/// </summary>
		/// <param name="sliceIndex"></param>
		/// <returns></returns>
		public SgSelectableData GetOverridingSelectableData(int sliceIndex) {
			if(overridingSliceColors != null) {
				foreach(SgOverridingSliceSelectableData data in overridingSliceColors) {
					if(data.sliceIndex == sliceIndex) {
						return data.colors;
					}
				}
			}

			return null;
		}

		//Inspector GUI internals
		[HideInInspector]
		public bool isFoldedOut = true;
	}

	/// <summary>
	/// Slice data with an index to identify which slice.
	/// </summary>
	[Serializable]
	public class SgOverridingSliceSelectableData {
		public int sliceIndex = 0;
		public SgSelectableData colors = new SgSelectableData();
	}
}
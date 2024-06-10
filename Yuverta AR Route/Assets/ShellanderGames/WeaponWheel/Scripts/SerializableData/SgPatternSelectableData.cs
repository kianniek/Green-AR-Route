using UnityEngine;

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Selectable data with slice pattern.
	/// </summary>
	[System.Serializable]
	public class SgPatternSelectableData : SgSelectableData
	{
		[Tooltip("Fill circle with texture")]
		public Texture2D fillTexture;
		public float textureScale = 1;
		public float textureRotation = 0;
		public float textureOffset = 0;

		public override Texture2D GetFillTexture()
		{
			return fillTexture;
		}
	}
}

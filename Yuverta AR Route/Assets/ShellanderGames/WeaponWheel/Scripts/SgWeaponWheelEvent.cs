namespace ShellanderGames.WeaponWheel
{
	public enum SgWeaponWheelEventType {
		/// <summary>
		/// A slice is not highlighted anymore
		/// </summary>
		Dehighlight,
		/// <summary>
		/// A slice is now highlighted
		/// </summary>
		Highlight,
		/// <summary>
		/// A slice has been selected (confirmed)
		/// </summary>
		Select,
		/// <summary>
		/// Weapon wheel is now visible
		/// </summary>
		WheelVisible,
		/// <summary>
		/// Weapon wheel is now hidden
		/// </summary>
		WheelInvisible
	}

	/// <summary>
	/// Used to notify important weapon wheel events.
	/// </summary>
	public struct SgWeaponWheelEvent
	{
		public readonly SgWeaponWheelEventType type;
		public readonly SgSliceController slice;

		public SgWeaponWheelEvent(SgWeaponWheelEventType type, SgSliceController slice)
		{
			this.type = type;
			this.slice = slice;
		}

		public override string ToString()
		{
			return $"SgWeaponWheelEvent[type={type}, slice={slice?.name}, {slice?.sliceIndex}, {slice?.sliceName}]";
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Makes slice borders findable
	/// </summary>
	public class SgSliceBorder : MonoBehaviour
	{
		private SgSelectable m_Selectable;
		public SgSelectable Selectable => SgMiscUtil.LazyComponent(this, ref m_Selectable);
	}
}

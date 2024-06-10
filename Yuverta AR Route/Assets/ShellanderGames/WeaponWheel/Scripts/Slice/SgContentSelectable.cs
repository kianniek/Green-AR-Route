#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// A graphic that can be highlighted and override SgWeaponWheel's contentColors
	/// </summary>
	public class SgContentSelectable : SgSelectable
	{
		public bool useGlobalContentColors = true;

		/// <summary>
		/// Refreshes colors
		/// </summary>
		public override void Refresh(bool forceImmediate)
		{
			if(useGlobalContentColors && WeaponWheel != null) {
				SgSelectableData.Copy(WeaponWheel.sliceContentColors, ref this.colors);
			}
			base.Refresh(forceImmediate);
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(SgContentSelectable))]
	public class SgSelectableInspector : Editor
	{
		private SgContentSelectable m_MainObject;
		private SerializedProperty m_UseGlobalContentColors;
		private SerializedProperty m_TargetGraphic;

		protected void OnEnable()
		{
			m_MainObject = (SgContentSelectable)serializedObject.targetObject;
			m_UseGlobalContentColors = serializedObject.FindProperty("useGlobalContentColors");
			m_TargetGraphic = serializedObject.FindProperty("m_TargetGraphic");
		}

		public override void OnInspectorGUI()
		{
			if(!m_UseGlobalContentColors.boolValue) {
				DrawDefaultInspector();
			} else {
				EditorGUILayout.PropertyField(m_TargetGraphic);
				EditorGUILayout.PropertyField(m_UseGlobalContentColors);
			}
			serializedObject.ApplyModifiedProperties();

			if(!EditorApplication.isPlaying) {
				m_MainObject.Refresh(true);
			}
		}
	}

#endif
}

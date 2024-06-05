using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Serializable data a SgSelectable uses.
	/// </summary>
	[System.Serializable]
	public class SgSelectableData
	{
		[Tooltip("Default/dehighlighted color")]
		public Color color = Color.white;
		[Tooltip("Obsolete/deprecated, use highlight animations instead")]
		public Color highlightColor = Color.white;
		[Tooltip("Highlight/dehighlight tween animations")]
		public SgHighlightAnimationData[] highlightAnimations = new SgHighlightAnimationData[0];
		[Tooltip("Select tween animations")]
		public SgAnimationData[] selectAnimations = new SgAnimationData[0];

		/// <summary>
		/// Returns a fill texture if one exist. Overriden by sub classes.
		/// </summary>
		/// <returns></returns>
		public virtual Texture2D GetFillTexture() {
			return null;
		}

		/// <summary>
		/// Copy data from one object to another
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		public static void Copy(SgSelectableData from, ref SgSelectableData to)
		{
			to.color = from.color;
			to.highlightColor = from.highlightColor;
			
			to.highlightAnimations = CopyAnimations(from.highlightAnimations, to.highlightAnimations);
			to.selectAnimations = CopyAnimations(from.selectAnimations, to.selectAnimations);
		}

		private static T[] CopyAnimations<T>(T[] from, T[] to) where T : SgAnimationData
		{
			if (to == null || to.Length != from.Length)
			{
				to = new T[from.Length];
			}
			for (int i = 0; i < from.Length; i++)
			{
				if (to[i] == null)
				{
					to[i] = (T)Activator.CreateInstance(typeof(T));
				}
				to[i].Copy(from[i]);
			}
			return to;
		}

		/// <summary>
		/// Some masks doesn't work with zero alpha. Correct it.
		/// </summary>
		public void PreventZeroAlpha() {
			PreventZeroAlpha(ref color);
			PreventZeroAlpha(ref highlightColor);
			foreach (SgAnimationData animationData in highlightAnimations)
			{
				PreventZeroAlpha(ref animationData.color);
			}
			foreach (SgAnimationData animationData in selectAnimations)
			{
				PreventZeroAlpha(ref animationData.color);
			}
		}
		private void PreventZeroAlpha(ref Color color) {
			if(color.a == 0) {
				color.a = 1f / 255f;
			}
		}
	}

	[System.Serializable]
	public class SgAnimationData
	{
		public bool animateColor = false;
		[Tooltip("Animate scale and affect children")]
		public bool animateScale = false;
		[Tooltip("Animate a rect transform's size")]
		public bool animateSize = false;
		[Tooltip("Color to animate to, requires animateColor=true")]
		public Color color = Color.white;
		[Tooltip("Scale to animate to, requires animateScale=true or animateSize=true")]
		public Vector2 scale = Vector2.one;
		public float duration;

		public static SgAnimationData Create() {
			return new SgAnimationData();
		}

		public virtual void Copy(SgAnimationData from) {
			this.animateColor = from.animateColor;
			this.animateScale = from.animateScale;
			this.animateSize = from.animateSize;
			this.color = from.color;
			this.scale = from.scale;
			this.duration = from.duration;
		}
	}

	[System.Serializable]
	public class SgHighlightAnimationData : SgAnimationData
	{
		public float dehighligtDuration;

		public override void Copy(SgAnimationData from)
		{
			base.Copy(from);
			this.dehighligtDuration = ((SgHighlightAnimationData)from).dehighligtDuration;
		}
	}

	#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(SgAnimationData))]
	[CustomPropertyDrawer(typeof(SgHighlightAnimationData))]
	public class SgAnimationDataDrawer : PropertyDrawer
	{
		private readonly int yChange = 20;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return yChange * 7;
		}

		private SerializedProperty[] FindProperties(SerializedProperty property, params string[] names) {
			SerializedProperty[] properties = new SerializedProperty[names.Length];
			for(int i = 0; i < names.Length; i++) {
				properties[i] = property.FindPropertyRelative(names[i]);
			}
			return properties;
		}

		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(rect, label, property);
			SerializedProperty[] boolProps = FindProperties(property, "animateColor", "animateScale", "animateSize");
			bool showColor = boolProps[0].boolValue;
			bool showScale = boolProps[1].boolValue || boolProps[2].boolValue;

			EditorGUIUtility.wideMode = true;
			EditorGUIUtility.labelWidth = 150;
			rect.height = yChange;

			foreach (SerializedProperty aProp in boolProps) {
				EditorGUI.PropertyField(rect, aProp);
				rect.y += yChange;
			}
			if(showColor) {
				EditorGUI.PropertyField(rect, property.FindPropertyRelative("color"));
				rect.y += yChange;
			}
			if(showScale) {
				EditorGUI.PropertyField(rect, property.FindPropertyRelative("scale"));
				rect.y += yChange;
			}
			if (showColor || showScale)
			{
				EditorGUI.PropertyField(rect, property.FindPropertyRelative("duration"));
				rect.y += yChange;
				SerializedProperty dehighlightProperty = property.FindPropertyRelative("dehighligtDuration");
				if (dehighlightProperty != null) {
					EditorGUI.PropertyField(rect, dehighlightProperty);
					rect.y += yChange;
				}
			}

			EditorGUI.EndProperty();
		}
	}
	#endif
}

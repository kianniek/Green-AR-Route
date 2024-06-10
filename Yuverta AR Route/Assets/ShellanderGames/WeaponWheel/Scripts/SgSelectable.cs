using UnityEngine;
using UnityEngine.UI;

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// A highlightable graphic.
	/// </summary>
	public class SgSelectable : MonoBehaviour
	{
		private RectTransform m_RectTransform;
		public RectTransform RectTransform => SgMiscUtil.LazyComponent(this, ref m_RectTransform);
		public Image Image => TargetGraphic as Image;
		public Graphic m_TargetGraphic;
		public Graphic TargetGraphic => SgMiscUtil.LazyComponent(this, ref m_TargetGraphic);
		public SgSelectableData colors = new SgSelectableData();

		private SgSelectableData Data => colors;

		private bool m_IsHighlighted = false;
		private bool m_IsSelected = false;
		private Vector2 m_DefaultSize = -Vector2.one;
		private SgWeaponWheel m_WeaponWheel;
		protected SgWeaponWheel WeaponWheel => SgMiscUtil.LazyParentComponent(this, ref m_WeaponWheel);
		private SgAnimationManager AnimationManager => WeaponWheel.AnimationManager;

		private void Start()
		{
			m_DefaultSize = RectTransform.sizeDelta;
		}

		protected virtual void Reset()
		{
			if (Data.highlightAnimations == null)
			{
				Data.highlightAnimations = new SgHighlightAnimationData[0];
			}
			if (Data.selectAnimations == null)
			{
				Data.selectAnimations = new SgAnimationData[0];
			}
		}

		/// <summary>
		/// Refresh colors and such
		/// </summary>
		/// <param name="forceImmediate"></param>
		public virtual void Refresh(bool forceImmediate)
		{
			Vector2 defaultSize = m_DefaultSize;
			if (defaultSize == -Vector2.one)
			{
				defaultSize = RectTransform.sizeDelta;
			}

			bool isHighlighted = m_IsHighlighted;
			if (TargetGraphic != null)
			{

				if (m_IsSelected)
				{
					foreach (SgAnimationData selectAnimationData in Data.selectAnimations)
					{
						if (selectAnimationData.animateColor)
						{
							AnimationManager.AnimateColor(TargetGraphic, selectAnimationData.color, selectAnimationData.duration, null);
						}
						if (selectAnimationData.animateScale)
						{
							AnimationManager.AnimateScale(TargetGraphic,  selectAnimationData.scale, selectAnimationData.duration, null);
						}
						if (selectAnimationData.animateSize)
						{
							AnimationManager.AnimateSize(TargetGraphic, RectTransform, defaultSize, selectAnimationData.scale, selectAnimationData.duration, null);
						}
					}
				}
				else
				{
					bool didSetColor = false;
					foreach (SgHighlightAnimationData highlightAnimationData in Data.highlightAnimations)
					{
						float duration = isHighlighted ? highlightAnimationData.duration : highlightAnimationData.dehighligtDuration;
						if (forceImmediate)
						{
							duration = 0;
						}
						if (highlightAnimationData.animateColor)
						{
							Color newColor = isHighlighted ? highlightAnimationData.color : Data.color;
							AnimationManager.AnimateColor(TargetGraphic, newColor, duration, null);
							didSetColor = true;
						}
						if (highlightAnimationData.animateScale)
						{
							Vector2 newScale = isHighlighted ? highlightAnimationData.scale : Vector2.one;
							AnimationManager.AnimateScale(TargetGraphic, newScale, duration, null);
						}
						if (highlightAnimationData.animateSize)
						{
							Vector2 newScale = isHighlighted ? highlightAnimationData.scale : Vector2.one;
							AnimationManager.AnimateSize(TargetGraphic, RectTransform, defaultSize, newScale, duration, null);
						}
					}
					if (!didSetColor)
					{
						Color color = Data.color;
						if (isHighlighted && Data.highlightAnimations.Length == 0)
						{
							color = Data.highlightColor; //legacy handling
						}
						AnimationManager.AnimateColor(TargetGraphic, color, 0, null);
					}
				}
			}
		}

		/// <summary>
		/// Called when this selectable has been highlighted
		/// </summary>
		/// <param name="highlighted">true to highligt, false to dehighligt</param>
		/// <param name="forceImmediate">duration=0 if true</param>
		public void SetHighlighted(bool highlighted, bool forceImmediate)
		{
			m_IsSelected = false;
			if (highlighted != m_IsHighlighted || forceImmediate)
			{
				m_IsHighlighted = highlighted;
				Refresh(forceImmediate);
			}
		}

		/// <summary>
		/// Called when this selectable has been selected (confirmed/submitted)
		/// </summary>
		public void SetSelected()
		{
			m_IsSelected = true;
			Refresh(false);
		}
	}
}

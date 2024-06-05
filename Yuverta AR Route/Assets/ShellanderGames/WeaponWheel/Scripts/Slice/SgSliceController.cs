using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Responsible for updating a slice and react to pointer events.
	/// </summary>
	public class SgSliceController : MonoBehaviour
	{
		[Tooltip("Set by weapon wheel. It's not recommended to edit it manually.")]
		public int sliceIndex = -1;
		[Tooltip("Optional item/weapon name. Will be passed in Weapon Wheel Events.")]
		public string sliceName;
		[Tooltip("Update these by pressing the \"Find\" button after graphic creation.")]
		public SgSelectable[] graphicSelectables;
		[Tooltip("Set by weapon wheel. It's not recommended to edit it manually.")]
		public SgCircleSlice[] circleSlices;
		[Tooltip("Set by weapon wheel. It's not recommended to edit it manually.")]
		public SgSliceBorder[] sliceBorders;
		[Tooltip("Set by weapon wheel. It's not recommended to edit it manually.")]
		public RectTransform centerPoint;

		public Vector2 CenterPoint => centerPoint != null ? centerPoint.transform.position : this.transform.position;

		private bool m_IsHighlighted = false;
		public bool IsHighlighted => m_IsHighlighted;
		private bool m_IsPrioritized = false;
		public bool IsPrioritized => m_IsPrioritized;

		private RectTransform m_RectTransform;
		public RectTransform RectTransform => SgMiscUtil.LazyComponent(this, ref m_RectTransform);
		private SgWeaponWheel m_WeaponWheel;
		private SgWeaponWheel WeaponWheel => SgMiscUtil.LazyParentComponent(this, ref m_WeaponWheel);
		private SgCircleSlice m_MainCircleSlice;
		private SgCircleSlice MainCircleSlice {
			get {
				if(m_MainCircleSlice == null) {
					foreach(SgCircleSlice circleSlice in circleSlices) {
						if(circleSlice.circleData.isMain) {
							m_MainCircleSlice = circleSlice;
							break;
						}
					}
				}
				return m_MainCircleSlice;
			}
		}
		private bool m_MouseEnterIsScheduled;
		private bool m_MouseExitIsScheduled;
		private bool m_MouseClickIsScheduled;

		private void Start()
		{
			if (MainCircleSlice != null)
			{
				SetupMouseListening();
			}
		}

		/// <summary>
		/// Reset component.
		/// </summary>
		public void ResetInputHandling()
		{
			m_MainCircleSlice = null;
			if(MainCircleSlice != null) {
				SetHighlighted(false, true);
				SetPrioritized(false);
				if (Application.isPlaying)
				{
					SetupMouseListening();
				}
			}
		}

		/// <summary>
		/// Needs to be called once to be able to react to pointer events.
		/// </summary>
		public void SetupMouseListening()
		{
			EventTrigger trigger = MainCircleSlice.gameObject.GetComponent<EventTrigger>();
			if (trigger == null)
			{
				trigger = MainCircleSlice.gameObject.AddComponent<EventTrigger>();
			}
			trigger.triggers.Clear();

			EventTrigger.Entry enterEntry = new EventTrigger.Entry();
			enterEntry.eventID = EventTriggerType.PointerEnter;
			enterEntry.callback.AddListener(MouseEnter);
			trigger.triggers.Add(enterEntry);

			EventTrigger.Entry exitEntry = new EventTrigger.Entry();
			exitEntry.eventID = EventTriggerType.PointerExit;
			exitEntry.callback.AddListener(MouseExit);
			trigger.triggers.Add(exitEntry);

			EventTrigger.Entry upEntry = new EventTrigger.Entry();
			upEntry.eventID = EventTriggerType.PointerUp;
			upEntry.callback.AddListener(MouseClick);
			trigger.triggers.Add(upEntry);

			EventTrigger.Entry clickEntry = new EventTrigger.Entry();
			clickEntry.eventID = EventTriggerType.PointerClick;
			clickEntry.callback.AddListener(MouseClick);
			trigger.triggers.Add(clickEntry);
		}

		private void MouseEnter(BaseEventData tmp)
		{
			m_MouseEnterIsScheduled = true;
		}
		private void MouseExit(BaseEventData tmp) {
			m_MouseExitIsScheduled = true;
		}
		private void MouseClick(BaseEventData tmp)
		{
			m_MouseClickIsScheduled = true;
		}

		private void ExecuteMouseEnter()
		{
			if (IsPrioritized)
			{
				SetHighlighted(true, false);
			}
		}
		private void ExecuteMouseExit()
		{
			SetHighlighted(false, false);
		}
		private void ExecuteMouseClick()
		{
			Select();
		}


		/// <summary>
		/// Adds an UI Image object, will be a child to this object.
		/// </summary>
		public void AddIcon() {
			SgSelectable selectable = SgMiscUtil.CreateUiObject("IconImage", this.transform).AddComponent<SgContentSelectable>();
			selectable.colors = new SgSelectableData();
			selectable.gameObject.AddComponent<Image>();
			selectable.TargetGraphic.raycastTarget = false;
			RefreshContentSelectables();
		}
		/// <summary>
		/// Adds an UI Text object, will be a child to this object
		/// </summary>
		public void AddText()
		{
			SgSelectable selectable = SgMiscUtil.CreateUiObject("Text", this.transform).AddComponent<SgContentSelectable>();
			selectable.colors = new SgSelectableData();
			Text text = selectable.gameObject.AddComponent<Text>();
			text.raycastTarget = false;
			text.text = "Slice Text";
			text.alignment = TextAnchor.MiddleCenter;
			text.rectTransform.sizeDelta = new Vector2(200,200);
			text.fontSize = 24;
			text.raycastTarget = false;
			RefreshContentSelectables();
		}

		/// <summary>
		/// Adds a Text Mesh Pro UI object, will be a child to this object
		/// </summary>
		public void AddTextMeshPro() {
			SgSelectable selectable = SgMiscUtil.CreateUiObject("TextMeshPro", this.transform).AddComponent<SgContentSelectable>();
			selectable.colors = new SgSelectableData();
			TextMeshProUGUI text = selectable.gameObject.AddComponent<TextMeshProUGUI>();
			text.raycastTarget = false;
			text.text = "Slice Text";
			text.alignment = TextAlignmentOptions.Center;
			text.rectTransform.sizeDelta = new Vector2(200, 200);
			text.fontSize = 24;
			text.raycastTarget = false;
			RefreshContentSelectables();
		}

		/// <summary>
		/// Select/confirm this slice
		/// </summary>
		public void Select() {
			foreach (SgSelectable selectable in graphicSelectables)
			{
				selectable.SetSelected();
			}
			foreach (SgCircleSlice circleSlice in circleSlices)
			{
				circleSlice.Selectable.SetSelected();
			}
			if (sliceBorders != null)
			{
				foreach (SgSliceBorder sliceBorder in sliceBorders)
				{
					sliceBorder.Selectable.SetSelected();
				}
			}
			WeaponWheel.NotifyEvent(new SgWeaponWheelEvent(SgWeaponWheelEventType.Select, this));
		}

		/// <summary>
		/// Change highlight status
		/// </summary>
		/// <param name="highlighted">new highligt status</param>
		public void SetHighlighted(bool highlighted, bool forceImmediate) 
		{
			bool valueChange = highlighted != m_IsHighlighted;
			m_IsHighlighted = highlighted;
			if (valueChange && highlighted)
			{
				WeaponWheel.DehighlightAll(forceImmediate, this);
			}
			if (valueChange || forceImmediate)
			{
				foreach (SgSelectable selectable in graphicSelectables)
				{
					selectable.SetHighlighted(highlighted, forceImmediate);
				}
				foreach(SgCircleSlice circleSlice in circleSlices) 
				{
					circleSlice.Selectable.SetHighlighted(highlighted, forceImmediate);
				}
				if(sliceBorders != null) {
					foreach (SgSliceBorder sliceBorder in sliceBorders)
					{
						sliceBorder.Selectable.SetHighlighted(highlighted, forceImmediate);
					}
				}
			}
			if (valueChange)
			{
				WeaponWheel.NotifyEvent(new SgWeaponWheelEvent(highlighted ? SgWeaponWheelEventType.Highlight : SgWeaponWheelEventType.Dehighlight, this));
			}
		}


		/// <summary>
		/// Enable/disable pointer events
		/// </summary>
		/// <param name="prioritized"></param>
		public void SetPrioritized(bool prioritized)
		{
			MainCircleSlice.Image.raycastTarget = prioritized;
			m_IsPrioritized = prioritized;

			ExecuteScheduledMouseEvents();
		}

		private void ExecuteScheduledMouseEvents() {
			if (m_IsPrioritized)
			{
				if (m_MouseEnterIsScheduled)
				{
					ExecuteMouseEnter();
				}
				if (m_MouseExitIsScheduled)
				{
					ExecuteMouseExit();
				}
				if (m_MouseClickIsScheduled)
				{
					ExecuteMouseClick();
				}
			}
			m_MouseEnterIsScheduled = false;
			m_MouseExitIsScheduled = false;
			m_MouseClickIsScheduled = false;
		}

		/// <summary>
		/// Finds child SgSelectables and attaches them to this component
		/// </summary>
		public void RefreshContentSelectables() {
			graphicSelectables = GetComponentsInChildren<SgSelectable>();
			foreach(SgSelectable selectable in graphicSelectables) {
				selectable.Refresh(true);
			}
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(SgSliceController))]
	public class SgSliceContentInspector : Editor
	{
		private SgSliceController m_MainObject;

		protected void OnEnable()
		{
			m_MainObject = (SgSliceController)serializedObject.targetObject;
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("\nAs long as this object is mapped to the main Weapon Wheel controller, the changes here and in its child graphics will be preserved even after weapon wheel refresh/generation. You may create your own graphics (be sure to press the \"Find child graphics\" button afterwards) or use the \"Add\" buttons.\n\nPlease read the tooltips (triggered by mouse hovering) for property descriptions.\n", MessageType.Info);

			DrawDefaultInspector();

			if (GUILayout.Button("Add icon"))
			{
				m_MainObject.AddIcon();
			}
			if (GUILayout.Button("Add text"))
			{
				m_MainObject.AddText();
			}
			if(GUILayout.Button("Add Text Mesh Pro")) {
				m_MainObject.AddTextMeshPro();
			}
			if (GUILayout.Button("Find child graphics"))
			{
				m_MainObject.RefreshContentSelectables();
			}
		}
	}

#endif
}

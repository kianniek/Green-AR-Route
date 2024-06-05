using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Responsible for weapon wheel generation/refreshing and event change notifications.
	/// </summary>
	[RequireComponent(typeof(CanvasGroup))]
	public class SgWeaponWheel : MonoBehaviour
	{
		[Tooltip("Auto-refresh your changes. If turned off, you can still refresh by pressing the bottom button.")]
		public bool autoRefresh = true;
		[Tooltip("Margin/borders between slices.")]
		public float sliceBorderWidth = 35;
		[Tooltip("If your slice border is reaching in too far, it can be adjusted here.")]
		public float sliceBorderCutInnerAdjustment = -3f;
		[Tooltip("If your slice border is reaching out too far, it can be adjusted here.")]
		public float sliceBorderCutOuterAdjustment = 1f;
		[Tooltip("When cutting slice borders, affect circles too. Will make the slice borders blend in better with the circles, but can result in harder circle edges.")]
		public bool cutCirclesToo = true;
		public SgSelectableData sliceBorderColors = new SgSelectableData();
		[Tooltip("Slice content graphics (e.g. icons and text) colors. Can be overridden for each slice in the content controllers.")]
		public SgSelectableData sliceContentColors = new SgSelectableData();
		[Range(0, 360)]
		public float rotation = 0;
		[Tooltip("Controls the number of slices and contains the unique slice controllers. Double click an element to jump to it.")]
		public List<SgSliceController> sliceContents = new List<SgSliceController>();
		[Tooltip("Debug logs weapon wheel events such as highlighting and wheel visibility changes.")]
		public bool logSelectionEvents = false;
		[Tooltip("A circle may be the main content or borders, it's up to you. It's important to mark the main content as main using the checkbox to align the content graphics properly.")]
		public List<SgCircleData> circleDatas = new List<SgCircleData>();

		[Tooltip("Animated effect. Fade in weapon wheel when it's made visible")]
		public float fadeInDuration = 0;
		[Tooltip("Animated effect. Fade out weapon wheel when it's made invisible")]
		public float fadeOutDuration = 0;

		//Events
		[Tooltip("Triggered when a slice is highlighted. A SgSliceController is passed as the first parameter.")]
		public UnityEvent<SgSliceController> OnSliceHighlighted;
		[Tooltip("Triggered when a slice is dehighlighted. A SgSliceController is passed as the first parameter.")]
		public UnityEvent<SgSliceController> OnSliceDehighlighted;
		[Tooltip("Triggered when a slice is selected (pressed/confirmed). A SgSliceController is passed as the first parameter.")]
		public UnityEvent<SgSliceController> OnSliceSelected;
		[Tooltip("Triggered when the weapon wheel is becoming visible")]
		public UnityEvent OnWheelVisible;
		[Tooltip("Triggered when the weapon wheel is becoming invisible (closed)")]
		public UnityEvent OnWheelInvisible;
		[Tooltip("Triggered when any weapon wheel event is triggered")]
		public UnityEvent<SgWeaponWheelEvent> OnAnyEvent;

		//Resources
		public Material sliceBorderMaterial;
		public Material circleMaterial;
		public Material circleUncutMaterial;
		public Material circleMaskMaterial;
		public Material circleHoleMaskMaterial;
		[Tooltip("Where all generated content are put. Most child objects should be considered temporary and will be overwritten in the next refresh.")]
		public GameObject generatedObjectsContainer;
		public SgCircleSpriteData circleSprites;
		[Tooltip("Optional slice border sprite.")]
		public Sprite sliceBorderSprite;

		public int NumberOfSlices => sliceContents.Count;
		private float SliceRatio => 1f / (float)NumberOfSlices;
		private readonly List<Action<SgWeaponWheelEvent>> m_EventCallbacks = new List<Action<SgWeaponWheelEvent>>();
		private readonly List<SgSliceBorder> m_NewBorders = new List<SgSliceBorder>();
		private readonly List<SgCircleSlice> m_NewCircleSlices = new List<SgCircleSlice>();
		private bool m_IsVisible = true;
		private CanvasGroup m_CanvasGroup;
		private CanvasGroup CanvasGroup => SgMiscUtil.LazyComponent(this, ref m_CanvasGroup);
		private SgAnimationManager m_AnimationManager = new SgAnimationManager();
		public SgAnimationManager AnimationManager => m_AnimationManager;

		/// <summary>
		/// Weapon wheel visibility. Set false to hide.
		/// </summary>
		public bool IsVisible
		{
			set
			{
				if (value != m_IsVisible)
				{
					NotifyEvent(new SgWeaponWheelEvent(value ? SgWeaponWheelEventType.WheelVisible : SgWeaponWheelEventType.WheelInvisible, null));
					m_IsVisible = value;
					AnimateVisibility(value);
				}
			}
			get
			{
				return m_IsVisible;
			}
		}

		private void Awake()
		{
			CanvasGroup.alpha = 1;
		}
		private void Start()
		{
			IsVisible = generatedObjectsContainer.activeInHierarchy;
		}

		private void Update()
		{
			AnimationManager.UpdateLoop(Time.unscaledTime);
		}

		/// <summary>
		/// All added actions here will be called every time a SgWeaponWheelEvent occurs.
		/// </summary>
		/// <param name="action">action to be called when an event occus</param>
		public void AddEventCallback(Action<SgWeaponWheelEvent> action)
		{
			m_EventCallbacks.Add(action);
		}

		 /// <summary>
		 /// Remove action. Usually this should be called in the OnDestroy method in the MonoBehavior who added the action.
		 /// </summary>
		 /// <param name="action">action to be removed</param>
	   public void RemoveEventCallback(Action<SgWeaponWheelEvent> action)
		{
			m_EventCallbacks.Remove(action);
		}

		/// <summary>
		/// Set visible if invisible and vice versa
		/// </summary>
		public void ToggleVisibility() {
			bool newVisible = !IsVisible;
			IsVisible = newVisible;
		}

		private void AnimateVisibility(bool visible)
		{
			float targetAlpha = visible ? 1 : 0;
			float duration = visible ? fadeInDuration : fadeOutDuration;
			if(visible) {
				DehighlightAll(true);
				generatedObjectsContainer.SetActive(true);
			}
			AnimationManager.AnimateAlpha(this, CanvasGroup, targetAlpha, duration, AnimateVisibilityDone);
		}
		private void AnimateVisibilityDone()
		{
			bool newIsVisible = IsVisible;
			if(!newIsVisible) {
				DehighlightAll(true);
			}
			generatedObjectsContainer.SetActive(newIsVisible);
		}

		/// <summary>
		/// Unhighlights all slices.
		/// </summary>
		public void DehighlightAll(bool forceImmidiate, SgSliceController ignore = null)
		{
			foreach (SgSliceController slice in sliceContents)
			{
				if(slice != ignore) {
					slice.SetHighlighted(false, forceImmidiate);
				}
			}
		}

		/// <summary>
		/// Highlights the slices with the closest angle (compared e.g. a gamepad stick).
		/// </summary>
		/// <param name="angle">angle to compare slice angle with</param>
		public void HighlightClosest(float angle)
		{
			SgSliceController newHighlighted = null;

			float closestDiff = float.MaxValue;
			List<SgSliceController> slices = sliceContents;
			foreach (SgSliceController slice in slices)
			{
				float sliceAngle = SgMiscUtil.Angle(slice.transform.position - generatedObjectsContainer.transform.position);
				while (sliceAngle > 360)
				{
					sliceAngle -= 360;
				}

				float distance = Mathf.Abs(sliceAngle - angle);
				if (distance > 180)
				{
					distance = Mathf.Abs(distance - 360);
				}
				if (distance < closestDiff)
				{
					closestDiff = distance;
					newHighlighted = slice;
				}
			}
			foreach (SgSliceController slice in slices)
			{
				if(slice != newHighlighted) {
					slice.SetHighlighted(false, false);
				}
			}
			newHighlighted.SetHighlighted(true, false);
		}

		public void NotifyEvent(SgWeaponWheelEvent wheelEvent)
		{
			if (logSelectionEvents)
			{
				Debug.Log(this.name + ": " + wheelEvent);
			}

			switch(wheelEvent.type) {
				case SgWeaponWheelEventType.Highlight:
					OnSliceHighlighted.Invoke(wheelEvent.slice);
					break;
				case SgWeaponWheelEventType.Dehighlight:
					OnSliceDehighlighted.Invoke(wheelEvent.slice);
					break;
				case SgWeaponWheelEventType.Select:
					OnSliceSelected.Invoke(wheelEvent.slice);
					break;
				case SgWeaponWheelEventType.WheelVisible:
					OnWheelVisible.Invoke();
					break;
				case SgWeaponWheelEventType.WheelInvisible:
					OnWheelInvisible.Invoke();
					break;
			}
			OnAnyEvent.Invoke(wheelEvent);

			foreach (Action<SgWeaponWheelEvent> action in m_EventCallbacks)
			{
				action(wheelEvent);
			}
		}

		/// <summary>
		/// Confirms the highlighted slice.
		/// </summary>
		public void SelectHighlighted()
		{
			foreach (SgSliceController slice in sliceContents)
			{
				if (slice != null && slice.IsHighlighted)
				{
					slice.Select();
					return;
				}
			}
		}

		/// <summary>
		/// Intended for non-cropped overlapping slices
		/// </summary>
		/// <param name="position">current pointer position</param>
		public void PrioritizeClosest(Vector2 position)
		{
			float closestDistance = float.MaxValue;
			SgSliceController newPrioritized = null;
			List<SgSliceController> slices = sliceContents;
			foreach (SgSliceController slice in slices)
			{
				float distance = Vector2.Distance(position, slice.CenterPoint);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					newPrioritized = slice;
				}
			}
			foreach (SgSliceController slice in slices)
			{
				slice.SetPrioritized(slice == newPrioritized);
			}
		}

		/// <summary>
		/// Generates/refreshes the weapon wheel. Is intended to be called from editor, but can 
		/// occasionally be called by scripts during runtime if, for instance, new slices has 
		/// been added to sliceContents. Then the weapon wheel needs to be refreshed by calling this method.
		/// </summary>
		/// <param name="destroyOldObjectsImmediately"></param>
		/// <param name="enableLogging">true if validation errors should be logged</param>
		public void Generate(bool destroyOldObjectsImmediately, bool enableLogging) {
			long t0 = SgMiscUtil.CurrentTimeMs();

			m_NewBorders.Clear();
			m_NewCircleSlices.Clear();

			if (generatedObjectsContainer != null)
			{
				Cleanup(destroyOldObjectsImmediately);
			}

			if (!Validate(enableLogging))
			{
				return;
			}

			CreateMasks();
			for (int i = 0; i < circleDatas.Count; i++)
			{
				SgCircleData circleData = circleDatas[i];
				GameObject circle = CreateCircle(i, circleData);
				AnchorTo(circle.GetComponent<RectTransform>(), generatedObjectsContainer.transform);
			}
			MapCircleSlicesToControllers();

			generatedObjectsContainer.transform.localEulerAngles = new Vector3(0,0,-rotation);

			m_NewBorders.Clear();
			m_NewCircleSlices.Clear();

			long timeTakenMs = SgMiscUtil.CurrentTimeMs() - t0;
			if (enableLogging)
			{
				Debug.Log("Weapon wheel generated in " + timeTakenMs + " milliseconds");
			}
		}

		private bool Validate(bool enableLogging)
		{
			string message = null;
			if(NumberOfSlices <= 0) {
				message = "At least 1 slice content required";
			}
			else if (circleDatas == null || circleDatas.Count == 0)
			{
				message = "At least 1 circle required";
			}
			else if (generatedObjectsContainer == null)
			{
				message = "Generated Objects Container is required";
			}
			else if(!cutCirclesToo && circleUncutMaterial == null) {
				message = "circleUncutMaterial must be asigned";
			}

			if (enableLogging && message != null)
			{
				Debug.LogWarning(message);
			}

			return message == null;
		}

		private void CreateMasks() {
			float maxDiameter = 0;
			float minDiameter = float.MaxValue;
			SgCircleData smallestCircle = null;
			SgCircleData biggestCircle = null;
			for(int i = 0; i < circleDatas.Count; i++) {
				SgCircleData circleData = circleDatas[i];

				if(circleData.diameter > maxDiameter) {
					biggestCircle = circleData;
					maxDiameter = circleData.diameter;
				}
				if(circleData.HoleDiameter < minDiameter) {
					smallestCircle = circleData;
					minDiameter = circleData.HoleDiameter;
				}
			}
			
			if(maxDiameter > 0) {
				maxDiameter += sliceBorderCutOuterAdjustment;
				Image outerMaskImage = CreateUiObject("OuterCircleMask").AddComponent<Image>();
				outerMaskImage.raycastTarget = false;
				outerMaskImage.material = circleMaskMaterial;
				outerMaskImage.sprite = GetSpriteData(biggestCircle).circleSprite;
				outerMaskImage.rectTransform.sizeDelta = new Vector2(maxDiameter, maxDiameter);
				AnchorTo(outerMaskImage.rectTransform, generatedObjectsContainer.transform);
			}
			minDiameter += sliceBorderCutInnerAdjustment;
			if (minDiameter < float.MaxValue && minDiameter > 0) {
				Image holeMaskImage = CreateUiObject("CircleHoleMask").AddComponent<Image>();
				holeMaskImage.raycastTarget = false;
				holeMaskImage.material = circleHoleMaskMaterial;
				holeMaskImage.sprite = GetSpriteData(smallestCircle).circleSprite;
				holeMaskImage.rectTransform.sizeDelta = new Vector2(minDiameter, minDiameter);
				AnchorTo(holeMaskImage.rectTransform, generatedObjectsContainer.transform);
			}

			if(NumberOfSlices > 1) {
				for (int i = 0; i < NumberOfSlices; i++)
				{
					float radius = maxDiameter / 2f;
					SgSliceBorder border = CreateUiObject("Border" + i).AddComponent<SgSliceBorder>();
					border.gameObject.AddComponent<SgSelectable>();
					border.gameObject.AddComponent<Image>();
					border.Selectable.RectTransform.pivot = new Vector2(0.5f, 0f);
					border.Selectable.Image.sprite = sliceBorderSprite;
					border.Selectable.Image.raycastTarget = false;
					border.Selectable.Image.material = sliceBorderMaterial;
					border.Selectable.RectTransform.sizeDelta = new Vector2(sliceBorderWidth, radius);
					float borderRotation = 360 * (SliceRatio * i);
					border.Selectable.Image.rectTransform.localEulerAngles = new Vector3(0, 0, -borderRotation);
					sliceBorderColors.PreventZeroAlpha();
					border.Selectable.colors = sliceBorderColors;
					border.Selectable.Refresh(true);
					m_NewBorders.Add(border);
					AnchorTo(border.Selectable.RectTransform, generatedObjectsContainer.transform);
				}
			}
		}

		private void MapCircleSlicesToControllers()
		{
			List<SgCircleSlice>[] circleSlicesBySliceIndex = new List<SgCircleSlice>[NumberOfSlices];
			foreach (SgCircleSlice circleSlice in m_NewCircleSlices)
			{
				if (circleSlice.sliceIndex >= NumberOfSlices)
				{
					continue;
				}
				if (circleSlicesBySliceIndex[circleSlice.sliceIndex] == null)
				{
					circleSlicesBySliceIndex[circleSlice.sliceIndex] = new List<SgCircleSlice>();
				}
				circleSlicesBySliceIndex[circleSlice.sliceIndex].Add(circleSlice);
			}

			if(m_NewBorders.Count > 0) {
				for (int i = 0; i < sliceContents.Count; i++)
				{
					SgSliceBorder[] attachedBorders = new SgSliceBorder[2];
					attachedBorders[0] = m_NewBorders[i];
					bool isLast = i == sliceContents.Count - 1;
					if(isLast) {
						attachedBorders[1] = m_NewBorders[0];
					} else {
						attachedBorders[1] = m_NewBorders[i + 1];
					}
					sliceContents[i].sliceBorders = attachedBorders;
				}
			}

			foreach (SgSliceController content in sliceContents)
			{
				content.circleSlices = circleSlicesBySliceIndex[content.sliceIndex].ToArray();
				content.ResetInputHandling();
			}

		}

		private GameObject CreateCircle(int index, SgCircleData circleData) {

			float scale = circleData.Scale;
			float offset = circleData.Offset;

			GameObject circleContainer = CreateUiObject("CircleContainer" + index);

			Material material = new Material(cutCirclesToo ? circleMaterial : circleUncutMaterial);
			
			material.SetTexture("_BlendTex", GetSpriteData(circleData).holeTexture);
			material.SetTextureScale("_BlendTex", new Vector2(scale, scale));
			material.SetTextureOffset("_BlendTex", new Vector2(offset, offset));
			
			if(circleData.colors.GetFillTexture() != null) {
				material.SetTexture("_BlendTex2", circleData.colors.GetFillTexture());
				material.SetTextureScale("_BlendTex2", new Vector2(circleData.colors.textureScale, circleData.colors.textureScale));
				material.SetTextureOffset("_BlendTex2", new Vector2(circleData.colors.textureOffset, circleData.colors.textureOffset));
			} else {
				material.SetTexture("_BlendTex2", null);
			}
			material.name = "GeneratedCircleMaterial" + index;

			float sliceRatio = SliceRatio;
			for (int i = 0; i < NumberOfSlices; i++)
			{

				SgCircleSlice circleSlice = CreateUiObject("CircleSlice" + index + "-" + i).AddComponent<SgCircleSlice>();
				circleSlice.gameObject.AddComponent<SgSelectable>();
				circleSlice.gameObject.AddComponent<SgFillableCircleImage>();
				circleSlice.RectTransform.sizeDelta = new Vector2(circleData.diameter, circleData.diameter);
				circleSlice.Image.type = Image.Type.Filled;
				circleSlice.Image.raycastTarget = false;
				circleSlice.Image.fillOrigin = 2;
				circleSlice.Image.sprite = GetSpriteData(circleData).circleSprite;
				circleSlice.Image.material = material;
				circleSlice.Image.fillAmount = sliceRatio;
				circleSlice.circleData = circleData;
				circleSlice.sliceIndex = i;
				m_NewCircleSlices.Add(circleSlice);

				SgSelectableData selectableData = circleData.colors;
				SgSelectableData overridingData = circleData.GetOverridingSelectableData(i);
				if(overridingData != null) {
					selectableData = overridingData;
				}
				circleSlice.Selectable.colors = selectableData;
				circleSlice.Selectable.Refresh(true);

				float circleRotation = 360 * (sliceRatio * i);
				circleSlice.RectTransform.localEulerAngles = new Vector3(0,0,-circleRotation);
				circleSlice.Image.uvRotation = -circleRotation + circleData.colors.textureRotation;


				AnchorTo(circleSlice.RectTransform, circleContainer.transform);

				if (circleData.isMain && sliceContents.Count > 0)
				{
					if (sliceContents[i] == null || (i > 0 && (sliceContents[i] == sliceContents[i - 1])))
					{
						SgSliceController newContent = CreateUiObject("SliceController").AddComponent<SgSliceController>();
						sliceContents[i] = newContent;
					}
					SgSliceController content = sliceContents[i];
					content.gameObject.name = "SliceController" + i;
					content.sliceIndex = i;

					float startRatio = (float)i / (float)NumberOfSlices;
					float nextRatio = ((float)i + 1f) / (float)NumberOfSlices;
					float ratio = nextRatio - ((nextRatio - startRatio) / 2f);

					Vector2 centerPoint = SgMiscUtil.GetPointInCircle(Vector2.zero, circleData.CenterRadius, ratio);
					AnchorTo(content.RectTransform, generatedObjectsContainer.transform);
					content.RectTransform.anchoredPosition = centerPoint;
					content.RectTransform.SetParent(circleSlice.transform, true);
					content.RectTransform.localScale = Vector3.one;
					content.RectTransform.localEulerAngles = new Vector3(0, 0, circleRotation + rotation);
					content.RefreshContentSelectables();

					RectTransform centerPointObject = CreateUiObject("CenterPoint" + i).GetComponent<RectTransform>();
					centerPointObject.position = content.transform.position;
					content.centerPoint = centerPointObject;
				}
			}



			return circleContainer;
		}

		private GameObject CreateUiObject(string name)
		{
			Transform defaultParent = generatedObjectsContainer.transform;
			GameObject uiObject = SgMiscUtil.CreateUiObject(name, defaultParent);
			return uiObject;
		}

		private static void AnchorTo(RectTransform rectTransform, Transform parent) {
			SgMiscUtil.AnchorTo(rectTransform, parent);
		}

		private SgCircleSpriteData GetSpriteData(SgCircleData circleData) {
			SgCircleSpriteData spriteData = circleData.overridingSprites;
			if(spriteData.circleSprite == null) {
				spriteData.circleSprite = circleSprites.circleSprite;
			}
			if(spriteData.holeTexture == null) {
				spriteData.holeTexture = circleSprites.holeTexture;
			}
			return spriteData;
		}

		private void Cleanup(bool destroyOldObjectsImmediately) {
			GameObject container = generatedObjectsContainer;
			if (container == null)
			{
				return;
			}

			List<GameObject> willDestroyObjects = new List<GameObject>();
			foreach (Component childComponent in container.GetComponentsInChildren<Transform>())
			{
				GameObject child = childComponent.gameObject;
				if (child == container)
				{
					continue;
				}

				willDestroyObjects.Add(child);
			}

			if(sliceContents != null) {
				foreach (SgSliceController content in sliceContents)
				{
					if(content != null) {
						willDestroyObjects.Remove(content.gameObject);
						foreach(Component childComponent in content.GetComponentInChildren<Transform>()) {
							willDestroyObjects.Remove(childComponent.gameObject);
						}

						AnchorTo(content.RectTransform, generatedObjectsContainer.transform);
					}
				}
			}
			

			for (int i = 0; i < willDestroyObjects.Count; i++)
			{
				GameObject gameObject = willDestroyObjects[i];
				if (destroyOldObjectsImmediately)
				{
					GameObject.DestroyImmediate(gameObject);
				}
				else
				{
					GameObject.Destroy(gameObject);
				}
			}
			
		}
	}


#if UNITY_EDITOR
	[CustomEditor(typeof(SgWeaponWheel))]
	public class SgWeaponWheelInspector : Editor
	{
		private SgWeaponWheel m_MainObject;

		private SgFoldoutSection m_GeneralSettingsSection;
		private SgFoldoutSection m_SlicesSection;
		private SgFoldoutSection m_CirclesSection;
		private SgFoldoutSection m_SliceBordersSection;
		private SgFoldoutSection m_ResoursesSection;
		private SgFoldoutSection m_EventsSection;
		private SgFoldoutSection[] m_FoldoutSections;

		private readonly int defaultSpace = 20;

		private readonly Dictionary<string, bool> cachedFoldoutStatuses = new Dictionary<string, bool>();
		private GUIStyle m_SmallButtonStyle;
		private GUIStyle SmallButtonStyle
		{
			get
			{
				if (m_SmallButtonStyle == null)
				{
					m_SmallButtonStyle = new GUIStyle(EditorStyles.miniButton);
					m_SmallButtonStyle.fixedWidth = 150;
					m_SmallButtonStyle.margin = new RectOffset(0, 0, 0, 0);
				}
				return m_SmallButtonStyle;
			}
		}
		private GUIStyle m_FoldoutButtonStyle;
		private GUIStyle FoldoutButtonStyle
		{
			get
			{
				if (m_FoldoutButtonStyle == null)
				{
					m_FoldoutButtonStyle = new GUIStyle(EditorStyles.miniButton);
					m_FoldoutButtonStyle.fixedWidth = 150;
					m_FoldoutButtonStyle.margin = new RectOffset(35, 0, 0, 0);
				}
				return m_FoldoutButtonStyle;
			}
		}

		protected void OnEnable()
		{
			m_MainObject = (SgWeaponWheel)serializedObject.targetObject;

			m_GeneralSettingsSection = new SgGeneralSettingsSection(m_MainObject, serializedObject);
			m_SlicesSection = new SgSlicesSection(m_MainObject, serializedObject);
			m_CirclesSection = new SgCirclesSection(m_MainObject, serializedObject, this);
			m_SliceBordersSection = new SgSliceBordersSection(m_MainObject, serializedObject);
			m_EventsSection = new SgEventsSection(m_MainObject, serializedObject);
			m_ResoursesSection = new SgResourcesSection(m_MainObject, serializedObject);

			m_FoldoutSections = new SgFoldoutSection[] { 
				m_GeneralSettingsSection, m_SlicesSection, m_CirclesSection, m_SliceBordersSection, m_EventsSection, m_ResoursesSection 
			};
		}

		

		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("\nStart building your weapon wheel by adding circles and adding slices by adding slice contents. Note that you control all weapon wheel generation with these settings, all changes you make inside the generated objects container will be overwritten. There's just one exception, the slice content controllers and its children. There you add graphics such as text and icons to your slices. Just double click the content elements in the list below to jump to these objects.\n\nIf you want to make very specific changes to your wheel inside the generated objects controller, besides the slice content controllers, you may do so by first deactivating auto-refresh. Just be aware that all your changes will be overwritten if you refresh later.\n\nDon't forget that you can change the alpha value of any color to add/reduce transparency.\n\nMost settings have tooltips. Hold your mouse over the labels to see them. If you have any questions or suggestions, send me an e-mail.\n", MessageType.Info);
			EditorGUILayout.Space(defaultSpace);

			int hash1 = OverrideDataHash();
			int circlesCount1 = m_MainObject.circleDatas.Count;
			int slicesCount1 = m_MainObject.sliceContents.Count;
			bool[] isMains1 = ExtractIsMains();

			//Fold out sections
			foreach (SgFoldoutSection section in m_FoldoutSections)
			{
				DrawFoldOut(section);
			}

			//Check if any property has changed
			bool hasChanged = serializedObject.hasModifiedProperties;
			serializedObject.ApplyModifiedProperties();
			int circlesCount2 = m_MainObject.circleDatas.Count;
			int slicesCount2 = m_MainObject.sliceContents.Count;
			int hash2 = OverrideDataHash();
			hasChanged = hasChanged || slicesCount1 != slicesCount2 || circlesCount1 != circlesCount2 || hash1 != hash2;

			bool[] isMains2 = ExtractIsMains();
			bool hasAnyMain = false;
			for (int i = 0; i < isMains2.Length; i++)
			{
				if (isMains2[i])
				{
					hasAnyMain = true;
				}
				if (isMains2[i] && !isMains1[i])
				{
					hasChanged = true;
					foreach (SgCircleData circleData in m_MainObject.circleDatas)
					{
						circleData.isMain = false;
					}
					m_MainObject.circleDatas[i].isMain = true;
					break;
				}
			}
			if (!hasAnyMain && m_MainObject.circleDatas.Count > 0)
			{
				m_MainObject.circleDatas[0].isMain = true;
				hasChanged = true;
			}

			//Check if the weapon wheel should be re-generated
			bool buttonPressed = GUILayout.Button("Force refresh");
			if (buttonPressed || (m_MainObject.autoRefresh && hasChanged))
			{
				m_MainObject.Generate(true, buttonPressed);
				EditorUtility.SetDirty(m_MainObject);
			}
		}

		private void DrawFoldOut(SgFoldoutSection foldout) {
			GUIStyle style = new GUIStyle(EditorStyles.foldout);
			style.fontStyle = FontStyle.Bold;
			style.fontSize = Mathf.RoundToInt(style.fontSize * 1.5f);
			bool isFoldedOut = cachedFoldoutStatuses.ContainsKey(foldout.title) && cachedFoldoutStatuses[foldout.title];
			cachedFoldoutStatuses[foldout.title] = isFoldedOut = EditorGUILayout.Foldout(isFoldedOut, " " + foldout.title, true, style);

			if(isFoldedOut) {
				EditorGUILayout.HelpBox(foldout.description, MessageType.Info);
				foldout.Validate();
				foreach(string warning in foldout.validationWarnings) {
					EditorGUILayout.HelpBox(warning, MessageType.Warning);
				}

				foldout.DrawProperties();
			}
			EditorGUILayout.Space(defaultSpace);

		}

		private int OverrideDataHash()
		{
			int h = 0;

			h += SelectableHash(m_MainObject.sliceBorderColors);
			h += SelectableHash(m_MainObject.sliceContentColors);
			foreach (SgCircleData circleData in m_MainObject.circleDatas)
			{
				h += SelectableHash(circleData.colors);
				if (circleData.overridingSliceColors != null)
				{
					foreach (SgOverridingSliceSelectableData selectable in circleData.overridingSliceColors)
					{
						h += selectable.sliceIndex;
						h += SelectableHash(selectable.colors);
					}
				}
			}
			return h;
		}

		private int SelectableHash(SgSelectableData selectableData)
		{
			int h = 0;
			h += selectableData.GetHashCode();
			h += selectableData.color.GetHashCode();
			h += selectableData.highlightColor.GetHashCode();

			if (selectableData.highlightAnimations != null)
			{
				foreach (SgHighlightAnimationData animationData in selectableData.highlightAnimations)
				{
					h += animationData.GetHashCode();
					h += animationData.color.GetHashCode();
					h += animationData.dehighligtDuration.GetHashCode();
					h += animationData.duration.GetHashCode();
					h += animationData.animateColor.GetHashCode();
					h += animationData.animateScale.GetHashCode();
					h += animationData.animateSize.GetHashCode();
				}
			}
			if (selectableData.selectAnimations != null)
			{
				foreach (SgAnimationData animationData in selectableData.selectAnimations)
				{
					h += animationData.GetHashCode();
					h += animationData.color.GetHashCode();
					h += animationData.duration.GetHashCode();
					h += animationData.animateColor.GetHashCode();
					h += animationData.animateScale.GetHashCode();
					h += animationData.animateSize.GetHashCode();
				}
			}

			return h;
		}

		private bool[] ExtractIsMains()
		{
			bool[] isMains = new bool[m_MainObject.circleDatas.Count];
			for (int i = 0; i < m_MainObject.circleDatas.Count; i++)
			{
				isMains[i] = m_MainObject.circleDatas[i].isMain;
			}
			return isMains;
		}

		private abstract class SgFoldoutSection {
			public SgWeaponWheel weaponWheel;
			public SerializedObject serializedObject;
			public SerializedProperty[] properties;
			public string description;
			public string title;
			public readonly List<string> validationWarnings = new List<string>();

			public void Init(SgWeaponWheel weaponWheel, string title, string description, SerializedProperty[] properties) {
				this.weaponWheel = weaponWheel;
				this.title = title;
				this.description = description;
				this.properties = properties;
			}

			public static SerializedProperty[] FindProperties(SerializedObject serializedObject, params string[] propertyNames)
			{
				SerializedProperty[] properties = new SerializedProperty[propertyNames.Length];
				for (int i = 0; i < propertyNames.Length; i++)
				{
					properties[i] = serializedObject.FindProperty(propertyNames[i]);
				}
				return properties;
			}

			public virtual void DrawProperties() {
				foreach(SerializedProperty property in properties) {
					EditorGUILayout.PropertyField(property);
				}
			}

			public abstract void Validate();
		}



		private class SgGeneralSettingsSection : SgFoldoutSection
		{
			public SgGeneralSettingsSection(SgWeaponWheel weaponWheel, SerializedObject serializedObject) {
				base.Init(weaponWheel, "General settings", "General weapon wheel settings",
					FindProperties(serializedObject, "autoRefresh", "fadeInDuration", "fadeOutDuration", "rotation", "logSelectionEvents"));
			}

			public override void Validate()
			{
				validationWarnings.Clear();

				if (weaponWheel.fadeInDuration < 0 || weaponWheel.fadeOutDuration < 0)
				{
					validationWarnings.Add("Fade in/out cannot be less than zero");
				}
			}
		}

		private class SgCirclesSection : SgFoldoutSection
		{
			private readonly SgWeaponWheel m_WeaponWheel;
			private readonly SgWeaponWheelInspector m_Inspector;
			private readonly SerializedProperty m_CircleDatasProperty;
			public SgCirclesSection(SgWeaponWheel weaponWheel, SerializedObject serializedObject, SgWeaponWheelInspector inspector)
			{
				m_Inspector = inspector;
				m_WeaponWheel = weaponWheel;
				SerializedProperty[] properties = FindProperties(serializedObject, "circleDatas");
				base.Init(weaponWheel, "Circles", "You need one main circle which icons/text will be attached to. If you want outer/inner borders you can add more circles.", properties);
				m_CircleDatasProperty = properties[0];
			}

			public override void Validate()
			{
				validationWarnings.Clear();
			}

			public override void DrawProperties()
			{
				if (GUILayout.Button("Add circle", m_Inspector.SmallButtonStyle))
				{
					m_WeaponWheel.circleDatas.Add(new SgCircleData());
				}

				int moveUpIndex = -1;
				int moveDownIndex = -1;
				int removeIndex = -1;
				int i1 = 0;
				var enumerator = m_CircleDatasProperty.GetEnumerator();
				while (enumerator.MoveNext())
				{
					SgCircleData circleData = m_WeaponWheel.circleDatas[i1];

					SerializedProperty elementProperty = enumerator.Current as SerializedProperty;
					if (elementProperty == null)
					{
						continue;
					}

					circleData.isFoldedOut = EditorGUILayout.Foldout(circleData.isFoldedOut, "Circle" + i1, true);
					EditorGUI.indentLevel = 1;

					if (circleData.isFoldedOut)
					{
						var innerEnumerator = elementProperty.GetEnumerator();
						while (innerEnumerator.MoveNext())
						{
							SerializedProperty innerProperty = innerEnumerator.Current as SerializedProperty;



							if (innerProperty.depth >= 3)
							{
								//Ignore
							}
							else if (innerProperty.name == "thickness")
							{
								float min = 0.01f;
								float max = circleData.diameter;
								float value = innerProperty.floatValue;

								innerProperty.floatValue = EditorGUILayout.Slider("Thickness", value, min, max);
							} 
							else
							{
								EditorGUILayout.PropertyField(innerProperty);
							}
						}

						EditorGUILayout.Space(5);
						GUILayout.Label("Circle control buttons");
						if (GUILayout.Button("Move up", m_Inspector.FoldoutButtonStyle))
						{
							moveUpIndex = i1;
						}
						if (GUILayout.Button("Move down", m_Inspector.FoldoutButtonStyle))
						{
							moveDownIndex = i1;
						}
						if (GUILayout.Button("Remove", m_Inspector.FoldoutButtonStyle))
						{
							removeIndex = i1;
						}
						EditorGUILayout.Space(25);
					}
					EditorGUI.indentLevel = 0;
					i1++;
				}
				if (moveUpIndex > 0)
				{
					m_CircleDatasProperty.MoveArrayElement(moveUpIndex, moveUpIndex - 1);
				}
				else if (moveDownIndex >= 0)
				{
					m_CircleDatasProperty.MoveArrayElement(moveDownIndex, moveDownIndex + 1);
				}
				else if (removeIndex >= 0)
				{
					m_CircleDatasProperty.DeleteArrayElementAtIndex(removeIndex);
				}
			}
		}

		private class SgSlicesSection : SgFoldoutSection
		{
			public SgSlicesSection(SgWeaponWheel weaponWheel, SerializedObject serializedObject)
			{
				base.Init(weaponWheel, "Slices", "Control the number of slices. Press the + button to increase the number of slices and the - button to decrease them. Double click one of the entries to jump to the slice controller, where you add a icon/text to that specific slice.",
					FindProperties(serializedObject, "sliceContents", "sliceContentColors"));
			}

			public override void Validate()
			{
				validationWarnings.Clear();

				if (weaponWheel.NumberOfSlices <= 0)
				{
					validationWarnings.Add("You need add least one slice");
				}
			}
		}

		private class SgSliceBordersSection : SgFoldoutSection
		{
			public SgSliceBordersSection(SgWeaponWheel weaponWheel, SerializedObject serializedObject)
			{
				base.Init(weaponWheel, "Slice borders", "Control the borders between slices",
					FindProperties(serializedObject, "sliceBorderWidth", "sliceBorderColors", "sliceBorderCutInnerAdjustment", "sliceBorderCutOuterAdjustment", "cutCirclesToo"));
			}

			public override void Validate()
			{
				validationWarnings.Clear();
			}
		}

		private class SgResourcesSection : SgFoldoutSection
		{
			public SgResourcesSection(SgWeaponWheel weaponWheel, SerializedObject serializedObject)
			{
				base.Init(weaponWheel, "Resources", "The resources the weapon wheel uses when generating the weapon wheel. An average user doesn't need to change these, these are more advanced settings.",
					FindProperties(serializedObject, "sliceBorderMaterial", "circleMaterial", "circleUncutMaterial", "circleMaskMaterial",
						"circleHoleMaskMaterial", "generatedObjectsContainer", "circleSprites", "sliceBorderSprite"));
			}

			public override void Validate()
			{
				validationWarnings.Clear();

				if (weaponWheel.fadeInDuration < 0 || weaponWheel.fadeOutDuration < 0)
				{
					validationWarnings.Add("Fade in/out cannot be less than zero");
				}
			}
		}

		private class SgEventsSection : SgFoldoutSection
		{
			public SgEventsSection(SgWeaponWheel weaponWheel, SerializedObject serializedObject)
			{
				base.Init(
					weaponWheel,
					"Events", 
					"You can assign event callbacks here. If you prefer to do it completely with code it's possible as well by calling AddEventCallback() instead.",
					FindProperties(
						serializedObject,
						"OnSliceHighlighted",
						"OnSliceDehighlighted",
						"OnSliceSelected",
						"OnWheelVisible",
						"OnWheelInvisible",
						"OnAnyEvent"
					)
				);
			}

			public override void Validate()
			{
				validationWarnings.Clear();
			}
		}
	}

#endif
}

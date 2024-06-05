using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Tween animation util methods
	/// </summary>
	public class SgAnimationManager
	{
		private readonly List<SgAnimationTaskI> m_ScheduledTasks = new List<SgAnimationTaskI>();
		private readonly List<SgTaskKey> m_CleanupKeys = new List<SgTaskKey>();
		private readonly Dictionary<SgTaskKey, SgAnimationTaskI> m_CurrentTasks = new Dictionary<SgTaskKey, SgAnimationTaskI>();
		private enum SgTaskType { alpha, color, scale }
		private float m_CurrentTime;

		/// <summary>
		/// Animation dictionary key
		/// </summary>
		private struct SgTaskKey {
			public readonly SgTaskType type;
			public readonly MonoBehaviour owner;

			public SgTaskKey(MonoBehaviour owner, SgTaskType type) {
				this.owner = owner;
				this.type = type;
			}
		}

		/// <summary>
		/// General animation data
		/// </summary>
		private struct SgTaskData
		{
			public readonly Action doneCallback;
			public readonly MonoBehaviour owner;
			public readonly float duration;
			public readonly float timeStarted;
			public readonly SgTaskKey key;

			public SgTaskData(MonoBehaviour owner, float duration, Action doneCallback, SgTaskType type, float timeStarted)
			{
				this.doneCallback = doneCallback;
				this.owner = owner;
				this.duration = duration;
				this.timeStarted = timeStarted;
				this.key = new SgTaskKey(owner, type);
			}

			public void Done()
			{
				doneCallback?.Invoke();
			}
		}

		private interface SgAnimationTaskI
		{
			public SgTaskData GetTaskData();
			public void Update(float timeElapsed);
			public void Done();
		}

		/// <summary>
		/// Alpha animation data
		/// </summary>
		private struct SgAlphaTask : SgAnimationTaskI
		{
			public readonly SgTaskData taskData;
			public readonly CanvasGroup canvasGroup;
			public readonly float startAlpha;
			public readonly float targetAlpha;

			public SgTaskData GetTaskData() => taskData;

			public SgAlphaTask(MonoBehaviour owner, CanvasGroup canvasGroup, float targetAlpha, float duration, Action doneCallback, float timeStarted)
			{
				taskData = new SgTaskData(owner, duration, doneCallback, SgTaskType.alpha, timeStarted);
				this.canvasGroup = canvasGroup;
				this.startAlpha = canvasGroup.alpha;
				this.targetAlpha = targetAlpha;
			}

			public void Update(float timeElapsed)
			{
				canvasGroup.alpha = Mathf.SmoothStep(startAlpha, targetAlpha, timeElapsed / taskData.duration);
			}
			public void Done()
			{
				canvasGroup.alpha = targetAlpha;
				taskData.Done();
			}
		}

		/// <summary>
		/// Color animation data
		/// </summary>
		private struct SgColorTask : SgAnimationTaskI
		{
			public readonly SgTaskData taskData;
			public readonly Graphic uiGraphic;
			public readonly Color startColor;
			public readonly Color targetColor;

			public SgTaskData GetTaskData() => taskData;

			public SgColorTask(Graphic uiGraphic, Color targetColor, float duration, Action doneCallback, float timeStarted)
			{
				taskData = new SgTaskData(uiGraphic, duration, doneCallback, SgTaskType.color, timeStarted);
				this.uiGraphic = uiGraphic;
				this.startColor = uiGraphic.color;
				this.targetColor = targetColor;
			}

			public void Update(float timeElapsed)
			{
				uiGraphic.color = Color.Lerp(startColor, targetColor, timeElapsed / taskData.duration);
			}
			public void Done()
			{
				uiGraphic.color = targetColor;
				taskData.Done();
			}
		}

		/// <summary>
		/// Scale animation data
		/// </summary>
		private struct SgScaleTask : SgAnimationTaskI
		{
			public readonly SgTaskData taskData;
			public readonly Vector2 startScale;
			public readonly Vector2 targetScale;

			public SgTaskData GetTaskData() => taskData;

			public SgScaleTask(MonoBehaviour target, Vector2 targetScale, float duration, Action doneCallback, float timeStarted)
			{
				taskData = new SgTaskData(target, duration, doneCallback, SgTaskType.alpha, timeStarted);
				this.startScale = target.transform.localScale;
				this.targetScale = targetScale;
			}

			public void Update(float timeElapsed)
			{
				Vector2 currentScale = new Vector2();
				for (int i = 0; i < 2; i++)
				{
					currentScale[i] = Mathf.SmoothStep(startScale[i], targetScale[i], timeElapsed / taskData.duration);
				}
				taskData.owner.transform.localScale = currentScale;
			}
			public void Done()
			{
				taskData.owner.transform.localScale = targetScale;
				taskData.Done();
			}
		}

		/// <summary>
		/// Scale size delta animation data
		/// </summary>
		private struct SgSizeTask : SgAnimationTaskI
		{
			public readonly SgTaskData taskData;
			public readonly RectTransform targetTransform;
			public readonly Vector2 startSize;
			public readonly Vector2 targetSize;

			public SgTaskData GetTaskData() => taskData;

			public SgSizeTask(MonoBehaviour target, RectTransform targetTransform, Vector2 defaultSize, Vector2 targetScale, float duration, Action doneCallback, float timeStarted)
			{
				taskData = new SgTaskData(target, duration, doneCallback, SgTaskType.alpha, timeStarted);
				this.targetTransform = targetTransform;
				this.startSize = targetTransform.sizeDelta;
				this.targetSize = new Vector2(defaultSize.x * targetScale.x, defaultSize.y * targetScale.y);				
			}

			public void Update(float timeElapsed)
			{
				Vector2 currentSize = new Vector2();
				for(int i = 0; i < 2; i++) {
					currentSize[i] = Mathf.SmoothStep(startSize[i], targetSize[i], timeElapsed / taskData.duration);
				}
				targetTransform.sizeDelta = currentSize;

			}
			public void Done()
			{
				targetTransform.sizeDelta = targetSize;
				taskData.Done();
			}
		}

		/// <summary>
		/// Call this from a MonoBehavoir's Update loop to update scheduled animations.
		/// </summary>
		/// <param name="deltaTime"></param>
		public void UpdateLoop(float time)
		{
			m_CurrentTime = time;
			if(m_ScheduledTasks.Count == 0 && m_CurrentTasks.Count == 0) {
				return;
			}

			while(m_ScheduledTasks.Count > 0) {
				SgAnimationTaskI task = m_ScheduledTasks[0];
				m_ScheduledTasks.RemoveAt(0);
				m_CurrentTasks[task.GetTaskData().key] = task;
			}

			foreach(KeyValuePair<SgTaskKey,SgAnimationTaskI> keyValue in m_CurrentTasks) {
				SgAnimationTaskI task = keyValue.Value;
				float timeElapsed = time - task.GetTaskData().timeStarted;
				bool isDone = timeElapsed >= task.GetTaskData().duration;
				bool isInProgress = !isDone;

				if (isInProgress)
				{
					task.Update(timeElapsed);
				} else if(isDone) {
					task.Done();
					m_CleanupKeys.Add(keyValue.Key);
				}
			}

			foreach(SgTaskKey key in m_CleanupKeys) {
				m_CurrentTasks.Remove(key);
			}
			m_CleanupKeys.Clear();
		}

		/// <summary>
		/// Animate a canvas group to a target alpha
		/// </summary>
		/// <param name="owner">the mono behavior which will run the coroutine</param>
		/// <param name="canvasGroup">canvas group to change alpha on</param>
		/// <param name="targetAlpha"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public void AnimateAlpha(MonoBehaviour owner, CanvasGroup canvasGroup, float targetAlpha, float duration, Action doneCallback)
		{
			TaskStart(new SgAlphaTask(owner, canvasGroup, targetAlpha, duration, doneCallback, m_CurrentTime));
		}

		/// <summary>
		/// Animate color transition
		/// </summary>
		/// <param name="uiGraphic">the UI graphic to change color on</param>
		/// <param name="targetColor"></param>
		/// <param name="duration"></param>
		/// <param name="doneCallback"></param>
		public void AnimateColor(Graphic uiGraphic, Color targetColor, float duration, Action doneCallback)
		{
			TaskStart(new SgColorTask(uiGraphic, targetColor, duration, doneCallback, m_CurrentTime));
		}

		/// <summary>
		/// Animate a target's scale
		/// </summary>
		/// <param name="target">the mono behavior which transform will change scale</param>
		/// <param name="targetScale"></param>
		/// <param name="duration"></param>
		/// <param name="doneCallback"></param>
		public void AnimateScale(MonoBehaviour target, Vector2 targetScale, float duration, Action doneCallback)
		{
			TaskStart(new SgScaleTask(target, targetScale, duration, doneCallback, m_CurrentTime));
		}

		/// <summary>
		/// Animate a rect transform's size
		/// </summary>
		/// <param name="target"></param>
		/// <param name="targetTransform"></param>
		/// <param name="defaultSize"></param>
		/// <param name="targetScale"></param>
		/// <param name="duration"></param>
		/// <param name="doneCallback"></param>
		public void AnimateSize(MonoBehaviour target, RectTransform targetTransform, Vector2 defaultSize, Vector2 targetScale, float duration, Action doneCallback)
		{
			TaskStart(new SgSizeTask(target, targetTransform, defaultSize, targetScale, duration, doneCallback, m_CurrentTime));
		}

		/// <summary>
		/// Make sure a task will run
		/// </summary>
		/// <param name="task"></param>
		private void TaskStart(SgAnimationTaskI task)
		{
			if (Application.isPlaying && m_CurrentTime > 0)
			{
				m_ScheduledTasks.Add(task);
			}
			else
			{
				task.Done();
			}
		}
	}
}

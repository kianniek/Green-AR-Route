using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Various helper methods.
	/// </summary>
	public static class SgMiscUtil
	{
		/// <summary>
		/// Anchor a RectTransform to a transform.
		/// </summary>
		/// <param name="rectTransform"></param>
		/// <param name="transform"></param>
		public static void AnchorTo(RectTransform rectTransform, Transform transform)
		{
			rectTransform.SetParent(transform);
			rectTransform.localScale = Vector3.one;
			rectTransform.anchoredPosition = Vector2.zero;
		}

		/// <summary>
		/// Angle from zero.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static float Angle(float x, float y)
		{
			if (x < 0)
			{
				return 360 - (Mathf.Atan2(x, y) * Mathf.Rad2Deg * -1);
			}
			else
			{
				return Mathf.Atan2(x, y) * Mathf.Rad2Deg;
			}
		}
		/// <summary>
		/// Angle from zero.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public static float Angle(Vector2 point)
		{
			return Angle(point.x, point.y);
		}

		/// <summary>
		/// Destroys objects who are children to the passed game object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="container">Parent</param>
		/// <param name="calledFromEditor">if true, destroy immediately</param>
		public static void CleanupComponentObjects<T>(GameObject container, bool calledFromEditor) where T : Component
		{
			if (container == null)
			{
				return;
			}

			List<GameObject> willDestroyObjects = new List<GameObject>();
			foreach (Component childComponent in container.GetComponentsInChildren<T>())
			{
				GameObject child = childComponent.gameObject;
				if (child == container)
				{
					continue;
				}

				willDestroyObjects.Add(child);
			}

			for (int i = 0; i < willDestroyObjects.Count; i++)
			{
				GameObject gameObject = willDestroyObjects[i];
				if (calledFromEditor)
				{
					GameObject.DestroyImmediate(gameObject);
				}
				else
				{
					GameObject.Destroy(gameObject);
				}
			}
		}

		/// <summary>
		/// Create a game object with a Rect Transform
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultParent"></param>
		/// <returns></returns>
		public static GameObject CreateUiObject(string name, Transform defaultParent)
		{
			GameObject gameObject = new GameObject(name);
			RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
			AnchorTo(rectTransform, defaultParent);
			gameObject.layer = 5;
			return gameObject;
		}

		/// <summary>
		/// Current time in milliseconds
		/// </summary>
		/// <returns></returns>
		public static long CurrentTimeMs()
		{
			return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		}

		/// <summary>
		/// Point in circle, starting from top center.
		/// </summary>
		/// <param name="center">Circle center</param>
		/// <param name="radius">Circle radius</param>
		/// <param name="ratio">Progress between 0-1</param>
		/// <returns></returns>
		public static Vector2 GetPointInCircle(Vector2 center, float radius, float ratio)
		{
			float startRatio = -0.25f;
			ratio = startRatio + ratio;

			float currentRadian = ratio * Mathf.PI * 2;
			currentRadian = -currentRadian;
			float x = Mathf.Cos(currentRadian) * radius;
			float y = Mathf.Sin(currentRadian) * radius;
			Vector2 point = new Vector2(x, y) + center;
			return point;
		}

		/// <summary>
		/// Find component unless it has been found before.
		/// </summary>
		/// <typeparam name="T">Component type</typeparam>
		/// <param name="component">Component to look for</param>
		/// <param name="localReference">Reference where the found component is cached</param>
		/// <returns></returns>
		public static T LazyComponent<T>(Component component, ref T localReference) where T : Component
		{
			if (localReference == null)
			{
				localReference = component.GetComponent<T>();
			}
			return localReference;
		}
		/// <summary>
		/// Find components unless they have been found before.
		/// </summary>
		/// <typeparam name="T">Component type</typeparam>
		/// <param name="component">Components to look for</param>
		/// <param name="localReference">Reference where the found components are cached</param>
		/// <returns></returns>
		public static T[] LazyComponents<T>(Component component, ref T[] localReference) where T : Component
		{
			if (localReference == null)
			{
				localReference = component.GetComponentsInChildren<T>();
			}
			return localReference;
		}
		/// <summary>
		/// Find component in parent unless it has been found before.
		/// </summary>
		/// <typeparam name="T">Component type</typeparam>
		/// <param name="component">Component to look for</param>
		/// <param name="localReference">Reference where the found component is cached</param>
		/// <returns></returns>
		public static T LazyParentComponent<T>(Component component, ref T localReference) where T : Component
		{
			if (localReference == null)
			{
				localReference = component.GetComponentInParent<T>();
			}
			return localReference;
		}

	}
}
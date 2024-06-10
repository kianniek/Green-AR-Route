using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ShellanderGames.WeaponWheel
{

	/// <summary>
	/// Extension of the Image component with support for uv offset.
	/// </summary>
	public class SgFillableCircleImage : Image
	{
		static readonly Vector3[] s_Xy = new Vector3[4];
		static readonly Vector3[] s_Uv = new Vector3[4];
		private Sprite activeSprite => sprite;

		public float uvRotation = 0;

#if UNITY_EDITOR
		protected override void Reset()
		{
			base.Reset();
			type = Type.Filled;
			fillClockwise = true;
			fillMethod = FillMethod.Radial360;
			fillOrigin = 2;
			preserveAspect = false;
		}
#endif


		/// <summary>
		/// Update the UI renderer mesh.
		/// </summary>
		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			if (activeSprite == null)
			{
				base.OnPopulateMesh(toFill);
				return;
			}

			switch (type)
			{
				case Type.Filled:
					GenerateFilledSprite(toFill);
					break;
			}
		}

		/// <summary>
		/// Generate vertices for a filled Radial360 Image.
		/// </summary>
		void GenerateFilledSprite(VertexHelper toFill)
		{
			toFill.Clear();

			if (fillAmount < 0.001f)
			{
				return;
			}


			Vector4 v = GetDrawingDimensions();
			Vector4 outer = activeSprite != null ? UnityEngine.Sprites.DataUtility.GetOuterUV(activeSprite) : Vector4.zero;
			UIVertex uiv = UIVertex.simpleVert;
			uiv.color = color;

			float tx0 = outer.x;
			float ty0 = outer.y;
			float tx1 = outer.z;
			float ty1 = outer.w;

			s_Xy[0] = new Vector2(v.x, v.y);
			s_Xy[1] = new Vector2(v.x, v.w);
			s_Xy[2] = new Vector2(v.z, v.w);
			s_Xy[3] = new Vector2(v.z, v.y);

			s_Uv[0] = new Vector2(tx0, ty0);
			s_Uv[1] = new Vector2(tx0, ty1);
			s_Uv[2] = new Vector2(tx1, ty1);
			s_Uv[3] = new Vector2(tx1, ty0);


			if (fillMethod == FillMethod.Radial360)
			{
				for (int corner = 0; corner < 4; ++corner)
				{
					float fx0, fx1, fy0, fy1;

					if (corner < 2)
					{
						fx0 = 0f;
						fx1 = 0.5f;
					}
					else
					{
						fx0 = 0.5f;
						fx1 = 1f;
					}

					if (corner == 0 || corner == 3)
					{
						fy0 = 0.0f;
						fy1 = 0.5f;
					}
					else
					{
						fy0 = 0.5f;
						fy1 = 1f;
					}

					s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
					s_Xy[1].x = s_Xy[0].x;
					s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
					s_Xy[3].x = s_Xy[2].x;

					s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
					s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
					s_Xy[2].y = s_Xy[1].y;
					s_Xy[3].y = s_Xy[0].y;

					s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
					s_Uv[1].x = s_Uv[0].x;
					s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
					s_Uv[3].x = s_Uv[2].x;

					s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
					s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
					s_Uv[2].y = s_Uv[1].y;
					s_Uv[3].y = s_Uv[0].y;

					float val = fillAmount * 4f - ((corner + fillOrigin) % 4);

					if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), true, ((corner + 2) % 4)))
					{
						for (int i = 0; i < 4; i++)
						{
							s_Uv[i] = RotatePoint(s_Uv[i], new Vector2(0.5f, 0.5f), uvRotation, 0, 1);
						}

						AddQuad(toFill, s_Xy, color, s_Uv);
					}

				}
			}
		}

		private static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, float angleInDegrees, float c1, float c2)
		{
			float angleInRadians = angleInDegrees * (Mathf.PI / 180);
			float cosTheta = Mathf.Cos(angleInRadians);
			float sinTheta = Mathf.Sin(angleInRadians);

			Vector2 rotatedPoint = new Vector2
			(
				(cosTheta * (pointToRotate.x - centerPoint.x) -
				sinTheta * (pointToRotate.y - centerPoint.y) + centerPoint.x),
				(sinTheta * (pointToRotate.x- centerPoint.x) +
				cosTheta * (pointToRotate.y - centerPoint.y) + centerPoint.y)
			);

			return rotatedPoint;
		}

		private Vector4 GetDrawingDimensions()
		{
			var padding = activeSprite == null ? Vector4.zero : UnityEngine.Sprites.DataUtility.GetPadding(activeSprite);
			var size = activeSprite == null ? Vector2.zero : new Vector2(activeSprite.rect.width, activeSprite.rect.height);

			Rect r = GetPixelAdjustedRect();
			// Debug.Log(string.Format("r:{2}, size:{0}, padding:{1}", size, padding, r));

			int spriteW = Mathf.RoundToInt(size.x);
			int spriteH = Mathf.RoundToInt(size.y);

			var v = new Vector4(
				padding.x / spriteW,
				padding.y / spriteH,
				(spriteW - padding.z) / spriteW,
				(spriteH - padding.w) / spriteH);

			v = new Vector4(
				r.x + r.width * v.x,
				r.y + r.height * v.y,
				r.x + r.width * v.z,
				r.y + r.height * v.w
			);

			return v;
		}

		static bool RadialCut(Vector3[] xy, Vector3[] uv, float fill, bool invert, int corner)
		{
			// Nothing to fill
			if (fill < 0.001f) return false;

			// Even corners invert the fill direction
			if ((corner & 1) == 1) invert = !invert;

			// Nothing to adjust
			if (!invert && fill > 0.999f) return true;

			// Convert 0-1 value into 0 to 90 degrees angle in radians
			float angle = Mathf.Clamp01(fill);
			if (invert) angle = 1f - angle;
			angle *= 90f * Mathf.Deg2Rad;

			// Calculate the effective X and Y factors
			float cos = Mathf.Cos(angle);
			float sin = Mathf.Sin(angle);

			RadialCut(xy, cos, sin, invert, corner);
			RadialCut(uv, cos, sin, invert, corner);
			return true;
		}

		static void RadialCut(Vector3[] xy, float cos, float sin, bool invert, int corner)
		{
			int i0 = corner;
			int i1 = ((corner + 1) % 4);
			int i2 = ((corner + 2) % 4);
			int i3 = ((corner + 3) % 4);

			if ((corner & 1) == 1)
			{
				if (sin > cos)
				{
					cos /= sin;
					sin = 1f;

					if (invert)
					{
						xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
						xy[i2].x = xy[i1].x;
					}
				}
				else if (cos > sin)
				{
					sin /= cos;
					cos = 1f;

					if (!invert)
					{
						xy[i2].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
						xy[i3].y = xy[i2].y;
					}
				}
				else
				{
					cos = 1f;
					sin = 1f;
				}

				if (!invert) xy[i3].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
				else xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
			}
			else
			{
				if (cos > sin)
				{
					sin /= cos;
					cos = 1f;

					if (!invert)
					{
						xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
						xy[i2].y = xy[i1].y;
					}
				}
				else if (sin > cos)
				{
					cos /= sin;
					sin = 1f;

					if (invert)
					{
						xy[i2].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
						xy[i3].x = xy[i2].x;
					}
				}
				else
				{
					cos = 1f;
					sin = 1f;
				}

				if (invert) xy[i3].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
				else xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
			}
		}

		static void AddQuad(VertexHelper vertexHelper, Vector3[] quadPositions, Color32 color, Vector3[] quadUVs)
		{
			int startIndex = vertexHelper.currentVertCount;

			for (int i = 0; i < 4; ++i) {
				vertexHelper.AddVert(quadPositions[i], color, quadUVs[i]);
			}

			vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
			vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
		}


	}

#if UNITY_EDITOR
		[CustomEditor(typeof(SgFillableCircleImage))]
		public class SgFillableCircleImageInspector : Editor
		{

			public override void OnInspectorGUI()
			{
				DrawDefaultInspector();
			}
	}
#endif
}
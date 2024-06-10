using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShellanderGames.WeaponWheel
{
    /// <summary>
    /// One slice has one circle slice per circle in SgWeaponWheel. In other words,
    /// one slice can have multiple circle slices.
    /// </summary>
    public class SgCircleSlice : MonoBehaviour
    {
        private SgFillableCircleImage m_Image;
        public SgFillableCircleImage Image => SgMiscUtil.LazyComponent(this, ref m_Image);
        private RectTransform m_RectTransform;
        public RectTransform RectTransform => SgMiscUtil.LazyComponent(this, ref m_RectTransform);
        private SgSelectable m_Selectable;
        public SgSelectable Selectable => SgMiscUtil.LazyComponent(this, ref m_Selectable);


        public SgCircleData circleData;
        public int sliceIndex = -1;
    }
}

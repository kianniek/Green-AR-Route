using System;
using UnityEngine;
using UnityEngine.UI;

namespace ShellanderGames.WeaponWheel
{
	/// <summary>
	/// Updates a UI Text component with performance, quality and screen info.
	/// </summary>
	public class SgFpsDebugInfo : MonoBehaviour
	{
		private Text m_TextComponent;
		private int m_PrevSeconds = -1;
		private int m_FpsCounter = 0;
		void Start()
		{
			m_TextComponent = GetComponent<Text>();
		}

		void Update()
		{
			int seconds = DateTime.Now.Second;
			m_FpsCounter++;
			if (m_PrevSeconds != seconds) {
				string gameResolution = Screen.width + "x" + Screen.height;
				m_TextComponent.text = "FPS=" + m_FpsCounter + 
					", GameQuality=" + QualitySettings.GetQualityLevel() + 
					", GameRes="+gameResolution + 
					", ScreenRes=" + Screen.currentResolution;

				m_FpsCounter = 0;
			}
			m_PrevSeconds = seconds;
		}
	}
}

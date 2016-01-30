using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class bs_LobbyOptions : MonoBehaviour {

	public Text m_FullScreen;

	public Text m_QualityText;
	protected string[] m_Qualitys = new string[] { "Fastest", "Fast", "Simple", "Good", "Beautiful", "Fantastic" };
	protected int m_Quality = 0;

	public Text VolumeText = null;
	private float volume = 1.0f;


	// Use this for initialization
	void Start ()
	{
		//sets the defualts for options to match the game
		m_Quality = QualitySettings.GetQualityLevel();
		m_FullScreen.text = (Screen.fullScreen ? "Fullscreen" : "Windowed");
		m_QualityText.text = "Quality: " + m_Qualitys[QualitySettings.GetQualityLevel()];
		AudioListener.volume = volume;
	}

	/// <summary>
	/// Changes the quality level and set the button text to match
	/// </summary>
	/// <param name="ChangeQuality">ChangeQuality</param>
	public void ChangeQuality(Text QText)
	{
		if (QText.text  == "Please wait ...")
			return;

		QText.text = "Please wait ...";
		m_Quality++;
		if (m_Quality > 5)
			m_Quality = 0;

			QualitySettings.SetQualityLevel(m_Quality, false);
			QText.text = "Quality: " + m_Qualitys[QualitySettings.GetQualityLevel()];

	}

	/// <summary>
	/// Changes the resolution level and set the button text to match
	/// </summary>
	/// <param name="ChangeResolution">ChangeResolution</param>
	public void ChangeResolution(Text FSText)
	{
		if (FSText.text  == "Please wait ...")
			return;

		FSText.text = "Please wait ...";

		ToggleFullscreen();
		FSText.text = (Screen.fullScreen ? "Fullscreen" : "Windowed");
	}

	/// <summary>
	/// called from the ChangeResolution()
	/// </summary>
	protected virtual void ToggleFullscreen()
	{
		if (!Screen.fullScreen)
		{
			Resolution k = new Resolution();
			foreach (Resolution r in Screen.resolutions)
			{
				if (r.width > k.width)
				{
					k.width = r.width;
					k.height = r.height;
				}
			}
			Screen.SetResolution(k.width, k.height, true);
		}
		else
			Screen.SetResolution(800, 600, false);
	}

	/// <summary>
	/// Changes the volume level and set the button text to match
	/// </summary>
	/// <param name="Volume">Volume</param>
	public void Volume(float v)
	{
		volume = v;

		if (VolumeText != null)
		{
			VolumeText.text = (volume * 100).ToString("00") + "%";
		}
	}
}

using UnityEngine;
using System.Collections;

namespace AlmostEngine.Screenshot
{
	[System.Serializable]
	/// <summary>
	/// Screenshot resolution are the resolutions used for rendering screenshots.
	/// </summary>
	public class ScreenshotResolution
	{
		[HideInInspector]
		public bool m_Active = true;

		public int m_Width = 1920;
		public int m_Height = 1080;
		public float m_Scale = 1f;

		public enum Orientation
		{
			LANDSCAPE,
			PORTRAIT}
		;

		public Orientation m_Orientation;


		[HideInInspector]
		public string m_ResolutionName = "";
		[HideInInspector]
		public string m_Category = "Custom";


		public int m_PPI = 0;
		[Tooltip ("If the Screen.dpi device value returned by Unity is not equals to the real device screen dpi, " +
		"you can set this value to render the device content like it will be it on the device.")]
		public int m_ForcedUnityPPI = 0;

		public Canvas m_DeviceCanvas;

		//		public Canvas m_LandscapeDeviceCanvasLandscape;

		//		public float m_Stats = 0f;

		[HideInInspector]
		public string m_Ratio = "";

		[HideInInspector]
		public string m_FileName = "";

		[System.NonSerialized]
		public Texture2D m_Texture;


		[HideInInspector]
		public bool m_IgnoreOrientation = false;

		public ScreenshotResolution ()
		{
		}

		public ScreenshotResolution (ScreenshotResolution res)
		{
			m_Active = res.m_Active;
			m_Width = res.m_Width;
			m_Height = res.m_Height;
			m_Scale = res.m_Scale;

			m_Orientation = res.m_Orientation;

			m_ResolutionName = res.m_ResolutionName;
			m_Category = res.m_Category;

			m_PPI = res.m_PPI;
			m_ForcedUnityPPI = res.m_ForcedUnityPPI;

			m_Ratio = res.m_Ratio;
//			m_Stats = res.m_Stats;

			m_DeviceCanvas = res.m_DeviceCanvas;

			m_FileName = res.m_FileName;
		}


		public ScreenshotResolution (int width, int height)
		{
			m_Width = width;
			m_Height = height;
		}

		public ScreenshotResolution (string category, int width, int height, string name = "", int dpi = 0)
		{
			m_Category = category;
			m_Width = width;
			m_Height = height;
			m_ResolutionName = name;
			m_PPI = dpi;
//			m_Stats = stats;

			UpdateRatio ();
		}

		public void UpdateRatio ()
		{
			int gcd = GCD (m_Width, m_Height);
			m_Ratio = ((float)m_Width / (float)gcd).ToString () + ":" + ((float)m_Height / (float)gcd).ToString ();
			return;
		}

		int GCD (int a, int b)
		{
			if (b == 0)
				return a;
			return GCD (b, a % b);
		}

		public bool IsValid ()
		{
			if (m_Width <= 0 || m_Height <= 0) {
				return false;
			}
			return true;			
		}

		public override string ToString ()
		{
			UpdateRatio ();
			string name = "";
			if (m_ResolutionName != "") {
				name += m_ResolutionName + "   -   ";
			}
			name += m_Width + "x" + m_Height;
			name += "  " + m_Ratio;
			if (m_PPI > 0) {
				name += "  " + m_PPI + "ppi";
			}
//			if (m_Stats > 0f) {
//				name += "          " + m_Stats + "%";
//			}
			return name;
		}

		public int ComputeTargetWidth ()
		{
			float width;
			if (m_IgnoreOrientation || m_Orientation == Orientation.LANDSCAPE) {
				width = m_Width;
			} else {
				width = m_Height;
			}
			if (m_Scale > 0) {
				width *= m_Scale;
			}
			
			return (int)width;
		}

		public int ComputeTargetHeight ()
		{
			float height;
			if (m_IgnoreOrientation || m_Orientation == Orientation.LANDSCAPE) {
				height = m_Height;
			} else {
				height = m_Width;
			}
			
			if (m_Scale > 0) {
				height *= m_Scale;
			}
			
			return (int)height;
		}
	}

}
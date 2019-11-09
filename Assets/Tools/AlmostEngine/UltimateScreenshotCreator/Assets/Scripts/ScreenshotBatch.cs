using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AlmostEngine.Screenshot
{
	[System.Serializable]
	public class ScreenshotBatch
	{
		public bool m_Active = true;
		public string m_Name = "";

		public List<ScreenshotProcess> m_PreProcess = new List<ScreenshotProcess> ();
		public List<ScreenshotProcess> m_PostProcess = new List<ScreenshotProcess> ();
	}
}


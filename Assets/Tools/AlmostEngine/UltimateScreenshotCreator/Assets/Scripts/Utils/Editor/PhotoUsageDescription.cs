using UnityEngine;
using System.Collections;

using UnityEditor;

namespace AlmostEngine.Screenshot
{
	public class PhotoUsageDescription : ScriptableObject
	{
		public string m_UsageDescription = "This application requires the access to the photo library to allow the user to take screenshots that are automatically added to the Camera Roll.";

		static PhotoUsageDescription m_Usage;
		public bool m_NeedsPermission = true;
	}
}
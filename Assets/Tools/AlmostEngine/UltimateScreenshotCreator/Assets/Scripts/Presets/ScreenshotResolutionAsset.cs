using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlmostEngine.Screenshot
{
	#if UNITY_EDITOR && UNITY_2017_1_OR_NEWER
	[CreateAssetMenu (fileName = "Custom preset", menuName = "AlmostEngine/Ultimate Screenshot Creator/Custom preset")]
	#endif
	[System.Serializable]
	public class ScreenshotResolutionAsset : ScriptableObject
	{
		public ScreenshotResolution m_Resolution;

		#if UNITY_EDITOR && !UNITY_2017_1_OR_NEWER
		[MenuItem ("Tools/Ultimate Screenshot Creator/Create Resolution Preset")]
		public static void CreateAsset ()
		{
			ProjectWindowUtil.ShowCreatedAsset (ScriptableObjectUtils.CreateAsset<ScreenshotResolutionAsset> ("Custom preset", "Presets/Custom"));
		}
		#endif
	}
}


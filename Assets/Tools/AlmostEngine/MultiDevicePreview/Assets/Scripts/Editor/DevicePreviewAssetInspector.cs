using UnityEngine;
using UnityEditor;
using System.Collections;


namespace AlmostEngine.Preview
{
	[CustomEditor (typeof(PreviewConfigAsset))]
	public class PreviewConfigAssetInspector : Editor
	{
		public override void OnInspectorGUI ()
		{
			EditorGUILayout.HelpBox ("This asset contains the settings used by the Multi Device Preview & Gallery.", MessageType.Info);

			if (GUILayout.Button ("Open Settings")) {
				ResolutionSettingsWindow.Init ();
			}
		}
	}
}

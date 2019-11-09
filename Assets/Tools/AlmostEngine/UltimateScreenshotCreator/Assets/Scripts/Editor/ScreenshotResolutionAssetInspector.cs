using UnityEngine;
using UnityEditor;
using System.Collections;


namespace AlmostEngine.Screenshot
{
	[CustomEditor (typeof(ScreenshotResolutionAsset))]
	[CanEditMultipleObjects]
	public class ScreenshotResolutionAssetInspectorInspector : Editor
	{
		public override void OnInspectorGUI ()
		{
			serializedObject.Update ();

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Resolution.m_Width"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Resolution.m_Height"));
//			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Resolution.m_Scale"));

			if (typeof(ScreenshotManager).Assembly.GetType ("AlmostEngine.Preview.MultiDevicePreviewGallery") != null) {
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Resolution.m_PPI"));
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Resolution.m_ForcedUnityPPI"));
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Resolution.m_DeviceCanvas"));
			}

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Resolution.m_Orientation"));

			serializedObject.ApplyModifiedProperties ();
		}
	}
}

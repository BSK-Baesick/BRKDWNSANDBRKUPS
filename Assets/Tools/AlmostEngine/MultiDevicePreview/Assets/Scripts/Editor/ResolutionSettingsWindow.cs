using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using AlmostEngine.Screenshot;

namespace AlmostEngine.Preview
{
	public class ResolutionSettingsWindow : EditorWindow
	{
		[MenuItem ("Window/Almost Engine/Multi Device Preview and Gallery/Settings")]
		public static void Init ()
		{
			m_Window = (ResolutionSettingsWindow)EditorWindow.GetWindow (typeof(ResolutionSettingsWindow), false, "Devices Config");
			m_Window.Show ();
		}

		public static ResolutionSettingsWindow m_Window;

		public static bool IsOpen ()
		{
			return m_Window != null;
		}

		ScreenshotConfigDrawer m_ConfigDrawer;
		PreviewConfigAsset m_ConfigAsset;
		SerializedObject serializedObject;

		Vector2 m_ScrollPos;

		public static PreviewConfigAsset GetConfig ()
		{
			PreviewConfigAsset asset = AssetUtils.GetOrCreate<PreviewConfigAsset> ("Assets/MultiDevicePreviewConfig.asset");

			asset.m_Config.m_CaptureMode = ScreenshotTaker.CaptureMode.GAMEVIEW_RESIZING;
			asset.m_Config.m_CameraMode = ScreenshotConfig.CamerasMode.GAME_VIEW;
			asset.m_Config.m_ResolutionCaptureMode = ScreenshotConfig.ResolutionMode.CUSTOM_RESOLUTIONS;
			asset.m_Config.m_ShotMode = ScreenshotConfig.ShotMode.ONE_SHOT;
			asset.m_Config.m_StopTimeOnCapture = false;
			asset.m_Config.m_PlaySoundOnCapture = false;

			return asset;
		}

		void OnEnable ()
		{
			m_ConfigAsset = GetConfig ();

			serializedObject = new SerializedObject (m_ConfigAsset);

			m_ConfigDrawer = new ScreenshotConfigDrawer ();
			m_ConfigDrawer.m_IsDevice = true;
			m_ConfigDrawer.Init (serializedObject, m_ConfigAsset, m_ConfigAsset.m_Config, serializedObject.FindProperty ("m_Config"));

		}

		#region GUI

		void OnGUI ()
		{
			InitAllGameviewSizes ();

			serializedObject.Update ();
			EditorGUI.BeginChangeCheck ();

			m_ScrollPos = EditorGUILayout.BeginScrollView (m_ScrollPos);

			EditorGUILayout.Separator ();

			EditorGUILayout.BeginVertical (GUI.skin.box);
			DrawGallery ();
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Separator ();

			DrawConfig ();

			EditorGUILayout.Separator ();
			EditorGUILayout.Separator ();
			EditorGUILayout.Separator ();
			EditorGUILayout.Separator ();
			DrawSupportGUI ();


			EditorGUILayout.Separator ();
			EditorGUILayout.Separator ();
			DrawContactGUI ();


			EditorGUILayout.EndScrollView ();


			if (EditorGUI.EndChangeCheck ()) {
				ResolutionWindowBase.RepaintWindows ();
			}

			serializedObject.ApplyModifiedProperties ();
		}


		protected int m_WindowWidth;

		protected void DrawGallery ()
		{

			m_ConfigAsset.m_ShowGallery = EditorGUILayout.Foldout (m_ConfigAsset.m_ShowGallery, "Gallery".ToUpper ());
			if (m_ConfigAsset.m_ShowGallery == false)
				return;
			EditorGUILayout.Separator ();

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_ScreenPPI"));

			EditorGUILayout.HelpBox ("Set ScreenPPI to the PPI of your display to have correct physical sizes in PPI mode.", MessageType.Info);

			EditorGUILayout.Separator ();
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_AutoRefresh"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_RefreshMode"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_RefreshDelay"));
			EditorGUILayout.Separator ();
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_ZoomScrollSpeed"));


			EditorGUILayout.Separator ();
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_DrawingMode"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_DeviceRendererCamera"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_DefaultDeviceCanvas"));
			EditorGUILayout.Separator ();
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_TransparentDeviceBackground"));
		}

		protected void DrawConfig ()
		{
			EditorGUILayout.BeginVertical (GUI.skin.box);
			m_ConfigDrawer.DrawResolutionTitleGUI ();



			if (m_ConfigAsset.m_ExpandDevices) {
				if (GUILayout.Button ("Hide device settings")) {
					m_ConfigAsset.m_ExpandDevices = false;
					m_ConfigDrawer.m_Expanded = false;
				}
			} else {
				if (GUILayout.Button ("Expand device settings ")) {
					m_ConfigAsset.m_ExpandDevices = true;
					m_ConfigDrawer.m_Expanded = true;
				}
			}

			EditorGUILayout.Separator ();

			m_ConfigDrawer.DrawResolutionContentGUI ();

			EditorGUILayout.EndVertical ();
			EditorGUILayout.Separator ();

			EditorGUILayout.BeginVertical (GUI.skin.box);
			m_ConfigDrawer.DrawFolderGUI ();
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Separator ();

			EditorGUILayout.BeginVertical (GUI.skin.box);
			m_ConfigDrawer.DrawNameGUI ();
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Separator ();

			EditorGUILayout.BeginVertical (GUI.skin.box);
			m_ConfigDrawer.DrawDelay (false);
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Separator ();


			EditorGUILayout.BeginVertical (GUI.skin.box);
			m_ConfigAsset.m_Config.m_ShowUtils = EditorGUILayout.Foldout (m_ConfigAsset.m_Config.m_ShowUtils, "Hotkeys".ToUpper ());
			if (m_ConfigAsset.m_Config.m_ShowUtils != false) {

				EditorGUILayout.Separator ();
				EditorGUILayout.HelpBox ("Note that you can use the Tools menu item hotkeys to Update the preview and Export the screenshots to files. " +
					"You can customize them by editing the UpdateDevicePreviewMenuItem.cs and ExportDevicePreviewMenuItem.cs scripts.", MessageType.Info);

			}
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Separator ();

			EditorGUILayout.BeginVertical (GUI.skin.box);
			m_ConfigDrawer.DrawUsage ();
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Separator ();



//			EditorGUILayout.BeginVertical (GUI.skin.box);
//			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Config.m_GameViewResizingWaitingMode")); 
//			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Config.m_ResizingWaitingTime")); 
//			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Config.m_ResizingWaitingFrames")); 
//			EditorGUILayout.EndVertical ();
//			EditorGUILayout.Separator ();
//

		}

		public static void DrawSupportGUI ()
		{

			EditorGUILayout.BeginVertical (GUI.skin.box);
			EditorGUILayout.LabelField ("SUPPORT");

			EditorGUILayout.HelpBox ("If you notice any mistake in the device resolutions, PPI values, or preview pictures, please let me know. " +
			"Also, you can contact me if you want a specific device to be added. " +
			"All suggestions are welcome.", MessageType.Info);

			Color cc = GUI.color;
			GUI.color = new Color (0.55f, 0.7f, 1f, 1.0f);
			if (GUILayout.Button ("More assets from Wild Mage Games")) {
				Application.OpenURL ("https://www.wildmagegames.com/unity/");
			}
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Contact support")) {
				Application.OpenURL ("mailto:support@wildmagegames.com");
			}
			GUI.color = new Color (0.6f, 1f, 0.6f, 1.0f);
			if (GUILayout.Button ("Leave a Review")) {
				Application.OpenURL ("https://assetstore.unity.com/packages/tools/utilities/multi-device-preview-gallery-82015");
			}
			GUI.color = cc;
			EditorGUILayout.EndHorizontal (); 

			EditorGUILayout.EndVertical ();
			EditorGUILayout.Separator ();

		}


		protected void DrawContactGUI ()
		{
			EditorGUILayout.LabelField (MultiDevicePreviewGallery.VERSION, UIStyle.centeredGreyTextStyle);
			EditorGUILayout.LabelField (MultiDevicePreviewGallery.AUTHOR, UIStyle.centeredGreyTextStyle);
		}

		#endregion

		string m_GameviewSizeName = "MultiDevicePreview - ";

		void InitAllGameviewSizes ()
		{
			// Remove all resolutions not in devices
			List<string> deviceSizes = new List<string> ();
			foreach (ScreenshotResolution res in m_ConfigAsset.m_Config.GetActiveResolutions()) {
				deviceSizes.Add (m_GameviewSizeName + res.ToString ());
			}
			List<string> gameviewSizes = GameViewUtils.GetAllSizeNames ();
			foreach (string size in gameviewSizes) {
				if (size.Contains (m_GameviewSizeName) && !deviceSizes.Contains (size)) {
					GameViewUtils.RemoveCustomSize (GameViewUtils.FindSize (size));
				}
			}

			// Add all new devices
			foreach (ScreenshotResolution res in m_ConfigAsset.m_Config.GetActiveResolutions()) {
				if (!gameviewSizes.Contains (m_GameviewSizeName + res.ToString ())) {
					GameViewUtils.AddCustomSize (GameViewUtils.SizeType.FIXED_RESOLUTION, res.ComputeTargetWidth (), res.ComputeTargetHeight (), m_GameviewSizeName + res.ToString ());
				}
			}


		}
	}
}
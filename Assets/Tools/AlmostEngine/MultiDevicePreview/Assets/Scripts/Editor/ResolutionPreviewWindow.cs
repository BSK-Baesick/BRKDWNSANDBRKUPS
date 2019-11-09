#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AlmostEngine.Screenshot;

namespace AlmostEngine.Preview
{
	public class ResolutionPreviewWindow : ResolutionWindowBase
	{
				
		[MenuItem ("Window/Almost Engine/Multi Device Preview and Gallery/Device Preview")]
		static void Init ()
		{
			ResolutionPreviewWindow window = (ResolutionPreviewWindow)EditorWindow.GetWindow (typeof(ResolutionPreviewWindow), false, "Preview");
			window.Show ();
		}

		public static ResolutionPreviewWindow m_Window;

		public static bool IsOpen ()
		{
			return m_Window != null;
		}

		protected override void OnEnable ()
		{
			m_Window = this;
			base.OnEnable ();
		}

		protected override void OnDisable ()
		{
			m_Window = null;
			base.OnDisable ();
		}

		protected override void HandleEditorEvents ()
		{
			Event e = Event.current;

			if (e == null)
				return;

			// Zoom
			if (e.type == EventType.ScrollWheel && e.control) {
				m_ConfigAsset.m_PreviewZoom -= m_ConfigAsset.m_ZoomScrollSpeed * e.delta.y;
				e.Use ();
			}
		}

		ScreenshotResolution m_SelectedResolution;

		protected override void DrawToolBarGUI ()
		{
			EditorGUILayout.BeginHorizontal (EditorStyles.toolbar);


			// BUTTONS
			if (GUILayout.Button ("Update", EditorStyles.toolbarButton)) {
				UpdateWindowResolutions ();
			}

			if (GUILayout.Button ("Export to file(s)", EditorStyles.toolbarButton)) {
				Export ();
			}

			// SPACE
			GUILayout.FlexibleSpace ();

			// MODE
			EditorGUILayout.LabelField ("Display mode", GUILayout.MaxWidth (85));
			EditorGUILayout.PropertyField (m_SerializedConfig.FindProperty ("m_PreviewDisplayMode"), GUIContent.none, GUILayout.MaxWidth (70)); 


			// MODE
			EditorGUILayout.LabelField ("Drawing mode", GUILayout.MaxWidth (85));
			EditorGUILayout.PropertyField (m_SerializedConfig.FindProperty ("m_DrawingMode"), GUIContent.none, GUILayout.MaxWidth (100)); 




			if (GUILayout.Button ("Settings", EditorStyles.toolbarButton)) {
				ResolutionSettingsWindow.Init ();
			}

			// ABOUT
			if (GUILayout.Button ("About", EditorStyles.toolbarButton)) {
				MultiDevicePreviewGallery.About ();
			}

			EditorGUILayout.EndHorizontal ();


			EditorGUILayout.BeginHorizontal (EditorStyles.toolbar);


			// AUTO REFRESH
			if (m_ConfigAsset.m_AutoRefresh) {
				if (GUILayout.Button ("Auto refresh (ON)", EditorStyles.toolbarButton)) {
					m_ConfigAsset.m_AutoRefresh = false;
					EditorUtility.SetDirty (m_ConfigAsset);
				}
			} else {
				if (GUILayout.Button ("Auto refresh (OFF)", EditorStyles.toolbarButton)) {
					m_ConfigAsset.m_AutoRefresh = true;
					EditorUtility.SetDirty (m_ConfigAsset);
				}
			}
			EditorGUILayout.LabelField ("Refresh delay (s)", GUILayout.MaxWidth (110));
			float delay = EditorGUILayout.Slider (m_ConfigAsset.m_RefreshDelay, 0.01f, 10f, GUILayout.MaxWidth (200));
			if (delay != m_ConfigAsset.m_RefreshDelay) {
				m_ConfigAsset.m_RefreshDelay = delay;
				EditorUtility.SetDirty (m_ConfigAsset);
			}



			// SPACE
			GUILayout.FlexibleSpace ();

			// ZOOM
			EditorGUILayout.LabelField ("Zoom", GUILayout.MaxWidth (40));
			float zoom = EditorGUILayout.Slider (m_ConfigAsset.m_PreviewZoom, 0.05f, 4f);
			if (zoom != m_ConfigAsset.m_PreviewZoom) {
				m_ConfigAsset.m_PreviewZoom = zoom;
				EditorUtility.SetDirty (m_ConfigAsset);
			}

			if (GUILayout.Button ("1:1", EditorStyles.toolbarButton)) {
				m_ConfigAsset.m_PreviewZoom = 1f;
				EditorUtility.SetDirty (m_ConfigAsset);
			}

			EditorGUILayout.EndHorizontal ();


			EditorGUILayout.BeginHorizontal (EditorStyles.toolbar);



			GUILayout.FlexibleSpace ();

			// SELECTION
			List<string> names = new List<string> ();
			foreach (ScreenshotResolution resolution in m_ConfigAsset.m_Config.GetActiveResolutions()) {
				names.Add (resolution.ToString ());
			}
			int selected = EditorGUILayout.Popup (m_ConfigAsset.m_Selected, names.ToArray (), GUILayout.MinWidth (300));
			if (selected != m_ConfigAsset.m_Selected) {
				m_ConfigAsset.m_Selected = selected;
				EditorUtility.SetDirty (m_ConfigAsset);
			}

			if (m_ConfigAsset.m_Selected >= m_ConfigAsset.m_Config.GetActiveResolutions ().Count) {
				m_ConfigAsset.m_Selected = 0;
			} 
			if (m_ConfigAsset.m_Config.GetActiveResolutions ().Count > 0) {
				m_SelectedResolution = m_ConfigAsset.m_Config.GetActiveResolutions () [m_ConfigAsset.m_Selected];
			} else {
				m_SelectedResolution = null;
			}




			GUILayout.FlexibleSpace ();

			EditorGUILayout.EndHorizontal ();




		}

		protected override void DrawPreviewGUI ()
		{
			if (m_SelectedResolution == null)
				return;

			Rect pos = GUILayoutUtility.GetLastRect ();
			pos = new Rect (m_ConfigAsset.m_MarginHorizontal, m_ConfigAsset.m_MarginVertical, m_WindowWidth, 1);

			// Start scroll area
			height = 0;
			width = 0;
			scroll = EditorGUILayout.BeginScrollView (scroll);

			// Draw the selected resolution
			DrawResolutionPreview (pos, m_SelectedResolution);

			// Make some space
			EditorGUILayout.LabelField ("", GUILayout.MinHeight (height));
			EditorGUILayout.LabelField ("", GUILayout.MinWidth (width));

			// End scroll
			EditorGUILayout.EndScrollView ();
		}

		void DrawResolutionPreview (Rect pos, ScreenshotResolution resolution)
		{
			Vector2 size = GetRenderSize (resolution, m_ConfigAsset.m_PreviewZoom, m_ConfigAsset.m_PreviewDisplayMode);
			if (size == Vector2.zero)
				return;

			pos.width = size.x;
			pos.height = size.y;

			// Center device
			if (pos.width < m_WindowWidth) {
				pos.x = (m_WindowWidth - pos.width) / 2;
			}
			
			// Draw the resolution texture
			if (resolution.m_Texture != null) {
				EditorGUI.DrawTextureTransparent (pos, resolution.m_Texture);
			} else {
				EditorGUI.DrawTextureTransparent (pos, m_BackgroundTexture);
				EditorGUI.LabelField (pos, "Needs to be updated.", m_NameStyle);
			}

			height = (int)Mathf.Max (height, pos.y + size.y);
			width = (int)Mathf.Max (width, size.x);

		}

		protected override void UpdateWindowResolutions ()
		{
			InitTempManager ();
			m_ScreenshotTaker.StartCoroutine (UpdateCoroutine (new List<ScreenshotResolution>{ m_SelectedResolution }));
		}

	}
}

#endif
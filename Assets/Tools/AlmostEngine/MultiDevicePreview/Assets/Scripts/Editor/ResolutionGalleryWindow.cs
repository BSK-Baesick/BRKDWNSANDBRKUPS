#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AlmostEngine.Screenshot;

namespace AlmostEngine.Preview
{
	/// <summary>
	/// The Resolution Gallery Window makes possible to preview the game in different resolutions at a glance.
	/// </summary>
	public class ResolutionGalleryWindow : ResolutionWindowBase
	{

		[MenuItem ("Window/Almost Engine/Multi Device Preview and Gallery/Gallery")]
		public static void Init ()
		{
			ResolutionGalleryWindow window = (ResolutionGalleryWindow)EditorWindow.GetWindow (typeof(ResolutionGalleryWindow), false, "Gallery");
			window.Show ();
		}

		public static ResolutionGalleryWindow m_Window;


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
				m_ConfigAsset.m_PreviewGalleryZoom -= m_ConfigAsset.m_ZoomScrollSpeed * e.delta.y;
				e.Use ();
			}
		}

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
			EditorGUILayout.PropertyField (m_SerializedConfig.FindProperty ("m_GalleryDisplayMode"), GUIContent.none, GUILayout.MaxWidth (70)); 

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
			float delay = EditorGUILayout.Slider (m_ConfigAsset.m_RefreshDelay, 0.01f, 10f);
			if (delay != m_ConfigAsset.m_RefreshDelay) {
				m_ConfigAsset.m_RefreshDelay = delay;
				EditorUtility.SetDirty (m_ConfigAsset);
			}



			// SPACE
			GUILayout.FlexibleSpace ();

			// ZOOM
			EditorGUILayout.LabelField ("Zoom", GUILayout.MaxWidth (40));
			float zoom = EditorGUILayout.Slider (m_ConfigAsset.m_PreviewGalleryZoom, 0.05f, 4f, GUILayout.ExpandWidth(true));
			if (zoom != m_ConfigAsset.m_PreviewGalleryZoom) {
				m_ConfigAsset.m_PreviewGalleryZoom = zoom;
				EditorUtility.SetDirty (m_ConfigAsset);
			}

			if (GUILayout.Button ("1:1", EditorStyles.toolbarButton)) {
				m_ConfigAsset.m_PreviewGalleryZoom = 1f;
				EditorUtility.SetDirty (m_ConfigAsset);
			}

			EditorGUILayout.EndHorizontal ();
		}

		protected override void DrawPreviewGUI ()
		{
			Rect pos = GUILayoutUtility.GetLastRect ();
			pos = new Rect (m_ConfigAsset.m_MarginHorizontal, m_ConfigAsset.m_MarginVertical, m_WindowWidth, 1);

			// Start scroll area
			height = 0;
			scroll = EditorGUILayout.BeginScrollView (scroll);
				
			// Draw each resolution
			foreach (ScreenshotResolution resolution in m_ConfigAsset.m_Config.GetActiveResolutions()) {
				pos = DrawResolutionPreview (pos, resolution);
			}

			// Make some space
			EditorGUILayout.LabelField ("", GUILayout.MinHeight (height));
			EditorGUILayout.LabelField ("", GUILayout.MinWidth (width));

			// End scroll
			EditorGUILayout.EndScrollView ();
		}

		Rect DrawResolutionPreview (Rect pos, ScreenshotResolution resolution)
		{
			Vector2 size = GetRenderSize (resolution, m_ConfigAsset.m_PreviewGalleryZoom, m_ConfigAsset.m_GalleryDisplayMode);
			if (size == Vector2.zero)
				return pos;

			// If can not draw the rect in the current row, create a new row
			if (pos.x > m_ConfigAsset.m_MarginHorizontal && pos.x + size.x + m_ConfigAsset.m_MarginHorizontal > m_WindowWidth) {
				pos.x = m_ConfigAsset.m_MarginHorizontal;
				pos.y = height + m_ConfigAsset.m_GalleryPaddingVertical;
			}
			pos.width = size.x;
			pos.height = size.y;

			// Draw the white background
			Rect borderpos = pos;
			borderpos.x -= m_ConfigAsset.m_GalleryBorderSize;
			borderpos.y -= m_ConfigAsset.m_GalleryBorderSize;
			borderpos.width += 2 * m_ConfigAsset.m_GalleryBorderSize;
			borderpos.height += 2 * m_ConfigAsset.m_GalleryBorderSize + m_ConfigAsset.m_GalleryTextHeight;


			// Draw the resolution texture
			if (resolution.m_Texture != null) {
				EditorGUI.DrawTextureTransparent (pos, resolution.m_Texture);
			} else {
				EditorGUI.DrawTextureTransparent (pos, m_BackgroundTexture);
				EditorGUI.LabelField (pos, "Needs to be updated.", m_NameStyle);
			}

			// Display the resolution name
			Rect labelpos = pos;
			labelpos.y = pos.y + size.y + 5;
			labelpos.height = m_ConfigAsset.m_GalleryTextHeight;
			EditorGUI.LabelField (labelpos, resolution.ToString (), m_NameStyle);

			// Increment the box position
			pos.x += size.x + m_ConfigAsset.m_GalleryPaddingVertical;
			height = (int)Mathf.Max (height, pos.y + size.y + 5 + m_ConfigAsset.m_GalleryTextHeight);
			width = (int)Mathf.Max (width, size.x);

			return pos;

		}


	}
}

#endif
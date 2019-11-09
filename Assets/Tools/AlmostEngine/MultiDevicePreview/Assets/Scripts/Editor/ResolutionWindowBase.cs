#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using System;

using AlmostEngine.Screenshot;

namespace AlmostEngine.Preview
{

	public class ResolutionWindowBase : EditorWindow
	{
		protected PreviewConfigAsset m_ConfigAsset;

		protected SerializedObject m_SerializedConfig;

		protected Texture2D m_BackgroundTexture;
		protected GUIStyle m_NameStyle;
		protected Vector2 m_ScrollviewPos;
		protected int m_WindowWidth;
		protected bool m_IsUpdating = false;


		public static UnityAction onUpdateBeginDelegate = () => {
		};
		public static UnityAction onUpdateEndDelegate = () => {
		};


		void Awake ()
		{
			Reset ();
		}

		void Reset ()
		{
//			Debug.Log ("Reset");

//			m_LastEditorUpdate = DateTime.Now;

			m_ConfigAsset = ResolutionSettingsWindow.GetConfig ();

			DestroyManager ();

			if (m_Camera) {
				GameObject.DestroyImmediate (m_Camera.gameObject);
			}
			if (m_Canvas) {
				GameObject.DestroyImmediate (m_Canvas.gameObject);
			}

			m_IsUpdating = false;
		}

		protected virtual void OnEnable ()
		{
			#if UNITY_2017_2_OR_NEWER
			EditorApplication.playModeStateChanged += StateChange;
			#else 
			EditorApplication.playmodeStateChanged += StateChange;
			#endif
		}

		protected virtual void OnDisable ()
		{
			#if UNITY_2017_2_OR_NEWER
			EditorApplication.playModeStateChanged -= StateChange;
			#else 
			EditorApplication.playmodeStateChanged -= StateChange;
			#endif
		}

		#if UNITY_2017_2_OR_NEWER
		void StateChange (PlayModeStateChange state)		
		


#else
		void StateChange ()
		#endif
		{
			Reset ();
//			if (EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying) {
//				InitTempManager ();
//			} else {
//				DestroyManager ();
//			}
		}

		#region Events

		protected virtual void HandleEditorEvents ()
		{
		}

		void Update ()
		{
			AutoRefresh ();
		}

		DateTime m_LastEditorUpdate = new DateTime ();

		void AutoRefresh ()
		{
			if (Application.isPlaying && m_ConfigAsset.m_RefreshMode == PreviewConfigAsset.AutoRefreshMode.ONLY_IN_EDIT_MODE)
				return;
						
			if (!Application.isPlaying && m_ConfigAsset.m_RefreshMode == PreviewConfigAsset.AutoRefreshMode.ONLY_IN_PLAY_MODE)
				return;

			if (!m_IsUpdating && m_ConfigAsset.m_AutoRefresh && (DateTime.Now - m_LastEditorUpdate).TotalSeconds > m_ConfigAsset.m_RefreshDelay) {
				UpdateAllRequiredResolutions ();
			}
		}

		#endregion

		#region UI


		void OnGUI ()
		{
			if (EditorApplication.isCompiling) {
				Reset ();
			}

			m_WindowWidth = (int)position.width;

			HandleEditorEvents ();

			InitStyle ();

			// Init manager serializer
			m_SerializedConfig = new SerializedObject (m_ConfigAsset);
			m_SerializedConfig.Update ();

			// Draw GUI
			EditorGUI.BeginChangeCheck ();
			DrawToolBarGUI ();
			if (EditorGUI.EndChangeCheck ()) {
				ResolutionWindowBase.RepaintWindows ();
			}

			DrawPreviewGUI ();

			// APply properties
			m_SerializedConfig.ApplyModifiedProperties ();

		}

		void InitStyle ()
		{
			if (m_BackgroundTexture == null) {
				m_BackgroundTexture = new Texture2D (1, 1);
				m_BackgroundTexture.SetPixel (0, 0, Color.white);
				m_BackgroundTexture.Apply ();
			}

			if (m_NameStyle == null) {
				m_NameStyle = new GUIStyle ();
				m_NameStyle.wordWrap = true;
				m_NameStyle.alignment = TextAnchor.MiddleCenter;
			}
		}

		static float PixelsPerPoint ()
		{
			#if UNITY_5_4_OR_NEWER
			return EditorGUIUtility.pixelsPerPoint;
			#else
			return 1f;
			#endif
		}

		protected Vector2 GetRenderSize (ScreenshotResolution resolution, float zoom, PreviewConfigAsset.GalleryDisplayMode mode)
		{
			int displayWidth, displayHeight;
			int width = resolution.ComputeTargetWidth ();
			int height = resolution.ComputeTargetHeight ();

			if (resolution.m_Texture != null) {
				width = resolution.m_Texture.width;
				height = resolution.m_Texture.height;
			}

			if (width <= 0 || height <= 0) {
				EditorGUILayout.LabelField ("Invalid dimensions for resolution " + resolution.ToString ());
				return Vector2.zero;
			}

			// Compute the box dimensions depending on the display mode
			if (mode == PreviewConfigAsset.GalleryDisplayMode.RATIOS) {
				displayWidth = (int)((m_WindowWidth - m_ConfigAsset.m_MarginHorizontal) * zoom);
				displayHeight = (int)(displayWidth * height / width);
			} else if (mode == PreviewConfigAsset.GalleryDisplayMode.PIXELS || resolution.m_PPI <= 0) {
				displayWidth = (int)(width * zoom / PixelsPerPoint ());
				displayHeight = (int)(height * zoom / PixelsPerPoint ());
			} else {
				displayWidth = (int)(width * zoom / resolution.m_PPI * m_ConfigAsset.m_ScreenPPI / PixelsPerPoint ());
				displayHeight = (int)(height * zoom / resolution.m_PPI * m_ConfigAsset.m_ScreenPPI / PixelsPerPoint ());
			}
			return new Vector2 (displayWidth, displayHeight);
		}


		protected Vector2 scroll;
		protected int height;
		protected int width;

		protected virtual void DrawPreviewGUI ()
		{
		}

		protected virtual void DrawToolBarGUI ()
		{
		}

		#endregion

		#region Actions

		public void UpdateAllRequiredResolutions ()
		{
			Reset ();
			InitTempManager ();
			m_ScreenshotTaker.StartCoroutine (UpdateCoroutine (GetResolutionsToUpdate ()));
		}

		protected virtual void UpdateWindowResolutions ()
		{	
			Reset ();
			InitTempManager ();
			m_ScreenshotTaker.StartCoroutine (UpdateCoroutine (m_ConfigAsset.m_Config.GetActiveResolutions (), true));
		}

		public void Export ()
		{
			m_ConfigAsset.m_Config.ExportToFiles (GetResolutionsToUpdate ());
		}

		protected static ScreenshotTaker m_ScreenshotTaker;

		protected void InitTempManager ()
		{
//			Debug.Log ("InitTempManager");

			if (m_ScreenshotTaker != null) {
				if (!Application.isPlaying) {
					m_ScreenshotTaker.Reset ();
				}
				return;
			}

			GameObject obj = new GameObject ();
			obj.name = "Temporary screenshot manager - remove if still exists in scene in edit mode.";

			// Create the screenshot taker
			m_ScreenshotTaker = obj.AddComponent<ScreenshotTaker> ();
			#if (UNITY_EDITOR) && (! UNITY_5_4_OR_NEWER)
			m_ScreenshotTaker.m_ForceLayoutPreservation = false;
			#endif
			m_ScreenshotTaker.m_GameViewResizingWaitingMode = m_ConfigAsset.m_Config.m_GameViewResizingWaitingMode;
			m_ScreenshotTaker.m_GameViewResizingWaitingFrames = m_ConfigAsset.m_Config.m_ResizingWaitingFrames;
			m_ScreenshotTaker.m_GameViewResizingWaitingTime = m_ConfigAsset.m_Config.m_ResizingWaitingTime;
		}

		protected void DestroyManager ()
		{
//			Debug.Log ("Destroy manager");

			if (m_ScreenshotTaker == null)
				return;
						
			if (Application.isPlaying)
				return;

			m_ScreenshotTaker.Reset ();
			GameObject.DestroyImmediate (m_ScreenshotTaker.gameObject);
			m_ScreenshotTaker = null;
		}

		protected IEnumerator UpdateCoroutine (List<ScreenshotResolution> resolutions, bool restoreFocus = false)
		{
			if (m_IsUpdating)
				yield break;
			m_IsUpdating = true;

			onUpdateBeginDelegate ();
			
			foreach (ScreenshotResolution res in resolutions) {

				// Capture the textures
				yield return   m_ScreenshotTaker.StartCoroutine (m_ScreenshotTaker.CaptureResolutionCoroutine (res, new List<Camera> (), new List<Canvas> (), ScreenshotTaker.CaptureMode.GAMEVIEW_RESIZING));

				// Capture the masks
				if (m_ConfigAsset.m_DrawingMode != PreviewConfigAsset.DrawingMode.TEXTURE_ONLY) {
					yield return   m_ScreenshotTaker.StartCoroutine (CaptureMasks (res));
				}
			}

			m_LastEditorUpdate = DateTime.Now;
			Repaint ();
			DestroyManager ();

			onUpdateEndDelegate ();

			m_IsUpdating = false;

		}

		Camera m_Camera;
		Canvas m_Canvas;

		protected IEnumerator CaptureMasks (ScreenshotResolution res)
		{
			if (m_ConfigAsset.m_DeviceRendererCamera == null)
				yield break;
			

			// Init camera
			m_Camera = GameObject.Instantiate (m_ConfigAsset.m_DeviceRendererCamera);
			m_Camera.clearFlags = CameraClearFlags.Color;
			m_Camera.backgroundColor = EditorGUIUtility.isProSkin 
				? new Color32 (56, 56, 56, (byte)(m_ConfigAsset.m_TransparentDeviceBackground ? 0 : 255))
				: new Color32 (194, 194, 194, (byte)(m_ConfigAsset.m_TransparentDeviceBackground ? 0 : 255));

			// Init canvas instance
			m_Canvas = res.m_DeviceCanvas;
			if (m_Canvas == null) {
				m_Canvas = m_ConfigAsset.m_DefaultDeviceCanvas;
			}
			if (m_Canvas == null)
				yield break;
			m_Canvas = GameObject.Instantiate (m_Canvas);

			// Texture
			MaskRenderer mask = m_Canvas.GetComponent<MaskRenderer> ();
			mask.m_Texture.texture = res.m_Texture;

			if (m_ConfigAsset.m_DrawingMode == PreviewConfigAsset.DrawingMode.SCREEN_MASK) {
				mask.m_Mask.rectTransform.anchoredPosition = Vector2.zero;
				mask.m_Device.gameObject.SetActive (false);
				if (mask.m_BorderTop) {
					mask.m_BorderTop.gameObject.SetActive (false);
					mask.m_BorderLeft.gameObject.SetActive (false);
					mask.m_BorderRight.gameObject.SetActive (false);
					mask.m_BorderBottom.gameObject.SetActive (false);
				}
			}

			// Scale the UI 
			Vector3 panelScale = Vector3.one;
			int widthWithDevice = res.ComputeTargetWidth ();
			int heightWithDevice = res.ComputeTargetHeight ();


			if (res.m_Orientation == ScreenshotResolution.Orientation.PORTRAIT) {

				// Resize the whole canvas
				panelScale.x = (float)res.ComputeTargetWidth () / (float)mask.m_Mask.GetComponent<RectTransform> ().rect.width;
				panelScale.y = (float)res.ComputeTargetHeight () / (float)mask.m_Mask.GetComponent<RectTransform> ().rect.height;
				mask.m_Panel.localScale = panelScale;

				// Add the border margin
				if (m_ConfigAsset.m_DrawingMode == PreviewConfigAsset.DrawingMode.FULL_DEVICE) {
					widthWithDevice = (int)(mask.m_Device.GetComponent<RectTransform> ().rect.width * panelScale.x);
					heightWithDevice = (int)(mask.m_Device.GetComponent<RectTransform> ().rect.height * panelScale.y);
				} 

			} else {
				// Rotate the whole canvas
				mask.m_Panel.localRotation = Quaternion.AngleAxis (90f, Vector3.forward);

				// Resize the whole canvas
				panelScale.x = (float)res.ComputeTargetWidth () / (float)mask.m_Mask.GetComponent<RectTransform> ().rect.height;
				panelScale.y = (float)res.ComputeTargetHeight () / (float)mask.m_Mask.GetComponent<RectTransform> ().rect.width;
				mask.m_Panel.localScale = new Vector3 (panelScale.y, panelScale.x, 1f);

				// Rotate the texture and scale it to match the screen size
				mask.m_Texture.transform.localRotation = Quaternion.AngleAxis (-90f, Vector3.forward);
				float tmp = mask.m_Texture.rectTransform.sizeDelta.x;
				mask.m_Texture.rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, mask.m_Texture.rectTransform.sizeDelta.y);
				mask.m_Texture.rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, tmp);

				// Add the border margin
				if (m_ConfigAsset.m_DrawingMode == PreviewConfigAsset.DrawingMode.FULL_DEVICE) {
					widthWithDevice = (int)(mask.m_Device.GetComponent<RectTransform> ().rect.height * panelScale.x);
					heightWithDevice = (int)(mask.m_Device.GetComponent<RectTransform> ().rect.width * panelScale.y);
				}

			}

			// Compute target resolution
			if (m_ConfigAsset.m_DrawingMode == PreviewConfigAsset.DrawingMode.SCREEN_MASK) {
				mask.m_Device.gameObject.SetActive (false);
			} else {
				mask.m_Device.gameObject.SetActive (true);
			}

			// Capture the device
			Texture2D texClone = new Texture2D (1, 1);
			yield return m_ScreenshotTaker.StartCoroutine (m_ScreenshotTaker.CaptureToTextureCoroutine (texClone, widthWithDevice, heightWithDevice, 
				new List<Camera>{ m_Camera }, new List<Canvas>{ m_Canvas }, 
				ScreenshotTaker.CaptureMode.GAMEVIEW_RESIZING, 8, false, ScreenshotTaker.ColorFormat.RGBA, false));

			// Replace the texture
			GameObject.DestroyImmediate (res.m_Texture);
			res.m_Texture = texClone;

			// Clean
			GameObject.DestroyImmediate (m_Camera.gameObject);
			GameObject.DestroyImmediate (m_Canvas.gameObject);
		}

		protected virtual List<ScreenshotResolution> GetResolutionsToUpdate ()
		{
			List<ScreenshotResolution> resolutions = new List<ScreenshotResolution> ();
						
			// Look if the preview or gallery window is open
			if (ResolutionGalleryWindow.IsOpen ()) {
				// If the gallery is open, we update all resolutions
				resolutions.AddRange (m_ConfigAsset.m_Config.GetActiveResolutions ());
						
			} else if (ResolutionPreviewWindow.IsOpen ()) {
				// If only the preview is open, we only update the selected resolution
				if (m_ConfigAsset.m_Selected < m_ConfigAsset.m_Config.GetActiveResolutions ().Count) {
					resolutions.Add (m_ConfigAsset.m_Config.GetActiveResolutions () [m_ConfigAsset.m_Selected]);
				}
			}
						
			return resolutions;
		}

		public static void RepaintWindows ()
		{
			if (ResolutionGalleryWindow.IsOpen ()) {
				ResolutionGalleryWindow.m_Window.Repaint ();
			}
			if (ResolutionPreviewWindow.IsOpen ()) {
				ResolutionPreviewWindow.m_Window.Repaint ();
			}
			if (ResolutionSettingsWindow.IsOpen ()) {
				ResolutionSettingsWindow.m_Window.Repaint ();
			}
		}


		#endregion

	}
}

#endif
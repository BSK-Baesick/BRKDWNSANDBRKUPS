using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace AlmostEngine.Screenshot
{
	/// <summary>
	/// Capture a screenshot and export it to the specified path.
	/// </summary>
	public static class SimpleScreenshotCapture
	{
		static ScreenshotTaker m_ScreenshotTaker;

		static void InitScreenshotTaker ()
		{
			if (m_ScreenshotTaker != null)
				return;
			
			if (m_ScreenshotTaker == null) {
				GameObject go = new GameObject ();
				go.name = "tmp Screenshot Capture";
				m_ScreenshotTaker = go.AddComponent<ScreenshotTaker> ();
			}
		}

		#region API

		/// <summary>
		/// Captures the current screen at its current resolution, including UI.
		/// The filename must be a valid full name.
		/// </summary>
		public static void CaptureScreen (string fileName, 
		                                  TextureExporter.ImageFileFormat fileFormat = TextureExporter.ImageFileFormat.PNG,
		                                  int JPGQuality = 100,
		                                  bool captureGameUI = true,
		                                  ScreenshotTaker.ColorFormat colorFormat = ScreenshotTaker.ColorFormat.RGB,
		                                  bool recomputeAlphaMask = false)
		{
			Vector2 size = GameViewController.GetCurrentGameViewSize ();
			Capture (fileName, (int)size.x, (int)size.y, fileFormat, JPGQuality, null, null, 
				ScreenshotTaker.CaptureMode.FIXED_GAMEVIEW, 
				8, captureGameUI, colorFormat, recomputeAlphaMask);
		}

		/// <summary>
		/// Captures the scene with the specified width, height, using the mode RENDER_TO_TEXTURE.
		/// Screenspace Overlay Canvas can not be captured with this mode.
		/// The filename must be a valid full name.
		/// </summary>
		public static void CaptureCameras (string fileName, 
		                                   int width, int height, 
		                                   List<Camera> cameras, 
		                                   TextureExporter.ImageFileFormat fileFormat = TextureExporter.ImageFileFormat.PNG,
		                                   int JPGQuality = 100,
		                                   int antiAliasing = 8,
		                                   ScreenshotTaker.ColorFormat colorFormat = ScreenshotTaker.ColorFormat.RGB,
		                                   bool recomputeAlphaMask = false)
		{
			Capture (fileName, width, height, fileFormat, JPGQuality, cameras, null, ScreenshotTaker.CaptureMode.RENDER_TO_TEXTURE, antiAliasing, false, colorFormat, recomputeAlphaMask);
		}



		/// <summary>
		/// Captures the game.
		/// The filename must be a valid full name.
		/// </summary>
		public static void Capture (string fileName,
		                            int width, int height,
		                            TextureExporter.ImageFileFormat fileFormat = TextureExporter.ImageFileFormat.PNG,
		                            int JPGQuality = 100,
		                            List<Camera> cameras = null, 
		                            List<Canvas> canvas = null, 
		                            ScreenshotTaker.CaptureMode captureMode = ScreenshotTaker.CaptureMode.RENDER_TO_TEXTURE,
		                            int antiAliasing = 8,
		                            bool captureGameUI = true,
		                            ScreenshotTaker.ColorFormat colorFormat = ScreenshotTaker.ColorFormat.RGB,
		                            bool recomputeAlphaMask = false)
		{
			InitScreenshotTaker ();
			m_ScreenshotTaker.StartCoroutine (CaptureCoroutine (fileName, width, height, fileFormat, JPGQuality, cameras, canvas, captureMode, antiAliasing, captureGameUI, colorFormat, recomputeAlphaMask));
		}

		public static IEnumerator CaptureCoroutine (string fileName,
		                                            int width, int height,
		                                            TextureExporter.ImageFileFormat fileFormat = TextureExporter.ImageFileFormat.PNG,
		                                            int JPGQuality = 100,
		                                            List<Camera> cameras = null, 
		                                            List<Canvas> canvas = null, 
		                                            ScreenshotTaker.CaptureMode captureMode = ScreenshotTaker.CaptureMode.RENDER_TO_TEXTURE,
		                                            int antiAliasing = 8,
		                                            bool captureGameUI = true,
		                                            ScreenshotTaker.ColorFormat colorFormat = ScreenshotTaker.ColorFormat.RGB,
		                                            bool recomputeAlphaMask = false)
		{
			// Create resolution item
			ScreenshotResolution captureResolution = new ScreenshotResolution ();
			captureResolution.m_Width = width;
			captureResolution.m_Height = height;
			captureResolution.m_FileName = fileName;

			// Create camera items
			List <ScreenshotCamera> screenshotCameras = new List<ScreenshotCamera> ();
			if (cameras != null) {
				foreach (Camera camera in cameras) {
					ScreenshotCamera scamera = new ScreenshotCamera (camera);
					screenshotCameras.Add (scamera);
				}
			}

			// Create the overlays items
			List <ScreenshotOverlay> screenshotCanvas = new List<ScreenshotOverlay> ();
			if (canvas != null) {
				foreach (Canvas c in canvas) {
					ScreenshotOverlay scanvas = new ScreenshotOverlay (c);
					screenshotCanvas.Add (scanvas);
				}
			}

			// Capture
			yield return m_ScreenshotTaker.StartCoroutine (m_ScreenshotTaker.CaptureAllCoroutine (new List<ScreenshotResolution>{ captureResolution }, 
				screenshotCameras, 
				screenshotCanvas, 
				captureMode, 
				antiAliasing,
				captureGameUI, 
				colorFormat, 
				recomputeAlphaMask));

			// EXPORT
			if (TextureExporter.ExportToFile (captureResolution.m_Texture, fileName, fileFormat, JPGQuality)) {
				Debug.Log ("Screenshot created : " + fileName);
			} else {
				Debug.LogError ("Failed to create : " + fileName);
			}

		}

		#endregion


	}
}




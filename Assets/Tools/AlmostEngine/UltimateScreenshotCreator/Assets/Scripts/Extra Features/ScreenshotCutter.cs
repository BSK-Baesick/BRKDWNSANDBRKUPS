using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AlmostEngine.Screenshot.Extra
{
	[ExecuteInEditMode]
	/// <summary>
	/// Can crop the screenshot using the specified RectTransform.
	/// Add it to one scene objects and set the RectTransform.	
	/// </summary>
	public class ScreenshotCutter : MonoBehaviour
	{

		public RectTransform m_SelectionArea;
		public bool m_HideSelectionAreaDuringCapture = false;
		public int m_CropBorder = 1;

		void OnEnable ()
		{
			ScreenshotTaker.onResolutionUpdateStartDelegate -= StartCallback;
			ScreenshotTaker.onResolutionUpdateStartDelegate += StartCallback;

			ScreenshotTaker.onResolutionUpdateEndDelegate -= EndCallback;
			ScreenshotTaker.onResolutionUpdateEndDelegate += EndCallback;
		}

		void OnDisable ()
		{
			ScreenshotTaker.onResolutionUpdateStartDelegate -= StartCallback;
			ScreenshotTaker.onResolutionUpdateEndDelegate -= EndCallback;
		}

		void StartCallback (ScreenshotResolution res)
		{
			if (m_SelectionArea == null)
				return;
						
			if (m_HideSelectionAreaDuringCapture) {
				Hide ();
			}
		}

		void EndCallback (ScreenshotResolution res)
		{
			if (m_SelectionArea == null)
				return;

			if (m_HideSelectionAreaDuringCapture) {
				Show ();
			}
			CropTexture (res);
		}

		bool m_WasActive = true;

		void Hide ()
		{
			m_WasActive = m_SelectionArea.gameObject.activeSelf;
			m_SelectionArea.gameObject.SetActive (false);
		}

		void Show ()
		{
			m_SelectionArea.gameObject.SetActive (m_WasActive);
		}

		void CropTexture (ScreenshotResolution res)
		{
			// Get the selection image coordinates
			Vector3[] corners = new Vector3[4];
			m_SelectionArea.GetWorldCorners (corners);

			// Create cropped texture
			int x0 = (int)corners [0].x + m_CropBorder;
			int y0 = (int)corners [0].y + m_CropBorder;
			int width = (int)(corners [2].x - corners [0].x) - 2 * m_CropBorder;
			int height = (int)(corners [1].y - corners [0].y) - 2 * m_CropBorder;
			Texture2D cropped = new Texture2D (width, height, res.m_Texture.format, false);

			if (width <= 2 || height <= 2)
				return;								

			// Copy the content
			Color col;
			for (int x = 0; x < width; ++x) {
				for (int y = 0; y < height; ++y) {
					if (x0 + x >= 0 && x0 + x < res.m_Texture.width
					    && y0 + y >= 0 && y0 + y < res.m_Texture.height) {										
						col = res.m_Texture.GetPixel (x0 + x, y0 + y);
					} else {
						col = Color.black;
					}
					cropped.SetPixel (x, y, col);
				}
			}
			cropped.Apply ();

			Debug.Log ("Screenshot cropped to (" + x0 + ", " + y0 + ", " + (x0 + width - 1) + ", " + (y0 + height - 1) + ")");

			// Replace the texture
			GameObject.DestroyImmediate(res.m_Texture);
			res.m_Texture = cropped;
		}
	}
}

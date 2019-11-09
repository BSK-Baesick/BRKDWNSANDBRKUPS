using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AlmostEngine.Screenshot.Extra
{
	[ExecuteInEditMode]
	public class RotateScreenshot : MonoBehaviour
	{
		void OnEnable ()
		{
			ScreenshotTaker.onResolutionUpdateEndDelegate -= EndCallback;
			ScreenshotTaker.onResolutionUpdateEndDelegate += EndCallback;
		}

		void OnDisable ()
		{
			ScreenshotTaker.onResolutionUpdateEndDelegate -= EndCallback;
		}

		void EndCallback (ScreenshotResolution res)
		{
			RotateTexture (res);
		}

		public static void RotateTexture (ScreenshotResolution res)
		{
			var rotated = RotateTexture (res.m_Texture);

			// Replace the texture
			DestroyImmediate (res.m_Texture);
			res.m_Texture = rotated;
		}

		public static Texture2D RotateTexture (Texture2D tex)
		{
			Texture2D rotated = new Texture2D (tex.height, tex.width, tex.format, false);

			// Copy the content
			Color col;
			for (int x = 0; x < tex.width; ++x) {
				for (int y = 0; y < tex.height; ++y) {									
					col = tex.GetPixel (tex.width - 1 - x, y);
					rotated.SetPixel (y, x, col);
				}
			}
			rotated.Apply ();
			return rotated;

		}
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using AlmostEngine.Screenshot;

namespace AlmostEngine.Examples
{
	public class CaptureCameraToTextureExample : MonoBehaviour
	{

		public ScreenshotTaker m_ScreenshotTaker;
		public RawImage m_RawImage;
		Texture2D m_TargetTexture;
		public Camera m_Camera;
		public int m_Width = 1600;
		public int m_Height = 900;

		public void Capture ()
		{
			StartCoroutine (CaptureToTexture ());
		}

		IEnumerator CaptureToTexture ()
		{
			// The texture must be initialized before calling the capture method.
			if (m_TargetTexture == null) {
				m_TargetTexture = new Texture2D (2, 2);
			}

			// We call the capture coroutine and wait for its termination
			yield return StartCoroutine (m_ScreenshotTaker.CaptureCamerasToTextureCoroutine (m_TargetTexture, m_Width, m_Height, new System.Collections.Generic.List<Camera>{ m_Camera }));

			// We apply the texture to the GUI element
			m_RawImage.texture = m_TargetTexture;
		}
	}
}


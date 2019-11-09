using UnityEngine;
using System.Collections;

using AlmostEngine.Screenshot;

namespace AlmostEngine.Preview
{
	public class PreviewConfigAsset : ScriptableObject
	{

		public ScreenshotConfig m_Config;

		[Tooltip ("Set your screen PPI value to be used by the preview Gallery.")]
		public int m_ScreenPPI = 109;

		public float m_ZoomScrollSpeed = 0.01f;

		#region UI

		public int m_MarginHorizontal = 10;
		public int m_MarginVertical = 30;


		public int m_GalleryPaddingVertical = 10;

		public int m_GalleryTextHeight = 30;
		public int m_GalleryBorderSize = 2;

		#endregion


		#region Windows settings

		public int m_Selected;

		public float m_PreviewGalleryZoom = 1f;
		public float m_PreviewZoom = 1f;

		public enum AutoRefreshMode
		{
			ONLY_IN_PLAY_MODE,
			ONLY_IN_EDIT_MODE,
			ALWAYS}
		;

		[Tooltip ("When auto refresh is enabled, the gallery and preview windows are updated automatically." +
		"The refresh can be done while editing the scene, while playing the game, or always.")]
		public AutoRefreshMode m_RefreshMode;
		public bool m_AutoRefresh = false;
		public float m_RefreshDelay = 0.05f;

		public enum GalleryDisplayMode
		{
			RATIOS,
			PIXELS,
			PPI}
		;

		public GalleryDisplayMode m_GalleryDisplayMode;
		public GalleryDisplayMode m_PreviewDisplayMode;

		#endregion

		#region Safe Area

		public enum DrawingMode
		{
			TEXTURE_ONLY,
			SCREEN_MASK,
			FULL_DEVICE}
		;

		public DrawingMode m_DrawingMode;

		public Camera m_DeviceRendererCamera;
		public Canvas m_DefaultDeviceCanvas;

		public bool m_TransparentDeviceBackground = false;

		#endregion

		#region UI

		public bool m_ShowGallery = true;
		public bool m_ExpandDevices = false;

		#endregion
	}
}

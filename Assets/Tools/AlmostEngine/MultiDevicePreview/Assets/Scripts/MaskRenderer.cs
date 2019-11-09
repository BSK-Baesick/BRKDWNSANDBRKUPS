using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AlmostEngine.Preview;
using AlmostEngine.Screenshot;



namespace AlmostEngine.Preview
{
	public class MaskRenderer : MonoBehaviour
	{
		public Canvas m_Canvas;
		public RectTransform m_Panel;
		public RawImage m_Device;
		public RawImage m_Mask;
		public RawImage m_Texture;

		public RawImage m_BorderLeft;
		public RawImage m_BorderRight;
		public RawImage m_BorderTop;
		public RawImage m_BorderBottom;

	}
}

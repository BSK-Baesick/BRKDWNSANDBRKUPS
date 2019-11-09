using UnityEngine;

namespace AlmostEngine
{
		public class UIStyle
		{

				static GUIStyle m_CenteredGreyTextStyle;

				public static GUIStyle centeredGreyTextStyle {
						get {
								if (m_CenteredGreyTextStyle == null) {
										m_CenteredGreyTextStyle = new GUIStyle ();
										m_CenteredGreyTextStyle.wordWrap = true;
										m_CenteredGreyTextStyle.alignment = TextAnchor.MiddleCenter;
										m_CenteredGreyTextStyle.fontSize = 10;
										m_CenteredGreyTextStyle.normal.textColor = Color.gray;
								}
								return m_CenteredGreyTextStyle;
						}
				}
		}
}


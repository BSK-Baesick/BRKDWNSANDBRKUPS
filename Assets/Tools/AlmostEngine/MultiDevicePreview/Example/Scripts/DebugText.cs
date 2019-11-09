using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AlmostEngine.Examples
{
	public class DebugText : MonoBehaviour {

		Text m_Text;

		void Start() {
			m_Text = GetComponent<Text> ();
		}

		void Update()
		{
			string debugText = Screen.width + "x" + Screen.height + "  Screen.dpi: " + Screen.dpi;
			m_Text.text = debugText;
		}

	}
}
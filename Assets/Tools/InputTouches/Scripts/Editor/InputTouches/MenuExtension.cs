using UnityEngine;
using UnityEditor;


public class MenuExtension : EditorWindow {

	[MenuItem ("Tools/InputTouches/Contact and Support Info", false, 100)]
	static void OpenForumLink () {
		SupportContactWindow.Init();
	}
	
}

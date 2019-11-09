using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlmostEngine.Preview
{
	public class MultiDevicePreviewGallery
	{
		public static string VERSION = "Multi Device Preview & Gallery v1.7.5";
		public static string AUTHOR = "(c)Arnaud Emilien - support@wildmagegames.com";

		#if UNITY_EDITOR
		public static void About ()
		{
			EditorUtility.DisplayDialog ("About", VERSION + "\n" + AUTHOR, "Close");
		}
		#endif
	}
}


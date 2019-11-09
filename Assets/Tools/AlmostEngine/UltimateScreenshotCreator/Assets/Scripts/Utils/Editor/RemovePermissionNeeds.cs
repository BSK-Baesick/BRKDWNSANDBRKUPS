using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace AlmostEngine.Screenshot
{
	public class RemovePermissionNeeds
	{

		public static void Remove (bool exclude = true)
		{
			if (exclude) {
				Debug.Log ("Removing permission needs for iOS and Android.");
			} else {
				Debug.Log ("Restoring permission needs for iOS and Android.");
			}

			// Plugin files
			string[] utils = AssetDatabase.FindAssets ("iOSUtils");
			for (int i = 0; i < utils.Length; ++i) {
				string path = AssetDatabase.GUIDToAssetPath (utils [i]);
				if (!path.Contains ("iOSUtils.m"))
					continue;
				string newPath = "";
				if (exclude) {
					newPath = path.Replace (".m", ".m.bk");
				} else {
					newPath = path.Replace (".bk", "");
				}
				Debug.Log ("Moving iOS plugin " + path + " to " + newPath);
				File.Move (path, newPath);
				AssetDatabase.SaveAssets ();
				AssetDatabase.Refresh ();
			}
			if (!exclude) {
				FrameworkDependency.SetiOSFrameworkDependency ();
			}

			// Symbols
			if (exclude) {
				UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.Android, UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup (BuildTargetGroup.Android) + ";IGNORE_ANDROID_SCREENSHOT");
				UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.iOS, UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup (BuildTargetGroup.iOS) + ";IGNORE_IOS_SCREENSHOT");
			} else {
				UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.Android, UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup (BuildTargetGroup.Android).Replace ("IGNORE_ANDROID_SCREENSHOT", ""));
				UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.iOS, UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup (BuildTargetGroup.iOS).Replace ("IGNORE_IOS_SCREENSHOT", ""));
			}

			Debug.Log ("Android player define symbols: " + UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup (BuildTargetGroup.Android));
			Debug.Log ("iOS player define symbols: " + UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup (BuildTargetGroup.iOS));

		}
	}
}
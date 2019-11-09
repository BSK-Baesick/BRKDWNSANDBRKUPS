using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace AlmostEngine.Screenshot
{
	public class FrameworkDependency
	{
		public static void SetiOSFrameworkDependency ()
		{
			string[] utils = AssetDatabase.FindAssets ("iOSUtils");
			if (utils.Length > 0) {
				string path = AssetDatabase.GUIDToAssetPath (utils [0]);
				FrameworkDependency.AddFrameworkDependency (path, BuildTarget.iOS, "Photos");
			}
		}

		public static void AddFrameworkDependency (string pluginPath, BuildTarget target, string framework)
		{
			PluginImporter plugin = AssetImporter.GetAtPath (pluginPath) as PluginImporter;
			if (plugin == null)
				return;
			string dependencies = plugin.GetPlatformData (target, "FrameworkDependencies");
			if (!dependencies.Contains (framework)) {
				plugin.SetPlatformData (target, "FrameworkDependencies", dependencies + ";" + framework);
				AssetDatabase.SaveAssets ();
				AssetDatabase.Refresh ();

				Debug.Log ("Adding framework dependency to " + target + ": " + framework);
			}
		}
	}

}
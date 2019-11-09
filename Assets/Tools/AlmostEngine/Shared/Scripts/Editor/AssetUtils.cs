using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AlmostEngine
{
	public class AssetUtils
	{
		public static List<T> LoadAll<T> () where T: UnityEngine.Object
		{
			string filter = "t:" + typeof(T).ToString ();
			List<string> guids = AssetDatabase.FindAssets (filter).ToList<string> ();
			return guids.Select (x => ((T)AssetDatabase.LoadAssetAtPath (AssetDatabase.GUIDToAssetPath (x), typeof(T)))).ToList<T> ();
		}

		public static T GetOrCreate<T> (string path) where T : ScriptableObject
		{
			T asset = null;
			List<T> objs = AssetUtils.LoadAll<T> ();
			if (objs.Count == 0) {
				Debug.Log ("Asset created at " + path);
				asset = ScriptableObject.CreateInstance<T> ();
				AssetDatabase.CreateAsset (asset, path);				
				AssetDatabase.SaveAssets ();
				AssetDatabase.Refresh ();
			} else {
				asset = objs [0];
			}
			return asset;
		}

	}
}


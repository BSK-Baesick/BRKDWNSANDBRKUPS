using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


namespace AlmostEngine.Screenshot
{
	public class ScreenshotResolutionPresets
	{
		public static List<ScreenshotResolution> m_ResolutionPresets = new List<ScreenshotResolution> ();

		public static Dictionary<string, List<ScreenshotResolution>> m_Categories = new Dictionary<string, List<ScreenshotResolution>> ();

		#if UNITY_EDITOR
		[MenuItem ("Tools/Ultimate Screenshot Creator/Reload all presets")]
		static void LoadAllAsPresets ()
		{
			Init ();
		}
		#endif

		public static void Init ()
		{
			m_ResolutionPresets.Clear ();

			LoadPresets ();
			LoadPopularityPresets ();
			LoadCollections ();

			UpdateCategories ();
		}


		public static void ExportPresets (List<ScreenshotResolution> resolutions)
		{
			foreach (ScreenshotResolution res in resolutions) {
				string name = res.m_ResolutionName == "" ? res.m_Width.ToString () + "x" + res.m_Height.ToString () : res.m_ResolutionName;
				ScreenshotResolutionAsset preset = ScriptableObjectUtils.CreateAsset<ScreenshotResolutionAsset> (name, "Presets/Custom");
				preset.m_Resolution = new ScreenshotResolution (res);
				EditorUtility.SetDirty (preset);
			}
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
		}


		public static void LoadPresets ()
		{
			List<ScreenshotResolutionAsset> presets = AssetUtils.LoadAll<ScreenshotResolutionAsset> ();
			foreach (ScreenshotResolutionAsset preset in presets) {
				if (preset.m_Resolution == null) {
					continue;
				}
				var res = new ScreenshotResolution (preset.m_Resolution);
				res.m_ResolutionName = preset.name.Replace (".asset", "");
				var cat = AssetDatabase.GetAssetPath (preset);
				if (cat.Contains ("Resources/")) {
					cat = cat.Substring (cat.LastIndexOf ("Resources"), cat.LastIndexOf ("/") - cat.LastIndexOf ("Resources"));
				}
				if (cat.Contains ("Presets/")) {
					cat = cat.Substring (cat.LastIndexOf ("Presets"), cat.LastIndexOf ("/") - cat.LastIndexOf ("Presets"));
				}
				cat = cat.Replace ("Presets/", "");
				cat = cat.Replace ("Resources/", "");
				res.m_Category = cat;
				m_ResolutionPresets.Add (res);
			}
		}

		public static void LoadPopularityPresets ()
		{
			List<PopularityPresetAsset> popularities = AssetUtils.LoadAll<PopularityPresetAsset> ();
			foreach (PopularityPresetAsset popularity in popularities) {
				foreach (var stat in popularity.m_Stats) {			
					if (stat == null)
						continue;		
					if (stat.m_Resolution == null)
						continue;
					var res = new ScreenshotResolution (stat.m_Resolution.m_Resolution);
					res.m_ResolutionName = stat.m_Frequency + "%   " + stat.m_Resolution.name;
					res.m_Category = "Popularity/" + popularity.name;
					m_ResolutionPresets.Add (res);
				}
			}
		}

		public static void LoadCollections ()
		{
			List<PresetCollectionAsset> collections = AssetUtils.LoadAll<PresetCollectionAsset> ();
			foreach (PresetCollectionAsset collection in collections) {
				foreach (var preset in collection.m_Presets) {		
					if (preset == null)
						continue;
					if (preset.m_Resolution == null)
						continue;
					var res = new ScreenshotResolution (preset.m_Resolution);
					res.m_ResolutionName = preset.name.Replace (".asset", "");
					res.m_Category = "Collections/" + collection.name;
					m_ResolutionPresets.Add (res);
				}
			}
		}


		public static void Add (List<ScreenshotResolution> resolutions)
		{
			m_ResolutionPresets.AddRange (resolutions);
			UpdateCategories ();
		}

		public static void Add (ScreenshotResolution resolution)
		{
			m_ResolutionPresets.Add (resolution);
			UpdateCategories ();
		}

		public static void UpdateCategories ()
		{
			m_Categories.Clear ();
			foreach (ScreenshotResolution res in m_ResolutionPresets) {

				if (!m_Categories.ContainsKey (res.m_Category)) {
					m_Categories [res.m_Category] = new List<ScreenshotResolution> ();
				}

				m_Categories [res.m_Category].Add (res);
			}
		}
	}
}
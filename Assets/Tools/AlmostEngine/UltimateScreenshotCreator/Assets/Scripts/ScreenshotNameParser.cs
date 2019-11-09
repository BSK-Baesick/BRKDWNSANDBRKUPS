using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


namespace AlmostEngine.Screenshot
{
	public class ScreenshotNameParser
	{

		public enum DestinationFolder
		{
			CUSTOM_FOLDER,
			DATA_PATH,
			PERSISTENT_DATA_PATH,
			PICTURES_FOLDER}
		;

		public static string ParseSymbols (string name, ScreenshotResolution resolution)
		{
			// Add a 0 before numbers if < 10
			if (System.DateTime.Now.Month < 10) {
				name = name.Replace ("{month}", "0{month}");
			}
			if (System.DateTime.Now.Day < 10) {
				name = name.Replace ("{day}", "0{day}");
			}
			if (System.DateTime.Now.Hour < 10) {
				name = name.Replace ("{hour}", "0{hour}");
			}
			if (System.DateTime.Now.Minute < 10) {
				name = name.Replace ("{minute}", "0{minute}");
			}
			if (System.DateTime.Now.Second < 10) {
				name = name.Replace ("{second}", "0{second}");
			}

			// Date
			name = name.Replace ("{year}", System.DateTime.Now.Year.ToString ());
			name = name.Replace ("{month}", System.DateTime.Now.Month.ToString ());
			name = name.Replace ("{day}", System.DateTime.Now.Day.ToString ());
			name = name.Replace ("{hour}", System.DateTime.Now.Hour.ToString ());
			name = name.Replace ("{minute}", System.DateTime.Now.Minute.ToString ());
			name = name.Replace ("{second}", System.DateTime.Now.Second.ToString ());

			// Dimensions
			name = name.Replace ("{width}", resolution.m_Width.ToString ());
			name = name.Replace ("{height}", resolution.m_Height.ToString ());
			name = name.Replace ("{scale}", resolution.m_Scale.ToString ());
			name = name.Replace ("{ratio}", resolution.m_Ratio).Replace (":", "_");

			// Resolution
			name = name.Replace ("{orientation}", resolution.m_Orientation.ToString ());
			name = name.Replace ("{name}", resolution.m_ResolutionName);
			name = name.Replace ("{ppi}", resolution.m_PPI.ToString ());
			name = name.Replace ("{category}", resolution.m_Category);
//			name = name.Replace ("{percent}", resolution.m_Stats.ToString ());

			return name;
		}

		public static string ParseExtension (TextureExporter.ImageFileFormat format)
		{
			if (format == TextureExporter.ImageFileFormat.PNG) {
				return ".png";
			} else {
				return ".jpg";
			}
		}

		public static string ParsePath (DestinationFolder destinationFolder, string customPath)
		{
			string path = "";
			if (destinationFolder == DestinationFolder.CUSTOM_FOLDER) {
				path = customPath;
			} else if (destinationFolder == DestinationFolder.PERSISTENT_DATA_PATH) {
				#if UNITY_EDITOR || UNITY_STANDALONE
				path = Application.persistentDataPath + "/" + customPath;
				#elif UNITY_ANDROID
								path = AndroidUtils.GetFirstAvailableMediaStorage()  + "/" + customPath;
				#else 
								path = Application.persistentDataPath + "/" +customPath;
				#endif
			} else if (destinationFolder == DestinationFolder.DATA_PATH) {
				path = Application.dataPath + "/" + customPath;
			} else if (destinationFolder == DestinationFolder.PICTURES_FOLDER) {
				#if UNITY_EDITOR || UNITY_STANDALONE
				path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyPictures) + "/" + customPath;
				#elif UNITY_ANDROID
								path = AndroidUtils.GetExternalPictureDirectory()  + "/" + customPath;
				#elif UNITY_IOS
								path = Application.persistentDataPath + "/" +customPath;
				#else 
								path = Application.persistentDataPath + "/" +customPath;
				#endif
			}

			// Add a slash if not already at the end of the folder name
			if (path.Length > 0) {
				path = path.Replace ("//", "/");
				if (path [path.Length - 1] != '/' && path [path.Length - 1] != '\\') {
					path += "/";
				}
			}

			return path;
		}


		/// <summary>
		/// Returns the parsed screenshot filename using the symbols, extensions and special folders.
		/// </summary>
		public static string ParseFileName (string screenshotName, ScreenshotResolution resolution, DestinationFolder destination, string customPath, TextureExporter.ImageFileFormat format, bool overrideFiles)
		{
			string filename = "";

			#if UNITY_EDITOR || !UNITY_WEBGL
			// Destination Folder can not be parsed in webgl
			filename += ParsePath (destination, customPath);
			#endif

			// File name
			filename += ParseSymbols (screenshotName, resolution);

			// Get the file extension
			filename += ParseExtension (format);


			// Increment the file number if a file already exist
			if (!overrideFiles) {
				return PreventOverride (filename);
			}

			return filename;
		}

		public static string PreventOverride (string fullname)
		{
			if (File.Exists (fullname)) {
				string filename = Path.GetDirectoryName (fullname) + "/" + Path.GetFileName (fullname);
				string extension = Path.GetExtension (fullname);
				int i = 1;
				while (File.Exists (filename + " (" + i.ToString () + ")" + extension)) {
					i++;
				}
				return filename + " (" + i.ToString () + ")" + extension;
			} else {
				return fullname;
			}
		}


	}
}
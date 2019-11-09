using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace AlmostEngine.Screenshot
{
	public class TextureExporter
	{

		#region Saving

		public enum ImageFileFormat
		{
			PNG,
			JPG}
		;

		public static bool CreateExportDirectory (string path)
		{
					
			// Create the folder if needed
			string fullpath = path;
			if (string.IsNullOrEmpty (fullpath)) {
				Debug.LogError ("Can not create directory, filename is null or empty.");
				return false;
			}
					
			fullpath = fullpath.Replace ("\\", "/");
					
			if (!fullpath.Contains ("/")) {
				Debug.LogError ("Can not create directory, filename is not a valid path : " + path);
				return false;
			}
					
			fullpath = fullpath.Substring (0, fullpath.LastIndexOf ('/'));
					
					
			if (!System.IO.Directory.Exists (fullpath)) {
				Debug.Log ("Creating directory " + fullpath);
				try {
					System.IO.Directory.CreateDirectory (fullpath);
				} catch {
					Debug.LogError ("Failed to create directory : " + fullpath);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Exports to file.
		/// </summary>
		/// <returns><c>true</c>, if to file was exported, <c>false</c> otherwise.</returns>
		/// <param name="texture">Texture.</param> The texture to export.
		/// <param name="filename">Filename.</param> The filename must be a valid full path. Use the ScreenshotNameParser to get a valid path.
		/// <param name="imageFormat">Image format.</param>
		/// <param name="JPGQuality">JPG quality.</param>
		public static bool ExportToFile (Texture2D texture, string filename, ImageFileFormat imageFormat, int JPGQuality = 70, bool addToGallery = true)
		{

			#if UNITY_ANDROID && !UNITY_EDITOR && IGNORE_ANDROID_SCREENSHOT
						return false;
			#endif

			#if UNITY_IOS && !UNITY_EDITOR && IGNORE_IOS_SCREENSHOT
						return false;
			#endif


			if (texture == null) {
				Debug.LogError ("Can not export the texture to file " + filename + ", texture is empty.");
				return false;
			}

			#if UNITY_WEBPLAYER

						Debug.Log("WebPlayer is not supported.");
						return false;

			#else

			// Convert texture to bytes
			byte[] bytes = null;
			if (imageFormat == ImageFileFormat.JPG) {
				bytes = texture.EncodeToJPG (JPGQuality);
			} else {
				bytes = texture.EncodeToPNG ();
			}

			#endif


			#if UNITY_WEBGL && !UNITY_EDITOR

						// Create a downloadable image for the web browser
						try {
							string shortFileName = filename;
							int index = filename.LastIndexOf('/');
							if (index >= 0) {
								shortFileName = filename.Substring(index+1);
							}
						string format = (imageFormat == ImageFileFormat.JPG) ? "jpeg" : "png";
							WebGLUtils.ExportImage(bytes, shortFileName, format);
						} catch {
							Debug.LogError ("Failed to create downloadable image.");
							return false;
						}

			#elif !UNITY_WEBPLAYER

			// Create the directory
			if (!CreateExportDirectory (filename))
				return false;

			// Export the image
			try {
				System.IO.File.WriteAllBytes (filename, bytes);
			} catch {
				Debug.LogError ("Failed to create the file : " + filename);
				return false;
			}

			#endif

			if (addToGallery) {
				#if UNITY_ANDROID && !UNITY_EDITOR

							// Update android gallery
							try {
								AndroidUtils.AddImageToGallery(filename);
							} catch {
								Debug.LogError ("Failed to update Android Gallery");
								return false;
							}

				#elif UNITY_IOS && !UNITY_EDITOR

							// Update ios gallery
							try {
								iOsUtils.AddImageToGallery(filename);
							} catch {
								Debug.LogError ("Failed to update iOS Gallery");
								return false;
							}
				#endif
			}



			#if !UNITY_WEBPLAYER
			return true;
			#endif
		
		
		}

		#endregion

		#region Loading

		public static Texture2D LoadFromFile (string fullname)
		{
			if (!System.IO.File.Exists (fullname)) {
				Debug.LogError ("Can not load texture from file " + fullname + ", file does not exists.");
				return null;
			}

			byte[] bytes = System.IO.File.ReadAllBytes (fullname);
			Texture2D texture = new Texture2D (2, 2);
			if (!texture.LoadImage (bytes)) {
				Debug.LogError ("Failed to load the texture " + fullname + ".");
			}

			return texture;

		}

		[System.Serializable]
		public class ImageFile
		{
			public Texture2D m_Texture;
			public string m_Name;
			public string m_Fullname;
			public System.DateTime m_CreationDate;
		}

		public static List<ImageFile> LoadFromPath (string path)
		{

			List<ImageFile> images = new List<ImageFile> ();

			if (!System.IO.Directory.Exists (path)) {
				Debug.LogError ("Can not load images from directory " + path + ", directory does not exists.");
				return images;
			}

			DirectoryInfo directory = new DirectoryInfo (path);
			FileInfo[] files = directory.GetFiles ();
			foreach (FileInfo file in files) {
				if (file.Extension == ".jpg" || file.Extension == ".png") {

					ImageFile item = new ImageFile ();
					item.m_Name = file.Name;
					item.m_Fullname = file.FullName;
					item.m_CreationDate = file.CreationTime;
					item.m_Texture = TextureExporter.LoadFromFile (file.FullName);

					images.Add (item);
				}
			}

			return images;
		}

		#endregion
	}
}

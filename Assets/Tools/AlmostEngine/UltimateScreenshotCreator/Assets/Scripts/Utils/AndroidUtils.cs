#if UNITY_ANDROID

using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


using AlmostEngine.Examples;

namespace AlmostEngine.Screenshot
{
	public class AndroidUtils
	{
		/// <summary>
		/// Returns the external picture directory.
		/// If the primary storage is not available or the permission to access the external storage is not granted,
		/// the method returns the first available media storage.
		/// </summary>
		/// <returns>The external picture directory.</returns>
		public static string GetExternalPictureDirectory ()
		{
			if (IsPrimaryStorageAvailable () && HasPermissionToAccessExternalStorage ()) {
				return GetPrimaryStorage () + "/" + GetPictureFolder ();
			} else {
				return GetFirstAvailableMediaStorage ();
			}
		}

		public static bool IsPrimaryStorageAvailable ()
		{
#if !IGNORE_ANDROID_SCREENSHOT
			try {
				// Get storage state
				AndroidJavaClass environment = new AndroidJavaClass ("android.os.Environment");
				string state = environment.CallStatic<string> ("getExternalStorageState");

				/*
				AndroidJavaClass environment = new AndroidJavaClass ("android.os.Environment");
				IntPtr getExternalStorageStateMethod = AndroidJNI.GetStaticMethodID (environment.GetRawClass (), "getExternalStorageState", "()Ljava/lang/String;");
				IntPtr statePtr = AndroidJNI.CallStaticObjectMethod (environment.GetRawClass (), getExternalStorageStateMethod, new jvalue[] { });
				string state = AndroidJNI.GetStringUTFChars (statePtr);
				// Clean
				AndroidJNI.DeleteLocalRef (statePtr);
				*/


				if (state == "mounted")
					return true;

			} catch (System.Exception e) {
				Debug.LogError ("AndroidUtils: Error getting external storage state: " + e.Message);
}
#endif
			return false;
		}

		public static string GetPrimaryStorage ()
		{
#if !IGNORE_ANDROID_SCREENSHOT
			try {
				AndroidJavaClass environment = new AndroidJavaClass ("android.os.Environment");
				AndroidJavaObject file = environment.CallStatic<AndroidJavaObject> ("getExternalStorageDirectory");
				string path = file.Call<string> ("getPath");

				/*
				// Get File
				AndroidJavaClass environment = new AndroidJavaClass ("android.os.Environment");
				IntPtr getExternalStorageDirectoryMethod = AndroidJNI.GetStaticMethodID (environment.GetRawClass (), "getExternalStorageDirectory", "()Ljava/io/File;");
				IntPtr filePtr = AndroidJNI.CallStaticObjectMethod (environment.GetRawClass (), getExternalStorageDirectoryMethod, new jvalue[] { });
				// Get path string from File class
				IntPtr getPathMethod = AndroidJNI.GetMethodID (AndroidJNI.GetObjectClass (filePtr), "getPath", "()Ljava/lang/String;");
				IntPtr pathPtr = AndroidJNI.CallObjectMethod (filePtr, getPathMethod, new jvalue[] { });
				string path = AndroidJNI.GetStringUTFChars (pathPtr);
				// Clean
				AndroidJNI.DeleteLocalRef (filePtr);
				AndroidJNI.DeleteLocalRef (pathPtr);
				*/


				return path;

			} catch (System.Exception e) {
				Debug.LogError ("AndroidUtils: Error getting primary external storage directory: " + e.Message);
			}
#endif
			return "";
		}


		public static string GetFirstAvailableMediaStorage ()
		{
#if !IGNORE_ANDROID_SCREENSHOT
			List<string> secondaryStorages = GetAvailableSecondaryStorages ();
			if (secondaryStorages.Count > 0)
				return secondaryStorages [0];
#endif
			// Fallback
			Debug.LogWarning ("No media storage available, using persistentDataPath as fallback");
			return Application.persistentDataPath;
		}

		/// <summary>
		/// Gets all secondary storages and return only available ones.
		/// </summary>
		public static List<string> GetAvailableSecondaryStorages ()
		{

			List<string> storages = new List<string> ();
#if !IGNORE_ANDROID_SCREENSHOT


			try {
				// Get methods
				AndroidJavaClass classPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
				AndroidJavaObject objActivity = classPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
				IntPtr getExternalMediaDirsMethod = AndroidJNI.GetMethodID (objActivity.GetRawClass (), "getExternalMediaDirs", "()[Ljava/io/File;");
				//				IntPtr obj_context = AndroidJNI.FindClass ("android/content/ContextWrapper");
				//				IntPtr getExternalMediaDirsMethod = AndroidJNIHelper.GetMethodID (obj_context, "getExternalMediaDirs", "()[Ljava/io/File;");

				// Get files array
				IntPtr filesPtr = AndroidJNI.CallObjectMethod (objActivity.GetRawObject (), getExternalMediaDirsMethod, new jvalue[0]);

				// Get files from array
				IntPtr[] files = AndroidJNI.FromObjectArray (filesPtr);

				// Parse files
				for (int i = 0; i < files.Length; ++i) {

					// Get file path
					IntPtr getPathMethod = AndroidJNI.GetMethodID (AndroidJNI.GetObjectClass (files [i]), "getPath", "()Ljava/lang/String;");
					IntPtr pathPtr = AndroidJNI.CallObjectMethod (files [i], getPathMethod, new jvalue[] { });

					string path = AndroidJNI.GetStringUTFChars (pathPtr);
					AndroidJNI.DeleteLocalRef (pathPtr);

					// Check path is available
					AndroidJavaClass environment = new AndroidJavaClass ("android.os.Environment");
					IntPtr getExternalStorageStateMethod = AndroidJNI.GetStaticMethodID (environment.GetRawClass (), "getExternalStorageState", "(Ljava/io/File;)Ljava/lang/String;");
					jvalue[] args = new jvalue[1];
					args [0].l = files [i];
					IntPtr statePtr = AndroidJNI.CallStaticObjectMethod (environment.GetRawClass (), getExternalStorageStateMethod, args);
					string state = AndroidJNI.GetStringUTFChars (statePtr);
					AndroidJNI.DeleteLocalRef (statePtr);

					if (state == "mounted") {
						storages.Add (path);
					}
				}


			} catch (System.Exception e) {
				Debug.LogError ("AndroidUtils: Error getting secondary external storage directory: " + e.Message);
			}
#endif

			return storages;

		}

		public static string GetPictureFolder ()
		{
#if !IGNORE_ANDROID_SCREENSHOT
			return GetDirectoryName ("DIRECTORY_PICTURES");
#else
			return "";
#endif
		}

		public static string GetDirectoryName (string directoryType = "DIRECTORY_PICTURES")
		{
#if !IGNORE_ANDROID_SCREENSHOT
			AndroidJavaClass environment = new AndroidJavaClass ("android.os.Environment");
			return environment.GetStatic<string> (directoryType);
#else
			return "";
#endif
			/*
			AndroidJavaClass environment = new AndroidJavaClass ("android.os.Environment");
			IntPtr fieldID = AndroidJNI.GetStaticFieldID (environment.GetRawClass (), directoryType, "Ljava/lang/String;");
			return AndroidJNI.GetStaticStringField (environment.GetRawClass (), fieldID);
			*/
		}


		/// <summary>
		/// Determines if has access to external storage.
		/// </summary>
		/// <returns><c>true</c> if has access to external storage; otherwise, <c>false</c>.</returns>
		public static bool HasPermissionToAccessExternalStorage ()
		{
#if !IGNORE_ANDROID_SCREENSHOT
			// checkSelfPermission not available for sdk < 23
			AndroidJavaClass version = new AndroidJavaClass ("android.os.Build$VERSION");
			if (version.GetStatic<int> ("SDK_INT") < 23) {
				return true;
			}

			return HasPermission ("android.permission.WRITE_EXTERNAL_STORAGE");
#else
			return false;
#endif
		}

		/*
		public static void RequestPermissionToAccessExternalStorage ()
		{
			RequestPermissions (new List<string>{ "android.permission.WRITE_EXTERNAL_STORAGE" });
		}
		*/


		public static bool HasPermission (string permissionName)
		{
#if !IGNORE_ANDROID_SCREENSHOT

			AndroidJavaClass classPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			AndroidJavaObject objActivity = classPlayer.GetStatic<AndroidJavaObject> ("currentActivity");	
			int permission = objActivity.Call<int> ("checkSelfPermission", permissionName);
			return (permission == 0);


			/*
			AndroidJavaClass classPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			AndroidJavaObject objActivity = classPlayer.GetStatic<AndroidJavaObject> ("currentActivity");	
			IntPtr checkSelfPermissionMethod = AndroidJNI.GetMethodID (objActivity.GetRawClass (), "checkSelfPermission", "(Ljava/lang/String;)I");

			jvalue[] args = new jvalue[1];
			args [0].l = AndroidJNI.NewStringUTF (permissionName);

			int permission = AndroidJNI.CallIntMethod (objActivity.GetRawObject (), checkSelfPermissionMethod, args);
			*/

#else
			return false;
#endif


		}


		/*
		public static void RequestPermissions (List<string> permissionNames)
		{
			AndroidJavaClass classPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			AndroidJavaObject objActivity = classPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
//			IntPtr checkSelfPermissionMethod = AndroidJNI.GetMethodID (objActivity.GetRawClass (), "checkSelfPermission", "(Ljava/lang/String;)I");
			IntPtr requestPermissionsMethod = AndroidJNI.GetMethodID (objActivity.GetRawClass (), "requestPermissions", "([Ljava/lang/String;I)V");
//			IntPtr requestPermissionsMethod = AndroidJNIHelper.GetMethodID (objActivity.GetRawClass (), "requestPermissions", "([Ljava/lang/String;I)V", true);






			List<IntPtr> strings = new List<IntPtr> ();
			foreach (string s in permissionNames) {
				strings.Add (AndroidJNI.NewStringUTF (s));
			}

			jvalue[] args = new jvalue[2];
			args [0].l = AndroidJNI.ToObjectArray (strings.ToArray ());
			args [1].i = 1;

//						AndroidJNI.CallObjectMethod (objActivity.GetRawObject (), requestPermissionsMethod, args); 
			AndroidJNI.CallVoidMethod (objActivity.GetRawObject (), requestPermissionsMethod, args);
		}
		*/


		/// <summary>
		/// Call the Media Scanner to add the media to the gallery
		/// </summary>
		public static void AddImageToGallery (string file)
		{
#if !IGNORE_ANDROID_SCREENSHOT
			AndroidJavaClass classPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			AndroidJavaObject objActivity = classPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
			AndroidJavaClass classMedia = new AndroidJavaClass ("android.media.MediaScannerConnection");
			classMedia.CallStatic ("scanFile", new object[4] { objActivity, new string[] { file }, null, null });
#endif
		}

	}
}

#endif
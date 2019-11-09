using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using System.IO;

namespace AlmostEngine.Screenshot
{
	public class ScreenshotConfigDrawer
	{
		public ReorderableList m_ResolutionReorderableList;
		public ReorderableList m_OverlayReorderableList;
		public ReorderableList m_CameraReorderableList;

		SerializedProperty m_DestinationFolder;
		SerializedProperty m_FileFormat;
		SerializedProperty m_ColorFormat;
		SerializedProperty m_RecomputeAlphaLayer;
		SerializedProperty m_JPGQuality;
		SerializedProperty m_CaptureMode;
		SerializedProperty m_AntiAliasing;
		SerializedProperty m_Cameras;
		SerializedProperty m_CameraMode;
		SerializedProperty m_ExportToDifferentLayers;
		SerializedProperty m_Resolutions;
		SerializedProperty m_ResolutionCaptureMode;
		SerializedProperty m_Overlays;
		SerializedProperty m_CaptureActiveUICanvas;
		SerializedProperty m_PreviewInGameViewWhilePlaying;
		SerializedProperty m_ShowGuidesInPreview;
		SerializedProperty m_GuideCanvas;
		SerializedProperty m_GuidesColor;
		SerializedProperty m_ShowPreview;
		SerializedProperty m_PreviewSize;
		SerializedProperty m_ShotMode;
		SerializedProperty m_MaxBurstShotsNumber;
		SerializedProperty m_ShotTimeStep;
		SerializedProperty m_PlaySoundOnCapture;
		SerializedProperty m_ShotSound;
		SerializedProperty m_StopTimeOnCapture;
		SerializedProperty m_OverrideFiles;

		ScreenshotConfig m_Config;
		SerializedObject serializedObject;

		Object m_Obj;

		public bool m_IsDevice = false;
		public bool m_Expanded = false;


		public void Init (SerializedObject s, Object obj, ScreenshotConfig config, SerializedProperty configProperty)
		{
			serializedObject = s;
			m_Obj = obj;
			m_Config = config;

			m_DestinationFolder = configProperty.FindPropertyRelative ("m_DestinationFolder");
			m_FileFormat = configProperty.FindPropertyRelative ("m_FileFormat");
			m_ColorFormat = configProperty.FindPropertyRelative ("m_ColorFormat");
			m_RecomputeAlphaLayer = configProperty.FindPropertyRelative ("m_RecomputeAlphaLayer");
			m_JPGQuality = configProperty.FindPropertyRelative ("m_JPGQuality");
			m_CaptureMode = configProperty.FindPropertyRelative ("m_CaptureMode");
			m_AntiAliasing = configProperty.FindPropertyRelative ("m_MultisamplingAntiAliasing");
			m_Cameras = configProperty.FindPropertyRelative ("m_Cameras");
			m_CameraMode = configProperty.FindPropertyRelative ("m_CameraMode");
			m_ExportToDifferentLayers = configProperty.FindPropertyRelative ("m_ExportToDifferentLayers");
			m_Resolutions = configProperty.FindPropertyRelative ("m_Resolutions");
			m_ResolutionCaptureMode = configProperty.FindPropertyRelative ("m_ResolutionCaptureMode");
			m_Overlays = configProperty.FindPropertyRelative ("m_Overlays");
			m_CaptureActiveUICanvas = configProperty.FindPropertyRelative ("m_CaptureActiveUICanvas");
			m_PreviewInGameViewWhilePlaying = configProperty.FindPropertyRelative ("m_PreviewInGameViewWhilePlaying");
			m_ShowGuidesInPreview = configProperty.FindPropertyRelative ("m_ShowGuidesInPreview");
			m_GuideCanvas = configProperty.FindPropertyRelative ("m_GuideCanvas");
			m_GuidesColor = configProperty.FindPropertyRelative ("m_GuidesColor");
			m_ShowPreview = configProperty.FindPropertyRelative ("m_ShowPreview");
			m_PreviewSize = configProperty.FindPropertyRelative ("m_PreviewSize");
			m_ShotMode = configProperty.FindPropertyRelative ("m_ShotMode");
			m_MaxBurstShotsNumber = configProperty.FindPropertyRelative ("m_MaxBurstShotsNumber");
			m_ShotTimeStep = configProperty.FindPropertyRelative ("m_ShotTimeStep");
			m_PlaySoundOnCapture = configProperty.FindPropertyRelative ("m_PlaySoundOnCapture");
			m_ShotSound = configProperty.FindPropertyRelative ("m_ShotSound");
			m_StopTimeOnCapture = configProperty.FindPropertyRelative ("m_StopTimeOnCapture");
			m_OverrideFiles = configProperty.FindPropertyRelative ("m_OverrideFiles");



			CreateResolutionReorderableList ();
			CreateOverlayList ();
			CreateCameraReorderableList ();

		}


		#region FOLDERS

		string newPath = "";

		public void DrawFolderGUI ()
		{
			// Title
			m_Config.m_ShowDestination = EditorGUILayout.Foldout (m_Config.m_ShowDestination, "Destination".ToUpper ());
			if (m_Config.m_ShowDestination == false)
				return;
			EditorGUILayout.Separator ();

			// Select destination type
			EditorGUILayout.PropertyField (m_DestinationFolder);

			// Path
			if (m_Config.m_DestinationFolder == ScreenshotNameParser.DestinationFolder.CUSTOM_FOLDER) {

				EditorGUILayout.BeginHorizontal ();

				// Path
				newPath = EditorGUILayout.TextField (m_Config.m_RootedPath);
				if (newPath != m_Config.m_RootedPath) {
					m_Config.m_RootedPath = newPath;
					EditorUtility.SetDirty (m_Obj);
				}

				// Browse button
				if (GUILayout.Button ("Browse", GUILayout.MaxWidth (70))) {
					newPath = EditorUtility.OpenFolderPanel ("Select destionation folder", m_Config.m_RootedPath, m_Config.m_RootedPath);
					if (newPath != m_Config.m_RootedPath) {
						m_Config.m_RootedPath = newPath;
						EditorUtility.SetDirty (m_Obj);

						// Dirty hack
						// The TextField is conflicting with the browse field:
						// if the textfield is selected then it will not be updated after the folder selection.
						GUI.FocusControl ("");
					}
				}
				EditorGUILayout.EndHorizontal ();

			} else {
				EditorGUILayout.BeginHorizontal ();

				// Path
				newPath = EditorGUILayout.TextField (m_Config.m_RelativePath);
				if (newPath != m_Config.m_RelativePath) {
					m_Config.m_RelativePath = newPath;
					EditorUtility.SetDirty (m_Obj);
				}

				EditorGUILayout.EndHorizontal ();
			}


			if (GUILayout.Button ("Open Folder")) {
				EditorUtility.RevealInFinder (m_Config.GetPath ());
			}

			// Warning message
			if (!PathUtils.IsValidPath (m_Config.GetPath ())) {
				EditorGUILayout.HelpBox ("Path \"" + m_Config.GetPath () + "\" is invalid.", MessageType.Warning);
			}

		}

		#endregion



		#region NAMES

		void OnNameSelectCallback (object target)
		{
			m_Config.m_FileName = (string)target;

			// Dirty hack
			// The TextField is conflicting with the browse field:
			// if the textfield is selected then it will not be updated after the folder selection.
			GUI.FocusControl ("");
		}

		string fullName = "";

		public void DrawNameGUI ()
		{
			//Title
			m_Config.m_ShowName = EditorGUILayout.Foldout (m_Config.m_ShowName, "File Name".ToUpper ());
			if (m_Config.m_ShowName == false)
				return;
			EditorGUILayout.Separator ();


			// Name
			EditorGUILayout.BeginHorizontal ();

			newPath = EditorGUILayout.TextField (m_Config.m_FileName);
			if (newPath != m_Config.m_FileName) {
				m_Config.m_FileName = newPath;
				EditorUtility.SetDirty (m_Obj);
			}

			// Create Name Examples Menu
			if (GUILayout.Button ("Presets", GUILayout.MaxWidth (70))) {
				var menu = new GenericMenu ();
				foreach (ScreenshotNamePresets.NamePreset path in ScreenshotNamePresets.m_NamePresets) {

					menu.AddItem (new GUIContent (path.m_Description), false, OnNameSelectCallback, path.m_Path);
				}
				menu.ShowAsContext ();
			}

			EditorGUILayout.EndHorizontal ();


			if (m_Config.m_Resolutions.Count > 0) {
				fullName = m_Config.ParseFileName (m_Config.m_Resolutions [0]);
			} else {
				fullName = m_Config.ParseFileName (m_Config.m_GameViewResolution);
			}
			if (m_Config.m_FileName == "" || PathUtils.IsValidPath (m_Config.GetPath ()) && !PathUtils.IsValidPath (fullName)) {
				EditorGUILayout.HelpBox ("Name is invalid.", MessageType.Warning);
			}

			// Override


			EditorGUILayout.PropertyField (m_OverrideFiles);


			// Format			
//			if (m_Config.m_CaptureMode != ScreenshotTaker.CaptureMode.FIXED_GAMEVIEW) {
			EditorGUILayout.PropertyField (m_FileFormat);
			if (m_Config.m_FileFormat == TextureExporter.ImageFileFormat.JPG) {
				EditorGUI.indentLevel++;
				EditorGUILayout.Slider (m_JPGQuality, 1f, 100f);
				EditorGUI.indentLevel--;
			} else {
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField (m_ColorFormat);

				if (m_Config.m_ColorFormat == ScreenshotTaker.ColorFormat.RGBA) {
					EditorGUILayout.PropertyField (m_RecomputeAlphaLayer);
				}

				EditorGUI.indentLevel--;
			}
//			}


			EditorGUILayout.Separator ();
			EditorGUILayout.Separator ();

			// Display full name
			EditorGUILayout.LabelField ("Full name: " + fullName, EditorStyles.miniLabel);

		}

		#endregion


		#region CAPTURE

		public void DrawCaptureModeGUI ()
		{
			EditorGUILayout.PropertyField (m_CaptureMode);


			if (m_Config.m_CaptureMode == ScreenshotTaker.CaptureMode.GAMEVIEW_RESIZING) {
				EditorGUILayout.HelpBox ("GAMEVIEW_RESIZING is for Editor and Windows Standalone only, can capture the UI, can capture custom resolutions.", MessageType.Info);
			} else if (m_Config.m_CaptureMode == ScreenshotTaker.CaptureMode.RENDER_TO_TEXTURE) {
				EditorGUILayout.HelpBox ("RENDER_TO_TEXTURE is for Editor and all platforms, can not capture the UI, can capture custom resolutions.", MessageType.Info);
			} else if (m_Config.m_CaptureMode == ScreenshotTaker.CaptureMode.FIXED_GAMEVIEW) {
				EditorGUILayout.HelpBox ("FIXED_GAMEVIEW is for Editor and all platforms, can capture the UI, can only capture at the screen resolution.", MessageType.Info);
			}

			if (m_Config.m_CaptureMode == ScreenshotTaker.CaptureMode.RENDER_TO_TEXTURE) {
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField (m_AntiAliasing);
				EditorGUI.indentLevel--;

				if (m_Config.m_MultisamplingAntiAliasing != ScreenshotConfig.AntiAliasing.NONE) {
					bool incompatibility = false;
					foreach (ScreenshotCamera camera in m_Config.m_Cameras) {
						if (camera.m_Camera == null)
							continue;
						#if UNITY_5_6_OR_NEWER
						if (camera.m_Camera.allowHDR) {
						#else 
						if (camera.m_Camera.hdr) {
							#endif
							incompatibility = true;														
						}
					}
					if (incompatibility) {
						EditorGUILayout.HelpBox ("It is impossible to use MultiSampling Antialiasing when one or more camera is using HDR.", MessageType.Warning);
					}
				}

				if (!UnityVersion.HasPro ()) {
					EditorGUILayout.HelpBox ("RENDER_TO_TEXTURE requires Unity Pro or Unity 5.0 and later.", MessageType.Error);
				}
			}

		}

		#endregion

		#region CAMERA

		void CreateCameraReorderableList ()
		{
			m_CameraReorderableList = new ReorderableList (serializedObject, m_Cameras, true, true, true, true);
			m_CameraReorderableList.drawElementCallback = (Rect position, int index, bool active, bool focused) => {
				SerializedProperty element = m_CameraReorderableList.serializedProperty.GetArrayElementAtIndex (index);
				EditorGUI.PropertyField (position, element);
			};
			m_CameraReorderableList.drawHeaderCallback = (Rect position) => {
				EditorGUI.LabelField (position, "Active             Camera                                                  Settings");
			};
			m_CameraReorderableList.onAddCallback = (ReorderableList list) => {
				m_Config.m_Cameras.Add (new ScreenshotCamera ());
				EditorUtility.SetDirty (m_Obj);
			};
			m_CameraReorderableList.elementHeight = 8 * 20;
		}

		public void DrawCamerasGUI ()
		{
			// Title
			m_Config.m_ShowCameras = EditorGUILayout.Foldout (m_Config.m_ShowCameras, "Cameras".ToUpper ());
			if (m_Config.m_ShowCameras == false)
				return;



			if (m_Config.m_CaptureMode == ScreenshotTaker.CaptureMode.RENDER_TO_TEXTURE) {
				m_Config.m_CameraMode = ScreenshotConfig.CamerasMode.CUSTOM_CAMERAS;
			} else {
				EditorGUILayout.PropertyField (m_CameraMode);
			}

			if (m_Config.m_CameraMode == ScreenshotConfig.CamerasMode.CUSTOM_CAMERAS) {

				EditorGUILayout.PropertyField (m_ExportToDifferentLayers);
				EditorGUILayout.Separator ();



				// List
				m_CameraReorderableList.DoLayoutList ();

				EditorGUILayout.HelpBox ("Note that you only can reference cameras within the scene.", MessageType.Info);



			} 

		}


		#endregion

		#region RESOLUTIONS


		void DrawResolution (Rect position, SerializedProperty property)
		{
			Rect activeRect = new Rect (position.x, position.y, 20, EditorGUIUtility.singleLineHeight);
			EditorGUI.PropertyField (activeRect, property.FindPropertyRelative ("m_Active"), GUIContent.none);


			string name = property.FindPropertyRelative ("m_ResolutionName").stringValue;
			if (name == "") {
				name = "no name";
			}

			activeRect.x += activeRect.width + 0;
			activeRect.width = 300;
			EditorGUI.LabelField (activeRect, name, EditorStyles.boldLabel);
		}



		void DrawResolutionName (Rect position, SerializedProperty property)
		{
			Rect activeRect = new Rect (position.x, position.y + 2 + (EditorGUIUtility.singleLineHeight + 2) * 1, 20, EditorGUIUtility.singleLineHeight);


			activeRect.x += activeRect.width + 0;
			activeRect.width = 75;
			EditorGUI.LabelField (activeRect, "Name");


			activeRect.x += activeRect.width + 2;
			int space = (int)activeRect.x;
			Rect nameRect = new Rect (space, activeRect.y, (position.width + 40 - space) / 2, 18);
			EditorGUI.PropertyField (nameRect, property.FindPropertyRelative ("m_ResolutionName"), GUIContent.none);

			Rect categoryRect = new Rect (space + 8 + (position.width + 40 - space) / 2, activeRect.y, (position.width - space) / 2, 18);
			EditorGUI.PropertyField (categoryRect, property.FindPropertyRelative ("m_Category"), GUIContent.none);

		}



		void DrawResolutionRes (Rect position, SerializedProperty property)
		{
			Rect activeRect = new Rect (position.x, position.y + 2 + (EditorGUIUtility.singleLineHeight + 2) * 2, 20, EditorGUIUtility.singleLineHeight);

			activeRect.x += activeRect.width + 0;
			activeRect.width = 75;
			EditorGUI.LabelField (activeRect, "Resolution");


			activeRect.x += activeRect.width + 2;
			activeRect.width = 45;
			EditorGUI.PropertyField (activeRect, property.FindPropertyRelative ("m_Width"), GUIContent.none);

			activeRect.x += activeRect.width + 2;
			activeRect.width = 10;
			EditorGUI.PrefixLabel (activeRect, new GUIContent ("x"));

			activeRect.x += activeRect.width + 2;
			activeRect.width = 45;
			EditorGUI.PropertyField (activeRect, property.FindPropertyRelative ("m_Height"), GUIContent.none);

			activeRect.x += activeRect.width + 4;
			activeRect.width = 40;
			EditorGUI.LabelField (activeRect, "Scale");

			activeRect.x += activeRect.width + 4;
			activeRect.width = 20;
			EditorGUI.PropertyField (activeRect, property.FindPropertyRelative ("m_Scale"), GUIContent.none);


			activeRect.x += activeRect.width + 4;
			activeRect.width = 50;
			EditorGUI.PrefixLabel (activeRect, new GUIContent (property.FindPropertyRelative ("m_Ratio").stringValue));
		}



		void DrawResolutionOrientation (Rect position, SerializedProperty property)
		{

			Rect activeRect = new Rect (position.x + 20, position.y + 2 + (EditorGUIUtility.singleLineHeight + 2) * 3, 20, EditorGUIUtility.singleLineHeight);

			activeRect.width = 75;
			EditorGUI.LabelField (activeRect, "Orientation");

			activeRect.x += activeRect.width + 2;
			activeRect.width = 150;
			EditorGUI.PropertyField (activeRect, property.FindPropertyRelative ("m_Orientation"), GUIContent.none);

		}

		void DrawDeviceResolutionPPI (Rect position, SerializedProperty property)
		{

			Rect activeRect = new Rect (position.x + 20, position.y + 2 + (EditorGUIUtility.singleLineHeight + 2) * 4, 20, EditorGUIUtility.singleLineHeight);

			activeRect.width = 75;
			EditorGUI.LabelField (activeRect, "PPI");

			activeRect.x += activeRect.width + 2;
			activeRect.width = 30;
			EditorGUI.PropertyField (activeRect, property.FindPropertyRelative ("m_PPI"), GUIContent.none);


			activeRect.x += activeRect.width + 4;
			activeRect.width = 75;
			EditorGUI.LabelField (activeRect, "forced PPI");


			activeRect.x += activeRect.width + 4;
			activeRect.width = 40;
			EditorGUI.PropertyField (activeRect, property.FindPropertyRelative ("m_ForcedUnityPPI"), GUIContent.none);

		}

		void DrawDeviceCanvasResolution (Rect position, SerializedProperty property)
		{

			Rect activeRect = new Rect (position.x + 20, position.y + 2 + (EditorGUIUtility.singleLineHeight + 2) * 5, 20, EditorGUIUtility.singleLineHeight);

			activeRect.width = 75;
			EditorGUI.LabelField (activeRect, "Device");

			activeRect.x += activeRect.width + 2;
			activeRect.width = 170;
			EditorGUI.PropertyField (activeRect, property.FindPropertyRelative ("m_DeviceCanvas"), GUIContent.none);
//
//
//			activeRect.x = position.x + 20;
//			activeRect.y += EditorGUIUtility.singleLineHeight + 2;
//			activeRect.width = 75;
//			EditorGUI.LabelField (activeRect, "Device");
//
//			activeRect.x += activeRect.width + 2;
//			activeRect.width = 170;
//			EditorGUI.PropertyField (activeRect, property.FindPropertyRelative ("m_Device"), GUIContent.none);
//
//
//
//
//
//			activeRect.x = position.x + 20;
//			activeRect.y += EditorGUIUtility.singleLineHeight + 2;
//			activeRect.width = 75;
//			EditorGUI.LabelField (activeRect, "Mask");
//
//			activeRect.x += activeRect.width + 2;
//			activeRect.width = 170;
//			EditorGUI.PropertyField (activeRect, property.FindPropertyRelative ("m_Mask"), GUIContent.none);


//			int space = (int)activeRect.x;
//			Rect nameRect = new Rect (space, activeRect.y, (position.width + 40 - space) / 2, 18);
//			EditorGUI.PropertyField (nameRect, property.FindPropertyRelative ("m_DeviceCanvas"), GUIContent.none);


//			Rect categoryRect = new Rect (space + 8 + (position.width + 40 - space) / 2, activeRect.y, (position.width - space) / 2, 18);
//			EditorGUI.PropertyField (categoryRect, property.FindPropertyRelative ("m_LandscapeDeviceCanvasLandscape"), GUIContent.none);

		}


		void CreateResolutionReorderableList ()
		{
			m_ResolutionReorderableList = new ReorderableList (serializedObject, m_Resolutions, true, true, true, true);

			if (m_IsDevice	&& m_Expanded) {
				m_ResolutionReorderableList.elementHeight = EditorGUIUtility.singleLineHeight * 8f;
			} else {
				m_ResolutionReorderableList.elementHeight = EditorGUIUtility.singleLineHeight * 1f;
			}



			m_ResolutionReorderableList.drawElementCallback = (Rect position, int index, bool active, bool focused) => {


				if (m_IsDevice) {
					DrawResolution (position, m_ResolutionReorderableList.serializedProperty.GetArrayElementAtIndex (index));
					if (m_Expanded) {
						DrawResolutionName (position, m_ResolutionReorderableList.serializedProperty.GetArrayElementAtIndex (index));
						DrawResolutionRes (position, m_ResolutionReorderableList.serializedProperty.GetArrayElementAtIndex (index));
						DrawDeviceResolutionPPI (position, m_ResolutionReorderableList.serializedProperty.GetArrayElementAtIndex (index));
						DrawResolutionOrientation (position, m_ResolutionReorderableList.serializedProperty.GetArrayElementAtIndex (index));
						DrawDeviceCanvasResolution (position, m_ResolutionReorderableList.serializedProperty.GetArrayElementAtIndex (index));
					}

				} else {
					SerializedProperty element = m_ResolutionReorderableList.serializedProperty.GetArrayElementAtIndex (index);
					EditorGUI.PropertyField (position, element);
				}

			};

			m_ResolutionReorderableList.onChangedCallback = (ReorderableList list) => {
				m_Config.UpdateRatios ();
				EditorUtility.SetDirty (m_Obj);
			};

			m_ResolutionReorderableList.onSelectCallback = (ReorderableList list) => {
				m_Config.UpdateRatios ();
				EditorUtility.SetDirty (m_Obj);
			};

			m_ResolutionReorderableList.drawHeaderCallback = (Rect position) => {
				if (m_IsDevice) {
					EditorGUI.LabelField (position, "");
				} else {
					EditorGUI.LabelField (position, "Active  Width     Height  Scale Ratio   Orientation        Name");
				}
			};



			m_ResolutionReorderableList.onAddDropdownCallback = (Rect position, ReorderableList list) => {
				var menu = new GenericMenu ();

				ScreenshotResolutionPresets.Init ();

				ConstructResolutionPresetsMenu (menu);

				menu.AddItem (new GUIContent ("new"), false, OnResolutionSelectCallback, new ScreenshotResolution ());

				menu.ShowAsContext ();
			};

		}

		void ConstructResolutionPresetsMenu (GenericMenu menu)
		{
			foreach (string key in ScreenshotResolutionPresets.m_Categories.Keys) {
				menu.AddItem (new GUIContent (key + "/(add all)"), false, OnResolutionSelectAllCallback, ScreenshotResolutionPresets.m_Categories [key]);
			}

			foreach (ScreenshotResolution res in ScreenshotResolutionPresets.m_ResolutionPresets) {
				string name = res.m_Category + "/" + res.ToString ();
				menu.AddItem (new GUIContent (name), false, OnResolutionSelectCallback, res);
			}
			EditorUtility.SetDirty (m_Obj);
		}

		void OnResolutionSelectAllCallback (object target)
		{
			List<ScreenshotResolution> selection = (List<ScreenshotResolution>)target;
			foreach (ScreenshotResolution res in selection) {
				m_Config.m_Resolutions.Add (new ScreenshotResolution (res));
			}
			m_Config.UpdateRatios ();
			EditorUtility.SetDirty (m_Obj);
		}

		void OnResolutionSelectCallback (object target)
		{
			ScreenshotResolution selection = (ScreenshotResolution)target;
			m_Config.m_Resolutions.Add (new ScreenshotResolution (selection));
			m_Config.UpdateRatios ();
			EditorUtility.SetDirty (m_Obj);
		}

		public virtual void DrawResolutionGUI ()
		{
			DrawResolutionTitleGUI ();

			if (m_Config.m_ShowResolutions == false)
				return;		
						
			EditorGUILayout.PropertyField (m_ResolutionCaptureMode);
			EditorGUILayout.Separator ();

			if (m_Config.m_ResolutionCaptureMode == ScreenshotConfig.ResolutionMode.CUSTOM_RESOLUTIONS) {
				DrawResolutionContentGUI ();

				if (m_Config.GetActiveResolutions ().Count == 0) {
					EditorGUILayout.HelpBox ("No active resolutions.", MessageType.Warning);
				}
			}



		}

		public virtual void DrawResolutionTitleGUI ()
		{
			m_Config.m_ShowResolutions = EditorGUILayout.Foldout (m_Config.m_ShowResolutions, "Resolutions".ToUpper ());

		}

		public virtual void DrawResolutionContentGUI ()
		{
			if (m_Config.m_ShowResolutions == false)
				return;		

			// Buttons
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Select all")) {
				m_Config.SelectAllResolutions ();
				EditorUtility.SetDirty (m_Obj);
			}
			if (GUILayout.Button ("Deselect all")) {
				m_Config.ClearAllResolutions ();
				EditorUtility.SetDirty (m_Obj);
			}
			if (GUILayout.Button ("Remove all")) {
				m_Config.RemoveAllResolutions ();
				EditorUtility.SetDirty (m_Obj);
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Set all Portait")) {
				m_Config.SetAllPortait ();
				EditorUtility.SetDirty (m_Obj);
			}
			if (GUILayout.Button ("Set all Landscape")) {
				m_Config.SetAllLandscape ();
				EditorUtility.SetDirty (m_Obj);
			}
			EditorGUILayout.EndHorizontal ();



			EditorGUILayout.Space ();


//			if (m_IsDevice) {
			if (m_Expanded) {
				m_ResolutionReorderableList.elementHeight = EditorGUIUtility.singleLineHeight * 8f;
			} else {
				m_ResolutionReorderableList.elementHeight = EditorGUIUtility.singleLineHeight * 1f;
			}
//					if (GUILayout.Button ("Minimize")) {
//						m_Expanded = false;
//					}
//				} else {
//					if (GUILayout.Button ("Expand resolution settings")) {
//						m_ResolutionReorderableList.elementHeight = EditorGUIUtility.singleLineHeight * 10f;
//						m_Expanded = true;
//					}
//				}
//			}


			// List
			m_ResolutionReorderableList.DoLayoutList ();




			EditorGUILayout.HelpBox ("Use '+' to add preset(s) to the list. Note that your first click may take a few seconds.", MessageType.Info);


			if (GUILayout.Button ("Export active resolution(s) as cutom preset(s)")) {
				ScreenshotResolutionPresets.ExportPresets (m_Config.GetActiveResolutions ());
			}


		}

		#endregion

		#region OVERLAYS

		void CreateOverlayList ()
		{
			m_OverlayReorderableList = new ReorderableList (serializedObject, m_Overlays, true, true, true, true);
			m_OverlayReorderableList.drawElementCallback = (Rect position, int index, bool active, bool focused) => {
				SerializedProperty element = m_OverlayReorderableList.serializedProperty.GetArrayElementAtIndex (index);
				EditorGUI.PropertyField (position, element);
			};
			m_OverlayReorderableList.drawHeaderCallback = (Rect position) => {
				EditorGUI.LabelField (position, "Active     Canvas");
			};
		}

		public void DrawOverlaysGUI ()
		{
			// Title
			m_Config.m_ShowCanvas = EditorGUILayout.Foldout (m_Config.m_ShowCanvas, "Overlays".ToUpper ());
			if (m_Config.m_ShowCanvas == false)
				return;
			EditorGUILayout.Separator ();


			// Auto add
			EditorGUILayout.PropertyField (m_CaptureActiveUICanvas);

			// List
			m_OverlayReorderableList.DoLayoutList ();

			if (m_Config.m_CaptureActiveUICanvas && m_Config.m_CaptureMode == ScreenshotTaker.CaptureMode.RENDER_TO_TEXTURE) {
				EditorGUILayout.HelpBox ("Note that Screenspace Overlay Canvas and Overlays can not be rendered in RENDER_TO_TEXTURE mode.", MessageType.Info);
			} else if (m_Config.m_CaptureActiveUICanvas && m_Config.m_CameraMode == ScreenshotConfig.CamerasMode.CUSTOM_CAMERAS) {
				EditorGUILayout.HelpBox ("Note that some of your UI will not be rendered if its layer isn't in any active camera culling mask.", MessageType.Info);
			}
		}

		#endregion


		#region COMPOSITION

		public void DrawCompositionGUI ()
		{
			m_Config.m_ShowComposition = EditorGUILayout.Foldout (m_Config.m_ShowComposition, "Composition".ToUpper ());
			if (m_Config.m_ShowComposition == false)
				return;

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Config.m_CompositionMode"));

			if (m_Config.m_CompositionMode == ScreenshotConfig.CompositionMode.COMPOSITION) {
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Config.m_Composers"), true);
			}




		}

		public void DrawBatchesGUI ()
		{

			m_Config.m_ShowBatches = EditorGUILayout.Foldout (m_Config.m_ShowBatches, "Batches".ToUpper ());
			if (m_Config.m_ShowBatches == false)
				return;
			
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Config.m_BatchMode"));

			if (m_Config.m_BatchMode == ScreenshotConfig.BatchMode.BATCHES) {
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Config.m_Batches"), true);
			}

		}


		#endregion

		#region PREVIEW


		float m_PreviewWidth;
		float m_PreviewHeight;

		public void DrawPreviewGUI ()
		{

			// Title
			m_Config.m_ShowPreviewGUI = EditorGUILayout.Foldout (m_Config.m_ShowPreviewGUI, "Preview".ToUpper ());
			if (m_Config.m_ShowPreviewGUI == false)
				return;
			EditorGUILayout.Separator ();

			EditorGUILayout.PropertyField (m_PreviewInGameViewWhilePlaying);
			EditorGUILayout.Separator ();
			EditorGUILayout.Separator ();


			EditorGUILayout.PropertyField (m_ShowGuidesInPreview);
			EditorGUILayout.PropertyField (m_GuideCanvas);
			EditorGUILayout.PropertyField (m_GuidesColor);


			EditorGUILayout.Separator ();
			EditorGUILayout.PropertyField (m_ShowPreview);


			if (m_Config.m_ShowPreview) {

				EditorGUILayout.Slider (m_PreviewSize, 0.05f, 1f);

				// Draw preview texture if any
				if (m_Config.GetFirstActiveResolution ().m_Texture != null) {

					// On repaint event, compute the preview dimensions and update the texture if needed
					#if UNITY_2017_3_OR_NEWER
										if (Event.current.type == EventType.Repaint) {
					#else
					if (Event.current.type == EventType.repaint) {
						#endif

						if (m_Config.GetFirstActiveResolution ().IsValid ()) {

							m_PreviewWidth = m_Config.m_PreviewSize * GUILayoutUtility.GetLastRect ().width;
							m_PreviewHeight = m_PreviewWidth * m_Config.GetFirstActiveResolution ().m_Texture.height / m_Config.GetFirstActiveResolution ().m_Texture.width;

						}
					}


					// Draw an empty label to make some place to display the preview texture
					EditorGUILayout.LabelField ("", GUILayout.Height (m_PreviewHeight));

					Rect previewRect = GUILayoutUtility.GetLastRect ();
					previewRect.x = previewRect.x + previewRect.width / 2 - m_PreviewWidth / 2;
					previewRect.width = m_PreviewWidth;
					previewRect.height = m_PreviewHeight;
					EditorGUI.DrawPreviewTexture (previewRect, m_Config.GetFirstActiveResolution ().m_Texture);
				} else {
					EditorGUILayout.Separator ();
					EditorGUILayout.Separator ();
					EditorGUILayout.LabelField ("Press update to create the preview image.", UIStyle.centeredGreyTextStyle);
					EditorGUILayout.Separator ();
				}

			}

		}

		#endregion

		#region CAPTURE



		public virtual void DrawCaptureGUI ()
		{
			// Title
			m_Config.m_ShowCapture = EditorGUILayout.Foldout (m_Config.m_ShowCapture, "Capture".ToUpper ());
			if (m_Config.m_ShowCapture == false)
				return;
			EditorGUILayout.Separator ();

			// Mode selection
			EditorGUILayout.PropertyField (m_ShotMode);
			if (m_Config.m_ShotMode == ScreenshotConfig.ShotMode.BURST) {
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField (m_MaxBurstShotsNumber);
				EditorGUILayout.PropertyField (m_ShotTimeStep);
				EditorGUI.indentLevel--;
			}

			// Info message

			if (m_Config.m_ShotMode == ScreenshotConfig.ShotMode.BURST && !Application.isPlaying) {
				EditorGUILayout.HelpBox ("The application needs to be playing to take the screenshots.", MessageType.Info);
			}

			if (m_Config.m_ShotMode == ScreenshotConfig.ShotMode.BURST
			    && m_Config.m_PreviewInGameViewWhilePlaying && m_Config.GetActiveResolutions ().Count > 1) {
				EditorGUILayout.HelpBox ("In burst mode and PreviewInGameViewWhilePlaying mode, it is recommanded to only capture one resolution at a time to prevent GameView deformationss while capturing.", MessageType.Warning);
			}

			// Warning message
			if (m_Config.m_ShotMode == ScreenshotConfig.ShotMode.BURST
			    && m_Config.m_OverrideFiles) {
				EditorGUILayout.HelpBox ("The file override mode is enabled: burst screenshots are probably going to be overrided. Set override to false to automatically increment screenshot names.", MessageType.Warning);

			}


		}

		#endregion


		#region UTILS

		public virtual void DrawHotkeysGUI ()
		{
			// Title
			m_Config.m_ShowHotkeys = EditorGUILayout.Foldout (m_Config.m_ShowHotkeys, "Hotkeys.".ToUpper ());
			if (m_Config.m_ShowHotkeys == false)
				return;
			EditorGUILayout.Separator ();


			// Hotkeys
			DrawHotkey ("Capture Key", m_Config.m_CaptureHotkey);
			DrawHotkey ("Align To View Key", m_Config.m_AlignHotkey);
			DrawHotkey ("Update Preview Key", m_Config.m_UpdatePreviewHotkey);
			DrawHotkey ("Pause Key (ingame only)", m_Config.m_PauseHotkey);


			EditorGUILayout.HelpBox ("Note that these hotkeys only work in Playing mode, or in Edit mode when focused on the SceneView, on the inspector or on the editor window.", MessageType.Info);

		}

		public void DrawUtilsGUI ()
		{
			// Title
			m_Config.m_ShowUtils = EditorGUILayout.Foldout (m_Config.m_ShowUtils, "Utils".ToUpper ());
			if (m_Config.m_ShowUtils == false)
				return;
			EditorGUILayout.Separator ();


			// Time
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Time scale");
			float timeScale = m_Config.m_Time;
			float time = EditorGUILayout.Slider (timeScale, 0f, 1f);
			if (time != timeScale) {
				m_Config.SetTime (time);
			}
			EditorGUILayout.EndHorizontal ();

			if (time == 0f) {
				EditorGUILayout.HelpBox ("Time scale is set to 0.", MessageType.Warning);
			}

			// Pause button
			if (Time.timeScale == 0f) {
				if (GUILayout.Button ("Resume game (set time scale to 1)")) {
					m_Config.TogglePause ();
				} 
			} else {
				if (GUILayout.Button ("Pause game (set time scale to 0)")) {
					m_Config.TogglePause ();
				} 
			}

			// Align
			if (GUILayout.Button ("Align cameras to view")) {
				m_Config.AlignToView ();
			}

			EditorGUILayout.Separator ();
			EditorGUILayout.Separator ();

			// Sounds
			EditorGUILayout.PropertyField (m_PlaySoundOnCapture);
			EditorGUILayout.PropertyField (m_ShotSound);
			EditorGUILayout.PropertyField (m_StopTimeOnCapture);

			EditorGUILayout.Separator ();
			EditorGUILayout.Separator ();

		}


		bool permissionChecked = false;

		public void DrawUsage ()
		{
			// Title
			m_Config.m_ShowUsage = EditorGUILayout.Foldout (m_Config.m_ShowUsage, "Permissions".ToUpper ());
			if (m_Config.m_ShowUsage == false)
				return;

			PhotoUsageDescription usage = AssetUtils.GetOrCreate<PhotoUsageDescription> ("Assets/iOSPhotoUsageDescription.asset");
			if (usage == null)
				return;


			// Exclude from build
			EditorGUILayout.Separator ();
			if (usage.m_NeedsPermission) {
				if (!permissionChecked) {
					permissionChecked = true;
					FrameworkDependency.SetiOSFrameworkDependency ();
				}

				EditorGUILayout.HelpBox ("Currently, your iOS and Android builds REQUIER the access to the device and photo gallery. " +
				"If you only want to use the asset in editor or do not requier to access to the device gallery, use the button below. ", MessageType.Info);

				if (GUILayout.Button ("Remove permission needs for iOS and Android")) {
					RemovePermissionNeeds.Remove (true);
					usage.m_NeedsPermission = false;
				}

				EditorGUILayout.LabelField ("iOS Photo library usage description:");
				string newUsage = EditorGUILayout.TextArea (usage.m_UsageDescription, EditorStyles.textArea);
				if (newUsage != usage.m_UsageDescription) {
					usage.m_UsageDescription = newUsage;
					EditorUtility.SetDirty (usage);
				}	

			} else {
				EditorGUILayout.HelpBox ("Currently, your iOS and Android builds DO NOT REQUIER the access to the device and photo gallery. " +
				"However, it is now impossible to access the device gallery from build.", MessageType.Info);

				if (GUILayout.Button ("Restore permission needs for iOS and Android")) {
					RemovePermissionNeeds.Remove (false);
					usage.m_NeedsPermission = true;
				}
			}

		}


		public void DrawHotkey (string name, HotKey key)
		{
			EditorGUILayout.BeginHorizontal ();

			EditorGUILayout.LabelField (name);

			bool shift = EditorGUILayout.ToggleLeft ("Shift", key.m_Shift, GUILayout.MaxWidth (45));
			if (shift != key.m_Shift) {
				EditorUtility.SetDirty (m_Obj);
				key.m_Shift = shift;
			}

			bool control = EditorGUILayout.ToggleLeft ("Control", key.m_Control, GUILayout.MaxWidth (60));
			if (control != key.m_Control) {
				EditorUtility.SetDirty (m_Obj);
				key.m_Control = control;
			}

			bool alt = EditorGUILayout.ToggleLeft ("Alt", key.m_Alt, GUILayout.MaxWidth (40));
			if (alt != key.m_Alt) {
				EditorUtility.SetDirty (m_Obj);
				key.m_Alt = alt;
			}

			KeyCode k = (KeyCode)EditorGUILayout.EnumPopup (key.m_Key);
			if (k != key.m_Key) {
				EditorUtility.SetDirty (m_Obj);
				key.m_Key = k;
			}

			EditorGUILayout.EndHorizontal ();


		}

		public void DrawDelay (bool detailHelpbox = true)
		{
			// Title
			m_Config.m_ShowDelay = EditorGUILayout.Foldout (m_Config.m_ShowDelay, "Capture delay".ToUpper ());
			if (m_Config.m_ShowDelay == false)
				return;
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Config.m_GameViewResizingWaitingMode")); 
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Config.m_ResizingWaitingTime")); 
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("m_Config.m_ResizingWaitingFrames")); 

			string help = "If your game contains elements that need several frames or seconds to be updated, like custom game UI or post effects, set the delay to capture them properly.";
			if (detailHelpbox) {
				help += " For GameViewResizing capture mode only.";
			}
			EditorGUILayout.HelpBox (help, MessageType.Info);
		}


		#endregion





	}
}

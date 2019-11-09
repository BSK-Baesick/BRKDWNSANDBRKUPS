using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(IT_Gesture))]
public class ITGestureEditor : Editor {

	private static IT_Gesture instance;

	void Awake(){
		instance = (IT_Gesture)target;
	}
	
	//private static bool showDefaultFlag=false;
	private GUIContent cont;
	
	
	private float width=145;
	
	public override void OnInspectorGUI(){
		GUI.changed = false;
		EditorGUILayout.Space();
		
		serializedObject.Update();
		SerializedProperty prop = serializedObject.FindProperty("m_Script");
		EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
		serializedObject.ApplyModifiedProperties();
		
		if(!instance.enabled) return;
		
		
		EditorGUILayout.BeginHorizontal();
		string text="Check to enable auto input sensitivity adjustment based on DPI. When enabled, the code will auto scale the input accordingly a default reference value to compensate for DPI difference when switching to a different device";
		text+="\n\nNote that this will only scale the value used for thresholding purpose like MaxTapDisplacement and MultiTapPosSpacing on TapDetector. You will still be given the raw value when it comes to cursor delta or speed. It's up to you to decide if you want to implement the scaling in your code";
		cont=new GUIContent("Auto DPI Scaling:", text);
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.useDPIScaling=EditorGUILayout.Toggle(instance.useDPIScaling);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("Default test device DPI:", "The Dots-Per-Inch (DPI) value of your default test device. This will be used as the refence value to scale the input data");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		if(!instance.useDPIScaling) EditorGUILayout.LabelField("-");
		else instance.defaultDPI=EditorGUILayout.FloatField(instance.defaultDPI);
		EditorGUILayout.EndHorizontal();
		
		
		/*
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
		showDefaultFlag=EditorGUILayout.Foldout(showDefaultFlag, "Show default editor");
		EditorGUILayout.EndHorizontal();
		if(showDefaultFlag) DrawDefaultInspector();
		*/
		
		EditorGUILayout.Space();
		
		if(GUI.changed) EditorUtility.SetDirty(instance);
	}
}


using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(DragDetector))]
public class DragDetectorEditor : Editor {

	private static DragDetector instance;

	void Awake(){
		instance = (DragDetector)target;
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
		cont=new GUIContent("MiniDragDistance:", "Minimum distance in terms of pixel for the cursor to travel before a drag event is being fired");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.minDragDistance=EditorGUILayout.IntField(instance.minDragDistance, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("EnableMultiDrag:", "Check to enable multiple drag instances to be fired simultaneously. Please note that this is only supported for single-finger drag");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.enableMultiDrag=EditorGUILayout.Toggle(instance.enableMultiDrag);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("FireWhenNotMoving:", "Fire onDragging event even when the cursor/finger on screen is not moving as long as a drag has started and has not end");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.fireOnDraggingWhenNotMoving=EditorGUILayout.Toggle(instance.fireOnDraggingWhenNotMoving);
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


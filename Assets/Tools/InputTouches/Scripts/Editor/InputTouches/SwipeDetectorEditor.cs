using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(SwipeDetector))]
public class SwipeDetectorEditor : Editor {

	private static SwipeDetector instance;
	
	private GUIContent cont;
	
	//private static bool showDefaultFlag=false;
	
	void Awake(){
		instance = (SwipeDetector)target;
	}
	
	
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
		cont=new GUIContent("MaxSwipeDuration:", "Maximum duration in section for a swipe");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.maxSwipeDuration=EditorGUILayout.FloatField(instance.maxSwipeDuration);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("MinSpeed:", "Minimum relative speed required for a swipe. This is calculated using (pixel-travelled)/(time- swiped)");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.minSpeed=EditorGUILayout.FloatField(instance.minSpeed);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("MinDistance:", "Minimum distance in pixels required from the beginning to the end of the swipe");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.minDistance=EditorGUILayout.FloatField(instance.minDistance);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("MaxDirectionChange:", "Maximum change of direction allowed during the swipe. This is the angle difference measured from the initial swipe direction");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.maxDirectionChange=EditorGUILayout.FloatField(instance.maxDirectionChange);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("OnlyFireWhenLiftCursor:", "Only fire swipe onSwipeEndE event when the finger/cursor is lifted");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.onlyFireWhenLiftCursor=EditorGUILayout.Toggle(instance.onlyFireWhenLiftCursor);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("EnableMultiSwipe:", "When checked, there can be multiple instance of swipe on the screen simultaneously. Otherwise only the first one will be registered");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.enableMultiSwipe=EditorGUILayout.Toggle(instance.enableMultiSwipe);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("SwipeCooldown:", "The minimum cooldown duration between 2 subsequent swipe. During the cooldown, no swipe event will be registered");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.minDurationBetweenSwipe=EditorGUILayout.FloatField(instance.minDurationBetweenSwipe);
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


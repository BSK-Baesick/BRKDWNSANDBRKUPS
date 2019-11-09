using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(DualFingerDetector))]
public class DualFingerDetectorEditor : Editor {

	private static DualFingerDetector instance;
	
	//private static bool showDefaultFlag=false;
	private GUIContent cont;
	private GUIContent[] contList;
	
	private static string[] smoothMethodLabel=new string[0];
	private static string[] smoothMethodTooltip=new string[0];

	void Awake(){
		instance = (DualFingerDetector)target;
		
		//~ public enum _SmoothMethod{None, Average, WeightedAverage}
		int enumLength = Enum.GetValues(typeof(DualFingerDetector._SmoothMethod)).Length;
		smoothMethodLabel=new string[enumLength];
		smoothMethodTooltip=new string[enumLength];
		for(int i=0; i<enumLength; i++){
			smoothMethodLabel[i]=((DualFingerDetector._SmoothMethod)i).ToString();
			if((DualFingerDetector._SmoothMethod)i==DualFingerDetector._SmoothMethod.None) 
				smoothMethodTooltip[i]="No smoothing will be done at all";
			if((DualFingerDetector._SmoothMethod)i==DualFingerDetector._SmoothMethod.Average) 
				smoothMethodTooltip[i]="Just use the average value across as many frame as specified in SmoothIteration";
			if((DualFingerDetector._SmoothMethod)i==DualFingerDetector._SmoothMethod.WeightedAverage) 
				smoothMethodTooltip[i]="Like Average but weighted, the value which takes place in more recent frame will be given a higher priority and thus will carry more weight in calculated value";
		}
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
		cont=new GUIContent("SmoothIteration:", "Number of previous frame value to be taken into account for smoothing. Only applicable if smoothing is turn on");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.rotationSmoothFrameCount=EditorGUILayout.IntField(instance.rotationSmoothFrameCount);
		EditorGUILayout.EndHorizontal();
		
		int smooth=(int)instance.rotationSmoothing;
		cont=new GUIContent("SmoothMethod:", "The smoothing method to be used for rotation. This is to smooth any spike and give a more consistent value for rotation input. In most case, this is used to compensate the inconsistent nature of the input.");
		contList=new GUIContent[smoothMethodLabel.Length];
		for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(smoothMethodLabel[i], smoothMethodTooltip[i]);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		smooth = EditorGUILayout.Popup(smooth, contList);
		instance.rotationSmoothing=(DualFingerDetector._SmoothMethod)smooth;
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


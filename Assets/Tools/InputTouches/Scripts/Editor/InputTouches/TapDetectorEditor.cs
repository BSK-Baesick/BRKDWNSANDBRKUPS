using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(TapDetector))]
public class TapDetectorEditor : Editor {

	private static TapDetector instance;
	
	private GUIContent cont;
	private GUIContent[] contList;
	
	private static string[] chargeModeLabel=new string[0];
	private static string[] chargeModeTooltip=new string[0];
	
	//private static bool showDefaultFlag=false;
	
	void Awake(){
		instance = (TapDetector)target;
		
		//~ public enum _ChargeMode{Once, Clamp, Loop, PingPong}
		int enumLength = Enum.GetValues(typeof(_ChargeMode)).Length;
		chargeModeLabel=new string[enumLength];
		chargeModeTooltip=new string[enumLength];
		for(int i=0; i<enumLength; i++){
			chargeModeLabel[i]=((_ChargeMode)i).ToString();
			if((_ChargeMode)i==_ChargeMode.Once) 
				chargeModeTooltip[i]="The charge end event will only trigger once and will trigger as soon as the charge reaches full amount";
			if((_ChargeMode)i==_ChargeMode.Clamp) 
				chargeModeTooltip[i]="The charge end event will only trigger once and will trigger when the touch/mouse has been released";
			if((_ChargeMode)i==_ChargeMode.Loop) 
				chargeModeTooltip[i]="The charge end event will trigger as soon as the charge reaches full amount. The charge will reset and restart immediately";
			if((_ChargeMode)i==_ChargeMode.PingPong) 
				chargeModeTooltip[i]="The charge end event will only trigger once the touch/mouse has been released. The charging process will be switching back and forth between 0% and 100%";
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
		cont=new GUIContent("Enable MultiTap Filter:", "Check to enable multi-tap filter. This will disable any tap event prior to a multiTap event. In any event of multiTap, only the final tap will be fired.  ie. A double tap not longer cause a single tap event.\n\nPlease note that enable this will cause a significant delay to the respondsiveness of any tap event which have tap count less than 'MaxMultiTapCount'");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.enableMultiTapFilter=EditorGUILayout.Toggle(instance.enableMultiTapFilter);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("MaxTapDisplacement:", "The maximum cursor displacement (in pixel) allowed on screen for a tap to be considered valid");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.maxTapDisplacementAllowance=EditorGUILayout.IntField(instance.maxTapDisplacementAllowance);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("ShortTapTime:", "The maximum time for a short tap to be valid");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.shortTapTime=EditorGUILayout.FloatField(instance.shortTapTime);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("longTapTime:", "The time required for a long tap event to be fired");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.longTapTime=EditorGUILayout.FloatField(instance.longTapTime);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("MultiTapInterval:", "The maximum time window for a second tap to take place for a multi-Tap event to be fired. Otherwise the tap will be registered as a new event and not a continuous event");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.multiTapInterval=EditorGUILayout.FloatField(instance.multiTapInterval);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("MultiTapPosSpacing:", "The maximum spacing in pixels allowed for a consecutive tap to be registered as a multi-Tap");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.multiTapPosSpacing=EditorGUILayout.FloatField(instance.multiTapPosSpacing);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("MaxMultiTapCount:", "The maximum multi-Tap count allowed\nSet to 1 to disable multi-Tap, 2 to enable double-tap and 3 to enable double-tap and triple-tap and so on");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.maxMultiTapCount=EditorGUILayout.IntField(instance.maxMultiTapCount);
		EditorGUILayout.EndHorizontal();
		
		int chargeMode=(int)instance.chargeMode;
		cont=new GUIContent("Charge Mode:", "The charge mode for charge event");
		contList=new GUIContent[chargeModeLabel.Length];
		for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(chargeModeLabel[i], chargeModeTooltip[i]);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		chargeMode = EditorGUILayout.Popup(chargeMode, contList);
		instance.chargeMode=(_ChargeMode)chargeMode;
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("MinChargeTime:", "Minimum time required for a charge event to start trigger. The value of percent passed by the event at this point will be minChargeTime/maxChargeTime");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.minChargeTime=EditorGUILayout.FloatField(instance.minChargeTime);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("MaxChargeTime:", "Maximum time possible for a charge event. The value of percent passed by the event at this point will be 1");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.maxChargeTime=EditorGUILayout.FloatField(instance.maxChargeTime);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("MaxFingerDistInGroup:", "The maximum distance between each finger for a multi-finger tap event. This is very much device dependent. Naturally this should be set so the pixel on screen covered the size of the number of fingers allowed. 1.5 inches for 2 fingers, 2 inches for 3 fingers and so on");
		EditorGUILayout.LabelField(cont, GUILayout.Width(width));
		instance.maxFingerGroupDist=EditorGUILayout.FloatField( instance.maxFingerGroupDist);
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


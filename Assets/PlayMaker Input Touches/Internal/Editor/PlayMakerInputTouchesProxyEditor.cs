using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PlayMakerInputTouchesProxy))] 

public class PlayMakerInputTouchesProxyEditor : Editor {


	SerializedProperty debug;
	
	SerializedProperty On;
	SerializedProperty Down;
	SerializedProperty Up;
	
	SerializedProperty MultiTap;
	SerializedProperty LongTap;

	SerializedProperty MultiFingersMultiTap;
	SerializedProperty MultiFingersLongTap;
	
	
	SerializedProperty Swipe;
	SerializedProperty Pinch;
	SerializedProperty Rotate;

	SerializedProperty Charging;
	SerializedProperty Dragging;

	SerializedProperty MultiFingersCharging;
	SerializedProperty MultiFingersDragging;
	
	SerializedProperty SwipeAngleThreshold;
	
    public void OnEnable () {
        // Setup the SerializedProperties
        debug = serializedObject.FindProperty("debug");
		
		On = serializedObject.FindProperty("On");
		Down = serializedObject.FindProperty("Down");
		Up = serializedObject.FindProperty("Up");
		
		MultiTap = serializedObject.FindProperty("MultiTap");
		LongTap = serializedObject.FindProperty("LongTap");
		
		MultiFingersMultiTap = serializedObject.FindProperty("MultiFingersMultiTap");
		MultiFingersLongTap = serializedObject.FindProperty("MultiFingersLongTap");
		
		Charging = serializedObject.FindProperty("Charging");
		Dragging = serializedObject.FindProperty("Dragging");
		
		MultiFingersCharging = serializedObject.FindProperty("MultiFingersCharging");
		MultiFingersDragging = serializedObject.FindProperty("MultiFingersDragging");
		
		Swipe = serializedObject.FindProperty("Swipe");
		Pinch = serializedObject.FindProperty("Pinch");
		Rotate = serializedObject.FindProperty("Rotate");
		
		SwipeAngleThreshold = serializedObject.FindProperty("SwipeAngleThreshold");
    }
	
	
	private bool _rawFoldOut = true;
	private bool _tapFoldOut = true;
	private bool _gestureFoldOut = true;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		
		PlayMakerInputTouchesProxy _target = target as PlayMakerInputTouchesProxy;
		if (GUILayout.Button("Online help"))
        {
           Application.OpenURL("https://hutonggames.fogbugz.com/default.asp?W961");
        }
		
		EditorGUI.indentLevel = 1;
			EditorGUILayout.PropertyField(debug,new GUIContent("Debug") );
		EditorGUI.indentLevel = 0;
		EditorGUILayout.Separator();
		
		_rawFoldOut = EditorGUILayout.Foldout(_rawFoldOut,"Raw Touches");
		if (_rawFoldOut)
		{
			EditorGUI.indentLevel = 1;
				EditorGUILayout.PropertyField(Down,new GUIContent("Down"));
				EditorGUILayout.PropertyField(On,new GUIContent("On"));
				EditorGUILayout.PropertyField(Up,new GUIContent("Up"));
			EditorGUI.indentLevel = 0;
		}
		_tapFoldOut =  EditorGUILayout.Foldout(_tapFoldOut,"Tap touches");
		if (_tapFoldOut)
		{
			EditorGUI.indentLevel = 1;
				EditorGUILayout.PropertyField(MultiTap,new GUIContent("Tap"));
				EditorGUILayout.PropertyField(MultiFingersMultiTap,new GUIContent("Multi Fingers Tap"));
				EditorGUILayout.PropertyField(LongTap,new GUIContent("LongTap"));
				EditorGUILayout.PropertyField(MultiFingersLongTap,new GUIContent("Multi Fingers Long Tap"));
				EditorGUILayout.PropertyField(Charging,new GUIContent("Charging"));
				EditorGUILayout.PropertyField(MultiFingersCharging,new GUIContent("Multi Fingers Charging"));
			
			EditorGUI.indentLevel = 0;
		}
		
		_gestureFoldOut =  EditorGUILayout.Foldout(_gestureFoldOut,"Gestures touches");
		if (_gestureFoldOut)
		{
			EditorGUI.indentLevel = 1;
				EditorGUILayout.PropertyField(Swipe,new GUIContent("Swipe"));
			if (_target.Swipe)
			{
				EditorGUI.indentLevel = 2;
				EditorGUILayout.PropertyField(SwipeAngleThreshold,new GUIContent("Angle threshold","Angle threshold to fire Swipe Down, Swipe Up, Swipe Right, Swipe Left"));
				EditorGUI.indentLevel = 1;
			}
				EditorGUILayout.PropertyField(Pinch,new GUIContent("Pinch"));
				EditorGUILayout.PropertyField(Rotate,new GUIContent("Rotate"));
				EditorGUILayout.PropertyField(Dragging,new GUIContent("Dragging"));
				EditorGUILayout.PropertyField(MultiFingersDragging,new GUIContent("Multi Fingers Dragging"));
			
				
			
			EditorGUI.indentLevel = 0;
		}		
		
		serializedObject.ApplyModifiedProperties ();
		

		if (_target.On != _target.isOn)
		{
			Debug.Log("on :"+_target.On);
			_target.isOn = _target.On;
		}
		if (_target.Up != _target.isUp)
		{
			Debug.Log("up :"+_target.Up);
			_target.isUp = _target.Up;
		}
		if (_target.Down != _target.isDown)
		{
			Debug.Log("down :"+_target.Down);
			_target.isDown = _target.Down;
		}

		if (_target.MultiTap != _target.isMultiTap)
		{
			Debug.Log("Multi Tap :"+_target.MultiTap);
			_target.isMultiTap = _target.MultiTap;
		}
		if (_target.LongTap != _target.isLongTap)
		{
			Debug.Log("Long Tap :"+_target.LongTap);
			_target.isLongTap = _target.LongTap;
		}

		if (_target.MultiFingersMultiTap != _target.isMultiFingersMultiTap)
		{
			Debug.Log("Multi Fingers Multi Tap :"+_target.MultiFingersMultiTap);
			_target.isMultiFingersMultiTap = _target.MultiFingersMultiTap;
		}
		if (_target.MultiFingersLongTap != _target.isMultiFingersLongTap)
		{
			Debug.Log("Multi Fingers Long Tap :"+_target.MultiFingersLongTap);
			_target.isMultiFingersLongTap = _target.MultiFingersLongTap;
		}
		
		if (_target.Swipe != _target.isSwipe)
		{
			Debug.Log("swipe :"+_target.Swipe);
			_target.isSwipe = _target.Swipe;
		}		
		if (_target.Pinch != _target.isPinch)
		{
			Debug.Log("pinch :"+_target.Pinch);
			_target.isPinch = _target.Pinch;
		}	
		if (_target.Rotate != _target.isRotate)
		{
			Debug.Log("rotate :"+_target.Rotate);
			_target.isRotate = _target.Rotate;
		}
		if (_target.Charging != _target.isCharging)
		{
			Debug.Log("charging :"+_target.Charging);
			_target.isCharging = _target.Charging;
		}	
		if (_target.Dragging != _target.isDragging)
		{
			Debug.Log("dragging :"+_target.Dragging);
			_target.isDragging = _target.Dragging;
		}
		if (_target.MultiFingersCharging != _target.isMultiFingersCharging)
		{
			Debug.Log("Multi Fingers charging :"+_target.MultiFingersCharging);
			_target.isMultiFingersCharging = _target.MultiFingersCharging;
		}	
		if (_target.MultiFingersDragging != _target.isMultiFingersDragging)
		{
			Debug.Log("Multi Fingers dragging :"+_target.MultiFingersDragging);
			_target.isMultiFingersDragging = _target.MultiFingersDragging;
		}		
	}
}

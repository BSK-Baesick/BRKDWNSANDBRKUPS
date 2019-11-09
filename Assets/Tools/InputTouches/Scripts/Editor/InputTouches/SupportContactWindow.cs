
/***************************************************************************************************************

	This script contains the code for support and contact information
	Please dont modify this script


****************************************************************************************************************/

using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

public class SupportContactWindow : EditorWindow {

	private static SupportContactWindow window;
	
	public static void Init () {
		window = (SupportContactWindow)EditorWindow.GetWindow(typeof (SupportContactWindow));
		window.minSize=new Vector2(375, 250);
	}
	
	void OnGUI () {
		if(window==null) Init();
		
		float startX=5;
		float startY=5;
		float spaceX=70;
		float spaceY=18;
		float width=230;
		float height=17;
		
		GUIStyle style=new GUIStyle("Label");
		style.fontSize=16;
		style.fontStyle=FontStyle.Bold;
		
		GUIContent cont=new GUIContent("InputTouches");
		EditorGUI.LabelField(new Rect(startX, startY, 300, 30), cont, style);
		
		EditorGUI.LabelField(new Rect(startX, startY+8, 320, height), "____________________________________________");
		
		startY+=30;
		EditorGUI.LabelField(new Rect(startX, startY, width, height), " - Version:");
		EditorGUI.LabelField(new Rect(startX+spaceX, startY, width, height), "1.2.2");
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), " - Release:");
		EditorGUI.LabelField(new Rect(startX+spaceX, startY, width, height), "6 February 2018");
		
		startY+=15;
		
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "Developed by K.Song Tan");
		
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), " - Email:");
		EditorGUI.TextField(new Rect(startX+spaceX, startY, width, height), "k.songtan@gmail.com");
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), " - Twitter:");
		EditorGUI.TextField(new Rect(startX+spaceX, startY, width, height), "SongTan@SongGameDev");
		
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), " - Website:");
		EditorGUI.TextField(new Rect(startX+spaceX, startY, width, height), "http://www.songgamedev.com/");
		if(GUI.Button(new Rect(startX+spaceX+width+10, startY, 50, height), "Open")){
			Application.OpenURL("https://www.songgamedev.com/");
		}
		
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), " - Support:");
		EditorGUI.TextField(new Rect(startX+spaceX, startY, width, height), "http://forum.unity3d.com/threads/input-touches-simple-gesture-solution.133991/");
		if(GUI.Button(new Rect(startX+spaceX+width+10, startY, 50, height), "Open")){
			Application.OpenURL("https://forum.unity.com/threads/input-touches-simple-gesture-solution.133991/");
		}
		
		startY+=spaceY;
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 300, height), "        Your feedback are much appreciated!");
		if(GUI.Button(new Rect(startX, startY+=spaceY, 300, height), "Please Rate InputTouches!")){
			Application.OpenURL("http://u3d.as/2Tf");
		}
	}
	
}

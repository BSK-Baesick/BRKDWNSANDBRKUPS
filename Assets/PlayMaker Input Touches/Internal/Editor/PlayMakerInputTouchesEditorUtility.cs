using UnityEditor;
using UnityEngine;

public class PlayMakerInputTouchesEditorUtility : Editor {
	
	
	static string PlayMakerInputTouchesProxyName = "PlayMakerInputTouchesProxy";
	static string InputTouchesGestureName = "Gesture";
	
	/// <summary>
	/// Menu item function that checks and adds if necessary the "PlayMaker Photon Proxy" prefab.
	/// </summary>
	[MenuItem("PlayMaker Add ons/Input touches/Components/Add proxy to scene", false)]
  	public static void DoCreateInputTouchesProxy()
    {
	
		Debug.Log("DoCreateInputTouchesProxy");
		
		if (! IsGestureSetup() )
		{
			Debug.Log(InputTouchesGestureName);

			PrefabUtility.InstantiatePrefab(Resources.Load(InputTouchesGestureName, typeof(GameObject)));
		}
		
		if ( IsSceneSetup() )
		{
			UnityEngine.Debug.LogWarning("Only one PlayMaker Input Touches Proxy GameObject is needed per scene");	
		}else{
			Debug.Log(PlayMakerInputTouchesProxyName);
			PrefabUtility.InstantiatePrefab(Resources.Load(PlayMakerInputTouchesProxyName, typeof(GameObject)));
		}
		
	}//DoCreateRpcProxy
	
	
	public static bool IsSceneSetup()
	{
				
		GameObject go = GameObject.Find(PlayMakerInputTouchesProxyName);
		
		return go !=null;
	}
	public static bool IsGestureSetup()
	{
				
		GameObject go = GameObject.Find(InputTouchesGestureName);
		
		return go !=null;
	}	
	
	
	

}
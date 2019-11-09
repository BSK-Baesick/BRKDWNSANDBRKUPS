// (c) copyright Hutong Games, LLC 2010-2012. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("InputTouches")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W964")]
	[Tooltip("Get the last event magnitude. Applies to drag, charge, pinch and rotate and swipe gestures")]
	public class InputTouchesGetMagnitude : FsmStateAction
	{

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("the magnitude of the last event. Applies to Pinch and Rotate gestures")]
		public FsmFloat magnitude;
		
		
		public override void Reset()
		{
			magnitude = null;
		}

		public override void OnEnter()
		{
			magnitude.Value = PlayMakerInputTouchesProxy.LastEventMagnitude;
		}
		
	}
}
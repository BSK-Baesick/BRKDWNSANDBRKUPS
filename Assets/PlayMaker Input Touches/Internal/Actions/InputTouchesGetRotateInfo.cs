// (c) copyright Hutong Games, LLC 2010-2012. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("InputTouches")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1028")]
	[Tooltip("Get the last rotate event info")]
	public class InputTouchesGetRotateInfo : FsmStateAction
	{

		[UIHint(UIHint.Variable)]
		[Tooltip("the screen position of the first touch when the rotating")]
		public FsmVector2 firstPoint;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("the screen position of the second touch when the rotating")]
		public FsmVector2 secondPoint;		

		[UIHint(UIHint.Variable)]
		[Tooltip("the magnitude of the rotation on screen.")]
		public FsmFloat magnitude;
		
		
		public override void Reset()
		{
			firstPoint = null;
			secondPoint = null;
			magnitude = null;
		}

		public override void OnEnter()
		{
			firstPoint.Value = PlayMakerInputTouchesProxy.LastRotateInfo.pos1;
			secondPoint.Value = PlayMakerInputTouchesProxy.LastRotateInfo.pos2;
			magnitude.Value = PlayMakerInputTouchesProxy.LastRotateInfo.magnitude;
		}
		
	}
}
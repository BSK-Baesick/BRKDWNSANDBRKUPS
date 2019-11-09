// (c) copyright Hutong Games, LLC 2010-2012. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("InputTouches")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W1027")]
	[Tooltip("Get the last pinch event info")]
	public class InputTouchesGetPinchInfo : FsmStateAction
	{

		[UIHint(UIHint.Variable)]
		[Tooltip("the screen position of the first touch when pinching")]
		public FsmVector2 firstPoint;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("the screen position of the second touch when pinching")]
		public FsmVector2 secondPoint;		

		[UIHint(UIHint.Variable)]
		[Tooltip("the magnitude of the pinch on screen.")]
		public FsmFloat magnitude;
		
		
		public override void Reset()
		{
			firstPoint = null;
			secondPoint = null;
			magnitude = null;
		}

		public override void OnEnter()
		{
			firstPoint.Value = PlayMakerInputTouchesProxy.LastPinchInfo.pos1;
			secondPoint.Value = PlayMakerInputTouchesProxy.LastPinchInfo.pos2;
			magnitude.Value = PlayMakerInputTouchesProxy.LastPinchInfo.magnitude;
		}
		
	}
}
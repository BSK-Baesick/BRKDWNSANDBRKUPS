// (c) copyright Hutong Games, LLC 2010-2012. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("InputTouches")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W966")]
	[Tooltip("Get the last swipe event info")]
	public class InputTouchesGetSwipeInfo : FsmStateAction
	{

		[UIHint(UIHint.Variable)]
		[Tooltip("the screen position of the cursor when the swipe event start")]
		public FsmVector2 startPoint;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("the screen position of the cursor when the swipe event end")]
		public FsmVector2 endPoint;		
		
		[UIHint(UIHint.Variable)]
		[Tooltip("the direction vector of the swipe")]
		public FsmVector2 direction;

		[UIHint(UIHint.Variable)]
		[Tooltip("the angle of the swipe on screen. Start from +ve x-axis in counter-clockwise direction")]
		public FsmFloat angle;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("the duration taken for the swipe")]
		public FsmFloat duration;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("the relative speed of the swipe")]
		public FsmFloat speed;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("boolean flag indicate if the input type is a mouse input.")]
		public FsmBool isMouse;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("unique index indicate which touch/mouse input trigger the event.\n " +
			"This is so an event by a particular touch can be keep tracked when there are multiple event on screen.")]
		public FsmInt index;
		
		
		public override void Reset()
		{
			startPoint = null;
			endPoint = null;
			direction = null;
			angle = null;
			duration = null;
			speed = null;
			isMouse = null;
			index = null;
		}

		public override void OnEnter()
		{
			startPoint.Value = PlayMakerInputTouchesProxy.LastSwipeInfo.startPoint;
			endPoint.Value = PlayMakerInputTouchesProxy.LastSwipeInfo.endPoint;
			direction.Value = PlayMakerInputTouchesProxy.LastSwipeInfo.direction;
			angle.Value = PlayMakerInputTouchesProxy.LastSwipeInfo.angle;
			duration.Value = PlayMakerInputTouchesProxy.LastSwipeInfo.duration;
			speed.Value = PlayMakerInputTouchesProxy.LastSwipeInfo.speed;
			isMouse.Value = PlayMakerInputTouchesProxy.LastSwipeInfo.isMouse;
			index.Value = PlayMakerInputTouchesProxy.LastSwipeInfo.index;
		}
		
	}
}
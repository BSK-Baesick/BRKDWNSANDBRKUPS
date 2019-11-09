// (c) copyright Hutong Games, LLC 2010-2012. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("InputTouches")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W962")]
	[Tooltip("Get the last charging info")]
	public class InputTouchesGetChargingInfo : FsmStateAction
	{
		
		[UIHint(UIHint.Variable)]
		[Tooltip("the percent of the charge. takes value from 0.0 - 1.0")]
		public FsmFloat percent;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("the screen position of the cursor for click/single finger event. For multi fingers event, this is the position between the multi fingers")]
		public FsmVector2 position;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("boolean flag indicate if the input type is a mouse input.")]
		public FsmBool isMouse;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("the number of fingers triggering this event")]
		public FsmInt fingerCount;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("unique index indicate which touch/mouse input trigger the event.\n " +
			"This is so an event by a particular touch can be keep tracked when there are multiple event on screen.")]
		public FsmInt index;
		
		public override void Reset()
		{
			percent = null;
			position = null;
			isMouse = null;
			index = null;
			fingerCount = null;
		}

		public override void OnEnter()
		{
			percent.Value = PlayMakerInputTouchesProxy.LastChargedInfo.percent;
			position.Value = PlayMakerInputTouchesProxy.LastChargedInfo.pos;
			isMouse.Value = PlayMakerInputTouchesProxy.LastChargedInfo.isMouse;
			index.Value = PlayMakerInputTouchesProxy.LastChargedInfo.index;
			fingerCount.Value = PlayMakerInputTouchesProxy.LastChargedInfo.fingerCount;
			
		}
		
	}
}
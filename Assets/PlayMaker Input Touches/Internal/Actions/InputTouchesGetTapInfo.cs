// (c) copyright Hutong Games, LLC 2010-2012. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("InputTouches")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W981")]
	[Tooltip("Get the last Tap info")]
	public class InputTouchesGetTapInfo : FsmStateAction
	{
		
		[UIHint(UIHint.Variable)]
		[Tooltip("The number of tap for this particular event. 1 for single-tap, 2 for double-tap and so on.")]
		public FsmInt count;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("the screen position of the cursor for click/single finger event. For multi fingers event, this is the position between the two fingers")]
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
			count = null;
			position = null;
			isMouse = null;
			index = null;
			fingerCount = null;
		}

		public override void OnEnter()
		{
			count.Value = PlayMakerInputTouchesProxy.LastTapInfo.count;
			position.Value = PlayMakerInputTouchesProxy.LastTapInfo.pos;
			isMouse.Value = PlayMakerInputTouchesProxy.LastTapInfo.isMouse;
			index.Value = PlayMakerInputTouchesProxy.LastTapInfo.index;
			fingerCount.Value = PlayMakerInputTouchesProxy.LastTapInfo.fingerCount;
			
		}
		
	}
}
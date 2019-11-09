// (c) copyright Hutong Games, LLC 2010-2012. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("InputTouches")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W963")]
	[Tooltip("Get the last dragging info")]
	public class InputTouchesGetDraggingInfo : FsmStateAction
	{
		
		[UIHint(UIHint.Variable)]
		[Tooltip("the screen position of the cursor")]
		public FsmVector2 position;
		
		[UIHint(UIHint.Variable)]
		[Tooltip("the moved direction of the event")]
		public FsmVector2 delta;	
	
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
			index = null;
			isMouse = null;
			position = null;
			delta = null;
			fingerCount = null;
		}

		public override void OnEnter()
		{
			index.Value = PlayMakerInputTouchesProxy.LastDragInfo.index;
			isMouse.Value = PlayMakerInputTouchesProxy.LastDragInfo.isMouse;
			position.Value = PlayMakerInputTouchesProxy.LastDragInfo.pos;
			delta.Value = PlayMakerInputTouchesProxy.LastDragInfo.delta;	
			fingerCount.Value = PlayMakerInputTouchesProxy.LastDragInfo.fingerCount;
		}
		
	}
}
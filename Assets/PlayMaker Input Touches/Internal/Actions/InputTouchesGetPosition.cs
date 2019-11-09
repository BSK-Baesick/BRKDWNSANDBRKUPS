// (c) copyright Hutong Games, LLC 2010-2012. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("InputTouches")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W965")]
	[Tooltip("Get the last event position")]
	public class InputTouchesGetPosition : FsmStateAction
	{

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("the position of the last event")]
		public FsmVector2 position;
		
		
		public override void Reset()
		{
			position = null;
		}

		public override void OnEnter()
		{
			position.Value = PlayMakerInputTouchesProxy.LastEventPosition;
		}
		
	}
}
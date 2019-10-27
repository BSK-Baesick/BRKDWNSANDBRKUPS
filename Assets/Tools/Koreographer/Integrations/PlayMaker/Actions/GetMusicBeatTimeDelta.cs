//----------------------------------------------
//            	   Koreographer                 
//    Copyright © 2014-2016 Sonic Bloom, LLC    
//----------------------------------------------

using UnityEngine;
using HutongGames.PlayMaker;
using Tooltip=HutongGames.PlayMaker.TooltipAttribute;

namespace SonicBloom.Koreo.PlayMaker
{
	[ActionCategory("Koreographer")]
	[Tooltip("Gets the delta in beat time of the current Koreography processing pass as a float.  This value is retrieved from the singleton Koreographer component instance.")]
	public class GetMusicBeatTimeDelta : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the beat time delta in a float variable.  If no music is playing [or a music player is not properly configured], this will return 0.")]
		public FsmFloat storeBeatTimeDelta;

		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		[ActionSection("Optional")]

		[Tooltip("The name of a specific audio clip to check music position of.")]
		public FsmString clipName;

		[Tooltip("The number of subdivisions of a beat to use as a base (must be greater than 0).  This is effectively a multiplier of the resultant delta.")]
		public FsmInt subdivisions = 1;

		public override void Reset()
		{
			storeBeatTimeDelta = null;
			everyFrame = false;
			clipName = null;
			subdivisions = 1;
		}

		public override void OnEnter()
		{
			DoGetBeatTimeDelta();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoGetBeatTimeDelta();
		}

		void DoGetBeatTimeDelta()
		{
			if (subdivisions.Value < 1)
			{
				subdivisions.Value = 1;
			}

			storeBeatTimeDelta.Value = Koreographer.GetBeatTimeDelta(clipName.Value, subdivisions.Value);
		}

		public override string ErrorCheck()
		{
			string error = base.ErrorCheck();

			if (subdivisions.Value < 1)
			{
				error = "\"Subdivisions\" must contain a value of 1 or greater.";
			}

			return error;
		}
	}
}

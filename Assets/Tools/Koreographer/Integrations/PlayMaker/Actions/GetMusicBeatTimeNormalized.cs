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
	[Tooltip("Gets the percentage of the way through the current beat as a float in the range of [0,1).  This value is retrieved from the singleton Koreographer component instance.")]
	public class GetMusicBeatTimeNormalized : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the normalized beat in a float variable.  If no music is playing [or a music player is not properly configured], this will return 0.")]
		public FsmFloat storeNormalizedBeatTime;

		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		[ActionSection("Optional")]

		[Tooltip("The name of a specific audio clip to check music position of.")]
		public FsmString clipName;

		[Tooltip("The number of subdivisions of a beat to use as a base (must be greater than 0).  Increasing this value will decrease the normalization period (values will cycle faster).")]
		public FsmInt subdivisions = 1;

		public override void Reset()
		{
			storeNormalizedBeatTime = null;
			everyFrame = false;
			clipName = null;
			subdivisions = 1;
		}

		public override void OnEnter()
		{
			DoGetBeatTimeNormalized();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoGetBeatTimeNormalized();
		}

		void DoGetBeatTimeNormalized()
		{
			if (subdivisions.Value < 1)
			{
				subdivisions.Value = 1;
			}

			storeNormalizedBeatTime.Value = Koreographer.GetBeatTimeNormalized(clipName.Value, subdivisions.Value);
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

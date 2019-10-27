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
	[Tooltip("Gets the current music time in beats as a float.  This value is retrieved from the singleton Koreographer component instance.")]
	public class GetMusicBeatTime : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the beat time in a float variable.  If no music is playing [or a music player is not properly configured], this will return -1.")]
		public FsmFloat storeBeatTime;

		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		[ActionSection("Optional")]

		[Tooltip("The name of a specific audio clip to check music position of.")]
		public FsmString clipName;

		[Tooltip("The number of subdivisions of a beat to use as a base (must be greater than 0).  If the music time is 12.5 beats and subdivisions is 2, this will return 25 beats.")]
		public FsmInt subdivisions = 1;

		public override void Reset()
		{
			storeBeatTime = null;
			everyFrame = false;
			clipName = null;
			subdivisions = 1;
		}

		public override void OnEnter()
		{
			DoGetBeatTime();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoGetBeatTime();
		}

		void DoGetBeatTime()
		{
			if (subdivisions.Value < 1)
			{
				subdivisions.Value = 1;
			}

			storeBeatTime.Value = Koreographer.GetBeatTime(clipName.Value, subdivisions.Value);
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

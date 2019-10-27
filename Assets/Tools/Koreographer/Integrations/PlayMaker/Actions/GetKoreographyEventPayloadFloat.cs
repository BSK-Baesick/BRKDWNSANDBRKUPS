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
	[Tooltip("Retrieves a Float value from stored Koreography Event info!  This action will evaluate a Curve Payload to get the float at the current event time.")]
	public class GetKoreographyEventPayloadFloat : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The float variable into which to store the Float Payload value or the result of an evaluated Curve Payload from the Koreography Event.")]
		public FsmFloat storeFloatPayload;

		public bool warnOnWrongType = true;

		public override void OnEnter()
		{
			KoreographyEventInfo info = KoreographyEventInfo.CallbackInfo;

			if (info != null)
			{
				KoreographyEvent koreoEvent = info.koreoEvent;

				if (koreoEvent != null)
				{
					FloatPayload fpl = koreoEvent.Payload as FloatPayload;
					if (fpl != null)
					{
						storeFloatPayload.Value = fpl.FloatVal;
					}
					else if (koreoEvent.HasCurvePayload())
					{
						storeFloatPayload.Value = koreoEvent.GetValueOfCurveAtTime(info.sampleTime);
					}
					else if (warnOnWrongType)
					{
						LogWarning("Processed a KoreographyEvent with no FloatPayload or CurvePayload!  Leaving variable unchanged!");
					}
				}
			}
			else
			{
				LogWarning("No Koreography Event Info available.  This action will only work during a Koreography Event callback phase.");
			}

			Finish();
		}
	}
}

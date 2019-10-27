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
	[Tooltip("Retrieves an AnimationCurve from stored Koreography Event info!")]
	public class GetKoreographyEventPayloadCurve : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The AnimationCurve variable into which to store the Curve Payload data from the Koreography Event.")]
		public FsmAnimationCurve storeCurvePayload;

		public bool warnOnWrongType = true;

		public override void OnEnter()
		{
			KoreographyEventInfo info = KoreographyEventInfo.CallbackInfo;

			if (info != null)
			{
				KoreographyEvent koreoEvent = info.koreoEvent;

				if (koreoEvent != null)
				{
					CurvePayload pl = koreoEvent.Payload as CurvePayload;
					if (pl != null)
					{
						storeCurvePayload.curve = pl.CurveData;
					}
					else if (warnOnWrongType)
					{
						LogWarning("Processed a KoreographyEvent with no CurvePayload!  Leaving variable unchanged!");
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

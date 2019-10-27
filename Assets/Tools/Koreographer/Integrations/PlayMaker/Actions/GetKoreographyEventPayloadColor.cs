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
	[Tooltip("Retrieves a Color value from stored Koreography Event info!  This action will evaluate a Gradient Payload to get the Color at the current event time.")]
	public class GetKoreographyEventPayloadColor : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The Color variable into which to store the Color Payload value or the result of an evaluated Gradient Payload from the Koreography Event.")]
		public FsmColor storeColorPayload;

		public bool warnOnWrongType = true;

		public override void OnEnter()
		{
			KoreographyEventInfo info = KoreographyEventInfo.CallbackInfo;

			if (info != null)
			{
				KoreographyEvent koreoEvent = info.koreoEvent;

				if (koreoEvent != null)
				{
					ColorPayload cpl = koreoEvent.Payload as ColorPayload;
					if (cpl != null)
					{
						storeColorPayload.Value = cpl.ColorVal;
					}
					else if (koreoEvent.HasGradientPayload())
					{
						storeColorPayload.Value = koreoEvent.GetColorOfGradientAtTime(info.sampleTime);
					}
					else if (warnOnWrongType)
					{
						LogWarning("Processed a KoreographyEvent with no ColorPayload or GradientPayload!  Leaving variable unchanged!");
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

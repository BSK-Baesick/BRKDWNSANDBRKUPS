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
	[Tooltip("Retrieves an Int value from stored Koreography Event info!")]
	public class GetKoreographyEventPayloadInt : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The int variable into which to store the Int Payload value from the Koreography Event.")]
		public FsmInt storeIntPayload;

		public bool warnOnWrongType = true;

		public override void OnEnter()
		{
			KoreographyEventInfo info = KoreographyEventInfo.CallbackInfo;

			if (info != null)
			{
				KoreographyEvent koreoEvent = info.koreoEvent;

				if (koreoEvent != null)
				{
					IntPayload pl = koreoEvent.Payload as IntPayload;
					if (pl != null)
					{
						storeIntPayload.Value = pl.IntVal;
					}
					else if (warnOnWrongType)
					{
						LogWarning("Processed a KoreographyEvent with no IntPayload!  Leaving variable unchanged!");
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

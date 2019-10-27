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
	[Tooltip("Retrieves a String value from stored Koreography Event info!")]
	public class GetKoreographyEventPayloadText : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("The string variable into which to store the Text Payload value from the Koreography Event.")]
		public FsmString storeTextPayload;

		public bool warnOnWrongType = true;

		public override void OnEnter()
		{
			KoreographyEventInfo info = KoreographyEventInfo.CallbackInfo;

			if (info != null)
			{
				KoreographyEvent koreoEvent = info.koreoEvent;

				if (koreoEvent != null)
				{
					TextPayload pl = koreoEvent.Payload as TextPayload;
					if (pl != null)
					{
						storeTextPayload.Value = pl.TextVal;
					}
					else if (warnOnWrongType)
					{
						LogWarning("Processed a KoreographyEvent with no TextPayload!  Leaving variable unchanged!");
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

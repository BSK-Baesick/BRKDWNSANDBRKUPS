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
	[Tooltip("Receive Koreography Events when they are triggered in audio!")]
	public class KoreographyEventTrigger : FsmStateAction
	{
		#region PlayMaker Variables

		[RequiredField]
		[Title("Koreography Event ID")]
		[Tooltip("The Event ID of KoreographyTracks to which to subscribe for events.")]
		public FsmString eventID;

		[UIHint(UIHint.FsmEvent)]
		[Tooltip("FSM Event to send when a Koreography Event from a Koreography Track with the given Event ID is triggered.")]
		public FsmEvent sendEvent;

		[ActionSection("Optional")]

		[UIHint(UIHint.FsmObject)]
		[Tooltip("A specific Koreographer component with which to register for events.  If this is null, it will use the singleton default.")]
		[ObjectType(typeof(Koreographer))]
		public FsmObject targetKoreographer;

		#endregion
		#region Fields

		// Internal, locally managed info object.  
		KoreographyEventInfo info = new KoreographyEventInfo();

		#endregion
		#region PlayMaker Methods
		
		public override void Reset()
		{
			eventID = string.Empty;
			sendEvent = null;
			targetKoreographer = null;

			// Clear the event info, just to be safe.
			info.Reset();
		}

		public override void OnEnter()
		{
			if (targetKoreographer.Value != null)
			{
				Koreographer koreographer = targetKoreographer.Value as Koreographer;

				if (koreographer != null)
				{
					koreographer.RegisterForEventsWithTime(eventID.Value, OnKoreographyEventTriggered);
				}
				else
				{
					LogError("'Target Koreographer' value was set but does not contain a Koreographer component reference!  Please check that it was not removed.");
				}
			}
			else if (Koreographer.Instance != null)
			{
				Koreographer.Instance.RegisterForEventsWithTime(eventID.Value, OnKoreographyEventTriggered);
			}
			else
			{
				LogWarning("No Koreographer component found in the scene to register with!  Did you add a Koreographer component to a GameObject in the scene?");
			}
		}

		public override void OnExit()
		{
			if (targetKoreographer.Value != null)
			{
				Koreographer koreographer = targetKoreographer.Value as Koreographer;
				
				if (koreographer != null)
				{
					koreographer.UnregisterForEvents(eventID.Value, OnKoreographyEventTriggered);
				}
			}
			else if (Koreographer.Instance != null)
			{
				Koreographer.Instance.UnregisterForEvents(eventID.Value, OnKoreographyEventTriggered);
			}
		}

		#endregion
		#region Koreographer Callback

		void OnKoreographyEventTriggered(KoreographyEvent koreoEvent, int sampleTime, int sampleDelta, DeltaSlice deltaSlice)
		{

			info.koreoEvent = koreoEvent;
			info.sampleTime = sampleTime;
			info.sampleDelta = sampleDelta;
			info.deltaSlice = deltaSlice;

			// Store the current info into a global object.
			KoreographyEventInfo.SetCallbackInfo(info);

			// Trigger the event.  Listeners will access the global KoreographyEventInfo object.
			Fsm.Event(sendEvent);

			// Remove our event info from the global object.
			KoreographyEventInfo.ClearCallbackInfo();

			// Reset our local info, freeing any memory references.
			info.Reset();
		}

		#endregion
	}
}

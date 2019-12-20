#if CHRONOS_PLAYMAKER

using HutongGames.PlayMaker;
using System.Collections;


namespace Chronos.PlayMaker
{
    [ActionCategory("Chronos")]
    [Tooltip("Delays an event for a given amount of time after executing the next event.")]
    public class Wait : ChronosComponentAction<Timeline>
    {
        [RequiredField]
        [CheckForComponent(typeof(Timeline))]
        public FsmOwnerDefault gameObject;

        [RequiredField]
        public FsmFloat delay;

        [Title("Event")]
        public FsmEvent scheduledEvent;

        public override void Reset()
        {
            gameObject = null;
            scheduledEvent = null;
        }

        public override void OnEnter()
        {
            if (!UpdateCache(Fsm.GetOwnerDefaultTarget(gameObject))) return;
            Delay();
        }

        IEnumerator Delay()
        {
            yield return timeline.WaitForSeconds(delay.Value); // Wait for the delay    
            
            if (!FsmEvent.IsNullOrEmpty(scheduledEvent))
            {
                Fsm.Event(scheduledEvent);
            }
            else
            {
                Finish();
            }
        }
    }
}

#endif

using UnityEngine;
using HutongGames.PlayMaker;
using DG.Tweening;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("DOTween")]
    [Tooltip("Sends all tweens to the given position (calculating also eventual loop cycles)")]
    [HelpUrl("http://dotween.demigiant.com/documentation.php")]
    public class DOTweenControlMethodsGoToAll : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.FsmFloat)]
        [Tooltip("Time position to reach (if higher than the whole tween duration the tween will simply reach its end).")]
        public FsmFloat to;

        [UIHint(UIHint.FsmBool)]
        [Tooltip("If TRUE the tween will play after reaching the given position, otherwise it will be paused.")]
        public FsmBool andPlay;

        [ActionSection("Debug Options")]
        [UIHint(UIHint.FsmBool)]
        public FsmBool debugThis;

        public override void Reset()
        {
            base.Reset();

            to = new FsmFloat { UseVariable = false };
            andPlay = new FsmBool { UseVariable = false, Value = false };

            debugThis = new FsmBool { Value = false };
        }

        public override void OnEnter()
        {
            int numberOfTweensInvolved = DOTween.GotoAll(to.Value, andPlay.Value);

            if (debugThis.Value) Debug.Log("GameObject [" + State.Fsm.GameObjectName + "] FSM [" + State.Fsm.Name + "]  State [" + State.Name + "] - DOTween Control Methods Go To All - SUCCESS! - " + numberOfTweensInvolved + " tweens involved");

            Finish();
        }


    }
}

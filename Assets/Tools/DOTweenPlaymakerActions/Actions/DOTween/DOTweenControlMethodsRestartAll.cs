using UnityEngine;
using HutongGames.PlayMaker;
using DG.Tweening;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("DOTween")]
    [Tooltip("Restarts all tweens")]
    [HelpUrl("http://dotween.demigiant.com/documentation.php")]
    public class DOTweenControlMethodsRestartAll : FsmStateAction
    {
        [UIHint(UIHint.FsmBool)]
        [Tooltip("If TRUE includes the eventual tween delay, otherwise skips it.")]
        public FsmBool includeDelay;

        [ActionSection("Debug Options")]
        [UIHint(UIHint.FsmBool)]
        public FsmBool debugThis;

        public override void Reset()
        {
            base.Reset();

            includeDelay = new FsmBool { UseVariable = false, Value = true };

            debugThis = new FsmBool { Value = false };
        }

        public override void OnEnter()
        {
            int numberOfTweensRestarted = DOTween.RestartAll(includeDelay.Value);

            if (debugThis.Value) Debug.Log("GameObject [" + State.Fsm.GameObjectName + "] FSM [" + State.Fsm.Name + "]  State [" + State.Name + "] - DOTween Control Methods Restart All - SUCCESS! - Restarted " + numberOfTweensRestarted + " tweens");

            Finish();
        }


    }
}

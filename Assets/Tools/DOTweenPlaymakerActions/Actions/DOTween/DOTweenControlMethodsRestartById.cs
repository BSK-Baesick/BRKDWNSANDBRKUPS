using UnityEngine;
using HutongGames.PlayMaker;
using DG.Tweening;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("DOTween")]
    [Tooltip("Restarts all tweens with the given ID")]
    [HelpUrl("http://dotween.demigiant.com/documentation.php")]
    public class DOTweenControlMethodsRestartById : FsmStateAction
    {
        [Tooltip("Select the tween ID type to use")]
        public DOTweenActionsEnums.TweenId tweenIdType;

        [UIHint(UIHint.FsmString)]
        [Tooltip("Use a String as the tween ID")]
        public FsmString stringAsId;

        [UIHint(UIHint.Tag)]
        [Tooltip("Use a Tag as the tween ID")]
        public FsmString tagAsId;

        [UIHint(UIHint.FsmGameObject)]
        [Tooltip("Use a GameObject as the tween ID")]
        public FsmGameObject gameObjectAsId;

        [UIHint(UIHint.FsmBool)]
        [Tooltip("If TRUE includes the eventual tween delay, otherwise skips it.")]
        public FsmBool includeDelay;

        [ActionSection("Debug Options")]
        [UIHint(UIHint.FsmBool)]
        public FsmBool debugThis;

        public override void Reset()
        {
            base.Reset();

            stringAsId = new FsmString { UseVariable = false };
            tagAsId = new FsmString { UseVariable = false };
            gameObjectAsId = new FsmGameObject { UseVariable = false, Value = null };
            includeDelay = new FsmBool { UseVariable = false, Value = true };

            debugThis = new FsmBool { Value = false };
        }

        public override void OnEnter()
        {
            int numberOfTweensRestarted = 0;

            switch (tweenIdType)
            {
                case DOTweenActionsEnums.TweenId.UseString: if (string.IsNullOrEmpty(stringAsId.Value) == false) numberOfTweensRestarted = DOTween.Restart(stringAsId.Value, includeDelay.Value); break;
                case DOTweenActionsEnums.TweenId.UseTag: if (string.IsNullOrEmpty(tagAsId.Value) == false) numberOfTweensRestarted = DOTween.Restart(tagAsId.Value, includeDelay.Value); break;
                case DOTweenActionsEnums.TweenId.UseGameObject: if (gameObjectAsId.Value != null) numberOfTweensRestarted = DOTween.Restart(gameObjectAsId.Value, includeDelay.Value); break;
            }

            if (debugThis.Value) Debug.Log("GameObject [" + State.Fsm.GameObjectName + "] FSM [" + State.Fsm.Name + "]  State [" + State.Name + "] - DOTween Control Methods Restart By Id - SUCCESS! - Restarted " + numberOfTweensRestarted + " tweens");

            Finish();
        }


    }
}

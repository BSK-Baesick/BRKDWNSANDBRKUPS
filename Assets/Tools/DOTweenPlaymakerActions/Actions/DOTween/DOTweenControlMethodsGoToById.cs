using UnityEngine;
using HutongGames.PlayMaker;
using DG.Tweening;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("DOTween")]
    [Tooltip("Sends all tweens with the given ID to the given position (calculating also eventual loop cycles)")]
    [HelpUrl("http://dotween.demigiant.com/documentation.php")]
    public class DOTweenControlMethodsGoToById : FsmStateAction
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

            stringAsId = new FsmString { UseVariable = false };
            tagAsId = new FsmString { UseVariable = false };
            gameObjectAsId = new FsmGameObject { UseVariable = false, Value = null };
            to = new FsmFloat { UseVariable = false };
            andPlay = new FsmBool { UseVariable = false, Value = false };

            debugThis = new FsmBool { Value = false };
        }

        public override void OnEnter()
        {
            int numberOfTweensInvolved = 0;

            switch (tweenIdType)
            {
                case DOTweenActionsEnums.TweenId.UseString: if (string.IsNullOrEmpty(stringAsId.Value) == false) numberOfTweensInvolved = DOTween.Goto(stringAsId.Value, to.Value, andPlay.Value); break;
                case DOTweenActionsEnums.TweenId.UseTag: if (string.IsNullOrEmpty(tagAsId.Value) == false) numberOfTweensInvolved = DOTween.Goto(tagAsId.Value, to.Value, andPlay.Value); break;
                case DOTweenActionsEnums.TweenId.UseGameObject: if (gameObjectAsId.Value != null) numberOfTweensInvolved = DOTween.Goto(gameObjectAsId.Value, to.Value, andPlay.Value); break;
            }

            if (debugThis.Value) Debug.Log("GameObject [" + State.Fsm.GameObjectName + "] FSM [" + State.Fsm.Name + "]  State [" + State.Name + "] - DOTween Control Methods Go To By Id - SUCCESS! - " + numberOfTweensInvolved + " tweens involved");

            Finish();
        }


    }
}

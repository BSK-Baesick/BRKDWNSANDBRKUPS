using UnityEngine;
using HutongGames.PlayMaker;
using DG.Tweening;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("DOTween")]
    [Tooltip(" Checks all active tweens to find and remove eventually invalid ones (usually because their targets became NULL) and returns the total number of invalid tweens found and removed. Automatically called when loading a new scene if DG.Tweening.DOTween.useSafeMode is TRUE. BEWARE: this is a slightly expensive operation so use it with care")]
    [HelpUrl("http://dotween.demigiant.com/documentation.php")]
    public class DOTweenAdditionalMethodsValidate : FsmStateAction
    {
        [ActionSection("Debug Options")]
        public FsmBool debugThis;

        public override void Reset()
        {
            base.Reset();

            debugThis = new FsmBool { Value = false };
        }

        public override void OnEnter()
        {
            DOTween.Validate();

            if (debugThis.Value) Debug.Log("GameObject [" + State.Fsm.GameObjectName + "] FSM [" + State.Fsm.Name + "]  State [" + State.Name + "] - DOTween Additional Methods Validate - SUCCESS!");

            Finish();
        }
    }
}

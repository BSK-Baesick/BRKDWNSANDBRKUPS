using UnityEngine;
using HutongGames.PlayMaker;
using DG.Tweening;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("DOTween")]
    [Tooltip("Kills all tweens. A tween is killed automatically when it reaches completion (unless you prevent it using SetAutoKill(false)), but you can use this method to kill it sooner if you don't need it anymore.")]
    [HelpUrl("http://dotween.demigiant.com/documentation.php")]
    public class DOTweenControlMethodsKillAll : FsmStateAction
    {
        [UIHint(UIHint.FsmBool)]
        [Tooltip("If TRUE instantly completes the tween before killing it.")]
        public FsmBool complete;

        [Tooltip("KillAll only > Eventual ids to exclude from the operation.")]
        public FsmString[] idsToExclude;

        [ActionSection("Debug Options")]
        [UIHint(UIHint.FsmBool)]
        public FsmBool debugThis;

        public override void Reset()
        {
            base.Reset();

            complete = new FsmBool { UseVariable = false, Value = false };

            debugThis = new FsmBool { Value = false };
        }

        public override void OnEnter()
        {
            int numberOfTweensKilled = 0;

            if (idsToExclude.Length > 0)
            {
                string[] idList = new string[idsToExclude.Length];

                for (int i = 0; i < idList.Length; i++)
                {
                    idList[i] = idsToExclude[i].Value;
                }

                numberOfTweensKilled = DOTween.KillAll(complete.Value, idList);
            }
            else
            {
                numberOfTweensKilled = DOTween.KillAll(complete.Value);
            }



            if (debugThis.Value) Debug.Log("GameObject [" + State.Fsm.GameObjectName + "] FSM [" + State.Fsm.Name + "]  State [" + State.Name + "] - DOTween Control Methods Kill All - SUCCESS! - Killed " + numberOfTweensKilled + " tweens");

            Finish();
        }


    }
}

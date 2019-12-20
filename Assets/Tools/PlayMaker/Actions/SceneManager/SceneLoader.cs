using System;
using UnityEngine;
using HutongGames.PlayMaker;
using BaesickEntertainment;

namespace HutongGames.PlayMaker.Actions
{
    
    [ActionCategory(ActionCategory.Scene)]
    [Note("This action handles the Loading Screen logic in loading the next scene.")]
    public class SceneLoader : FsmStateAction
    {
        
        public FsmOwnerDefault sceneManager;

        [RequiredField]
        public FsmString sceneName;

        public FsmFloat delayInterval = 1;
        
        SplashScreenSceneManager theScript;
        
        public override void OnEnter()
        {
            GameObject go = Fsm.GetOwnerDefaultTarget(sceneManager);
            theScript = go.GetComponent<SplashScreenSceneManager>();
            sceneName.Value = theScript.sceneName;
            delayInterval.Value = theScript.delayInterval;
            Finish();
        }
    }
}
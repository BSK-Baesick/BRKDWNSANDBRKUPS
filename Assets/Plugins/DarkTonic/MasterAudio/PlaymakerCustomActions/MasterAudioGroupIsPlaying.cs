using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using DarkTonic.MasterAudio;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Check if a Master Audio Sound Group is playing, then fire events depending on that")]
public class MasterAudioGroupIsPlaying : FsmStateAction{
	[RequiredField]
	[Tooltip("Name of Master Audio Sound Group")]
	public FsmString soundGroupName;
	     
	[Tooltip("Event to send when the AudioClip is playing.")]
	public FsmEvent isPlayingEvent;

	[Tooltip("Event to send when the AudioClip is not playing.")]
	public FsmEvent notPlayingEvent;
		
	public override void OnUpdate (){
		if(!string.IsNullOrEmpty(soundGroupName.Value)){
			if(MasterAudio.GetGroupInfo(soundGroupName.Value).Group.ActiveVoices > 0){
				if(isPlayingEvent != null){
					Fsm.Event(isPlayingEvent);
					Finish();
              	}else{
	                if(notPlayingEvent != null){
 	                   Fsm.Event(notPlayingEvent);
	                   Finish();
                    }
                }
			}
        }
    }
}	
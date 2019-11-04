using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using DarkTonic.MasterAudio;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Play a Sound in Master Audio and wait till sound is finished, then fire events")]
public class MasterAudioPlaySoundWithEvent : FsmStateAction{
	[RequiredField]
	[Tooltip("The GameObject to use for sound position.")]
	public FsmOwnerDefault gameObject;

	[RequiredField]
	[Tooltip("Name of Master Audio Sound Group")]
	public FsmString soundGroupName;

	[Tooltip("Name of specific variation (optional)")]
	public FsmString variationName;
		
	[RequiredField]
	[HasFloatSlider(0,1)]
	public FsmFloat volume = new FsmFloat(1f);

	[HasFloatSlider(0,10)]
	public FsmFloat delaySound = new FsmFloat(0f);

	public FsmBool useThisLocation = new FsmBool(true);
	
	public FsmBool attachToGameObject = new FsmBool(false);

	public FsmBool useFixedPitch = new FsmBool(false);

	[TooltipAttribute("Fixed Pitch will be used only if 'Use Fixed Pitch' is checked above.")]
	[HasFloatSlider(-3,3)]
	public FsmFloat fixedPitch = new FsmFloat(1f);

	[Tooltip("Event to send when the AudioClip finishes playing.")]
	public FsmEvent finishedEvent;

	PlaySoundResult sound;

	public override void Reset(){
	    volume = 1f;
        finishedEvent = null;
	}
		
	public override void OnEnter(){
		var groupName = soundGroupName.Value;
		var childName = variationName.Value;
		var willAttach = attachToGameObject.Value;
		var use3dLocation = useThisLocation.Value;
		var vol = volume.Value;
		var fDelay = delaySound.Value;
		float? pitch = fixedPitch.Value;
		if (!useFixedPitch.Value) {
			pitch = null;
		}
		
		if (string.IsNullOrEmpty(childName)) {
			childName = null;
		}
		
		Transform trans;
		if (gameObject.GameObject.Value != null) {
			trans = gameObject.GameObject.Value.transform;
		} else {
			trans = Owner.transform;
		}

		if (!use3dLocation && !willAttach) {
			sound = MasterAudio.PlaySound(groupName, vol, pitch, fDelay, childName);
		} else if (!willAttach) {
			sound = MasterAudio.PlaySound3DAtVector3(groupName, trans.position, vol, pitch, fDelay, childName);
		} else {
			sound = MasterAudio.PlaySound3DFollowTransform(groupName, trans, vol, pitch, fDelay, childName);
		}

        if(sound != null){
			return;
        }

		Finish();
    }

	public override void OnUpdate (){
	    if(sound == null){
	        Finish();
			return;
        }

		if(finishedEvent != null){
		    if (!sound.ActingVariation.IsPlaying){
		        Fsm.Event(finishedEvent);
			    Finish();
			}
		}
    }
}
	
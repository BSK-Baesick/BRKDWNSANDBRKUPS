using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Play a Sound in Master Audio")]
public class MasterAudioPlaySound : FsmStateAction {
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
	
	public override void OnEnter() {
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
			MasterAudio.PlaySoundAndForget(groupName, vol, pitch, fDelay, childName);
		} else if (!willAttach) {
			MasterAudio.PlaySound3DAtVector3AndForget(groupName, trans.position, vol, pitch, fDelay, childName);
		} else {
			MasterAudio.PlaySound3DFollowTransformAndForget(groupName, trans, vol, pitch, fDelay, childName);
		}
		
		Finish();
	}
	
	public override void Reset() {
		gameObject = null;
		soundGroupName = new FsmString(string.Empty);
		variationName = new FsmString(string.Empty);
		volume = new FsmFloat(1f);
		delaySound = new FsmFloat(0f);
		useThisLocation = new FsmBool(true);
		attachToGameObject = new FsmBool(false);
		useFixedPitch = new FsmBool(false);
		fixedPitch = new FsmFloat(1f);
	}
}

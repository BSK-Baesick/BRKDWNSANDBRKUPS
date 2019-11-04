using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Turn music ducking on or off in Master Audio.")]
public class MasterAudioDuckingToggle : FsmStateAction {
    [RequiredField]
    [Tooltip("Check this to enable ducking, uncheck it to disable ducking.")]
	public FsmBool enableDucking;
	
	public override void OnEnter() {
		MasterAudio.Instance.EnableMusicDucking = enableDucking.Value;
		
		Finish();
	}
	
	public override void Reset() {
		enableDucking = new FsmBool(false);
	}
}

using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Stop all sound effects in Master Audio. Does not include Playlists.")]
public class MasterAudioMixerStop : FsmStateAction {
	public override void OnEnter() {
		MasterAudio.StopMixer();
		
		Finish();
	}
	
	public override void Reset() {
	}
}

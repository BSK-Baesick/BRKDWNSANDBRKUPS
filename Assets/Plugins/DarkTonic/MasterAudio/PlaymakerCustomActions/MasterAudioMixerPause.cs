using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Pause all sound effects in Master Audio. Does not include Playlists.")]
public class MasterAudioMixerPause : FsmStateAction {
	public override void OnEnter() {
		MasterAudio.PauseMixer();
		
		Finish();
	}
	
	public override void Reset() {
	}
}

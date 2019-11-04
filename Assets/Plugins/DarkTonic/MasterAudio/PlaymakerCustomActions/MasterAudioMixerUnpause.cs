using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Unpause all sound effects in Master Audio. Does not include Playlists.")]
public class MasterAudioMixerUnpause : FsmStateAction {
	public override void OnEnter() {
		MasterAudio.UnpauseMixer();
		
		Finish();
	}
	
	public override void Reset() {
	}
}

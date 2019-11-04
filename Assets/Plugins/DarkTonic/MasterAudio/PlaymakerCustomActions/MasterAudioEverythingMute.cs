using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Mute all sound effects and Playlists in Master Audio.")]
public class MasterAudioEverythingMute : FsmStateAction {
	public override void OnEnter() {
		MasterAudio.MuteEverything();
		
		Finish();
	}
	
	public override void Reset() {
	}
}

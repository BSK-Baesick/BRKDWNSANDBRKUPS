using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Unmute all sound effects and Playlists in Master Audio.")]
public class MasterAudioEverythingUnmute : FsmStateAction {
	public override void OnEnter() {
		MasterAudio.UnmuteEverything();
		
		Finish();
	}
	
	public override void Reset() {
	}
}

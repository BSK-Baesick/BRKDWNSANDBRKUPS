using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Pause all sound effects and Playlists in Master Audio.")]
public class MasterAudioEverythingPause : FsmStateAction {
	public override void OnEnter() {
		MasterAudio.PauseEverything();
		
		Finish();
	}
	
	public override void Reset() {
	}
}

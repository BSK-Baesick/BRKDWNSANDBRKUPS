using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Unpause all sound effects and Playlists in Master Audio.")]
public class MasterAudioEverythingUnpause : FsmStateAction {
	public override void OnEnter() {
		MasterAudio.UnpauseEverything();
		
		Finish();
	}
	
	public override void Reset() {
	}
}

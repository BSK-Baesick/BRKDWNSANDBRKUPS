using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Stop all sound effects and Playlists in Master Audio.")]
public class MasterAudioEverythingStop : FsmStateAction {
	public override void OnEnter() {
		MasterAudio.StopEverything();
		
		Finish();
	}
	
	public override void Reset() {
	}
}

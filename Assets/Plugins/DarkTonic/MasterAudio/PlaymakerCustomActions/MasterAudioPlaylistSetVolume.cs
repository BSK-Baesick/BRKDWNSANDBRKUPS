using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Set the Playlist Master volume in Master Audio to a specific volume.")]
public class MasterAudioPlaylistSetVolume : FsmStateAction {
	[RequiredField]
	[HasFloatSlider(0, 1)]
	[Tooltip("Playlist New Volume")]
	public FsmFloat newVolume;

    [Tooltip("Repeat every frame while the state is active.")]
    public bool everyFrame;
	
	public override void OnEnter() {
		SetVolume();
		
		if (!everyFrame) {
			Finish();
		}
	}
	
	public override void OnUpdate() {
		SetVolume();
	}
	
	private void SetVolume() {
		MasterAudio.PlaylistMasterVolume = newVolume.Value;
	}
	
	public override void Reset() {
		newVolume = new FsmFloat();
	}
}

using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Set master volume level in Master Audio")]
public class MasterAudioSetMasterVolume : FsmStateAction {
    [RequiredField]
	[HasFloatSlider(0,1)]
	public FsmFloat volume = new FsmFloat(1f);

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
		MasterAudio.MasterVolumeLevel = volume.Value;
	}
	
	public override void Reset() {
		volume = new FsmFloat(1f);
	}
}

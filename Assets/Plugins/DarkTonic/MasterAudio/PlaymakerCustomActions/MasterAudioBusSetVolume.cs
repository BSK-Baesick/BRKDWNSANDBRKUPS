using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Set a single bus volume level in Master Audio")]
public class MasterAudioBusSetVolume : FsmStateAction {
	[Tooltip("Check this to perform action on all Buses")]
	public FsmBool allBuses;	
	
    [Tooltip("Name of Master Audio Bus")]
	public FsmString busName;
	
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
		if (!allBuses.Value && string.IsNullOrEmpty(busName.Value)) {
			Debug.LogError("You must either check 'All Buses' or enter the Bus Name");
			return;
		}

		if (allBuses.Value) {
			var busNames = MasterAudio.RuntimeBusNames;
			for (var i = 0; i < busNames.Count; i++) {
				MasterAudio.SetBusVolumeByName(busNames[i], volume.Value);
			}
		} else {
			MasterAudio.SetBusVolumeByName(busName.Value, volume.Value);
		}
	}
	
	public override void Reset() {
		allBuses = new FsmBool(false);
		busName = new FsmString(string.Empty);
		volume = new FsmFloat(1f);
	}
}

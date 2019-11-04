using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Fade a Bus in Master Audio to a specific volume over X seconds.")]
public class MasterAudioBusFade : FsmStateAction {
	[Tooltip("Check this to perform action on all Buses")]
	public FsmBool allBuses;	
	
    [Tooltip("Name of Master Audio Bus")]
	public FsmString busName;

    [RequiredField]
	[HasFloatSlider(0, 1)]
	[Tooltip("Target Bus volume")]
	public FsmFloat targetVolume;
	
    [RequiredField]
	[HasFloatSlider(0, 10)]
	[Tooltip("Amount of time to complete fade (seconds)")]
	public FsmFloat fadeTime;

	[Tooltip("Check this box if you want to stop the Bus after the fade is complete")]
	public FsmBool stopBusAfterFade;

	[Tooltip("Check this box if you want to restore the original pre-fade volume after the fade is complete")]
	public FsmBool restoreVolumeAfterFade;

	public override void OnEnter() {
		if (!allBuses.Value && string.IsNullOrEmpty(busName.Value)) {
			Debug.LogError("You must either check 'All Buses' or enter the Bus Name");
			return;
		}
		
		if (allBuses.Value) {
			var busNames = MasterAudio.RuntimeBusNames;
			for (var i = 0; i < busNames.Count; i++) {
				MasterAudio.FadeBusToVolume(busNames[i], targetVolume.Value, fadeTime.Value, null, stopBusAfterFade.Value, restoreVolumeAfterFade.Value);
			}
		} else {
			MasterAudio.FadeBusToVolume(busName.Value, targetVolume.Value, fadeTime.Value, null, stopBusAfterFade.Value, restoreVolumeAfterFade.Value);
		}
		
		Finish();
	}
	
	public override void Reset() {
		allBuses = new FsmBool(false);
		busName = new FsmString(string.Empty);
		targetVolume = new FsmFloat();
		fadeTime = new FsmFloat();
	}
}

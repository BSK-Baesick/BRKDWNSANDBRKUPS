using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Fade a Sound Group in Master Audio to a specific volume over X seconds.")]
public class MasterAudioGroupFade : FsmStateAction {
	[Tooltip("Check this to perform action on all Sound Groups")]
	public FsmBool allGroups;	

    [Tooltip("Name of Master Audio Sound Group")]
	public FsmString soundGroupName;

    [RequiredField]
	[HasFloatSlider(0, 1)]
	[Tooltip("Target Sound Group volume")]
	public FsmFloat targetVolume;
	
    [RequiredField]
	[HasFloatSlider(0, 10)]
	[Tooltip("Amount of time to complete fade (seconds)")]
	public FsmFloat fadeTime;

	[Tooltip("Check this box if you want to stop the Group after the fade is complete")]
	public FsmBool stopGroupAfterFade;
	
	[Tooltip("Check this box if you want to restore the original pre-fade volume after the fade is complete")]
	public FsmBool restoreVolumeAfterFade;

	public override void OnEnter() {
		if (!allGroups.Value && string.IsNullOrEmpty(soundGroupName.Value)) {
			Debug.LogError("You must either check 'All Groups' or enter the Sound Group Name");
			return;
		}

		if (allGroups.Value) {
			var groupNames = MasterAudio.RuntimeSoundGroupNames;
			for (var i = 0; i < groupNames.Count; i++) {
				MasterAudio.FadeSoundGroupToVolume(groupNames[i], targetVolume.Value, fadeTime.Value, null, stopGroupAfterFade.Value, restoreVolumeAfterFade.Value);
			}
		} else {
			MasterAudio.FadeSoundGroupToVolume(soundGroupName.Value, targetVolume.Value, fadeTime.Value, null, stopGroupAfterFade.Value, restoreVolumeAfterFade.Value);
		}
		
		Finish();
	}
	
	public override void Reset() {
		allGroups = new FsmBool(false);
		soundGroupName = new FsmString(string.Empty);
		targetVolume = new FsmFloat();
		fadeTime = new FsmFloat();
	}
}

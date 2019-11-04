using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Fade all of a Sound Group in Master Audio to zero volume over X seconds")]
public class MasterAudioFadeOutAllOfSound : FsmStateAction {
	[Tooltip("Check this to perform action on all Sound Groups")]
	public FsmBool allGroups;	

    [Tooltip("Name of Master Audio Sound Group")]
	public FsmString soundGroupName;
	
    [RequiredField]
	[HasFloatSlider(0, 10)]
	[Tooltip("Amount of time to complete fade (seconds)")]
	public FsmFloat fadeTime;
	
	public override void OnEnter() {
		if (!allGroups.Value && string.IsNullOrEmpty(soundGroupName.Value)) {
			Debug.LogError("You must either check 'All Groups' or enter the Sound Group Name");
			return;
		}

		if (allGroups.Value) {
			var groupNames = MasterAudio.RuntimeSoundGroupNames;
			for (var i = 0; i < groupNames.Count; i++) {
				MasterAudio.FadeOutAllOfSound(groupNames[i], fadeTime.Value);
			}
		} else {
			MasterAudio.FadeOutAllOfSound(soundGroupName.Value, fadeTime.Value);
		}
		
		Finish();
	}
	
	public override void Reset() {
		allGroups = new FsmBool(false);
		soundGroupName = new FsmString(string.Empty);
		fadeTime = new FsmFloat();
	}
}

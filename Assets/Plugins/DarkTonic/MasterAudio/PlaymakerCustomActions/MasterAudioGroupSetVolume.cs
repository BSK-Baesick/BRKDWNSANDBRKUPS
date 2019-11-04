using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Set a single Sound Group volume level in Master Audio")]
public class MasterAudioGroupSetVolume : FsmStateAction {
	[Tooltip("Check this to perform action on all Sound Groups")]
	public FsmBool allGroups;	

    [Tooltip("Name of Master Audio Sound Group")]
	public FsmString soundGroupName;
	
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
		if (!allGroups.Value && string.IsNullOrEmpty(soundGroupName.Value)) {
			Debug.LogError("You must either check 'All Groups' or enter the Sound Group Name");
			return;
		}

		if (allGroups.Value) {
			var groupNames = MasterAudio.RuntimeSoundGroupNames;
			for (var i = 0; i < groupNames.Count; i++) {
				MasterAudio.SetGroupVolume(groupNames[i], volume.Value);
			}
		} else {
			MasterAudio.SetGroupVolume(soundGroupName.Value, volume.Value);
		}
	}
	
	public override void Reset() {
		allGroups = new FsmBool(false);
		soundGroupName = new FsmString(string.Empty);
		volume = new FsmFloat(1f);
	}
}

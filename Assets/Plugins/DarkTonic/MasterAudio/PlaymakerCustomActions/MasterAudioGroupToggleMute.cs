using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Toggle the mute button of a Sound Group in Master Audio")]
public class MasterAudioGroupToggleMute : FsmStateAction {
	[Tooltip("Check this to perform action on all Sound Groups")]
	public FsmBool allGroups;	

    [Tooltip("Name of Master Audio Sound Group")]
	public FsmString soundGroupName;
	
	public override void OnEnter() {
		if (!allGroups.Value && string.IsNullOrEmpty(soundGroupName.Value)) {
			Debug.LogError("You must either check 'All Groups' or enter the Sound Group Name");
			return;
		}

		if (allGroups.Value) {
			var groupNames = MasterAudio.RuntimeSoundGroupNames;
			
			var groupName = string.Empty;
			for (var i = 0; i < groupNames.Count; i++) {
				groupName = groupNames[i];
				
				var grp = MasterAudio.GrabGroup(groupName);
				if (grp != null) {
					if (grp.isMuted) {
						MasterAudio.UnmuteGroup(groupName);
					} else {
						MasterAudio.MuteGroup(groupName);
					}
				}
			}
		} else {
			var grp = MasterAudio.GrabGroup(soundGroupName.Value);
			if (grp != null) {
				if (grp.isMuted) {
					MasterAudio.UnmuteGroup(soundGroupName.Value);
				} else {
					MasterAudio.MuteGroup(soundGroupName.Value);
				}
			}
		}
		
		Finish();
	}
	
	public override void Reset() {
		allGroups = new FsmBool(false);
		soundGroupName = new FsmString(string.Empty);
	}
}

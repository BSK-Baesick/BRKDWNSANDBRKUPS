using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Stop sounds made by a Transform in Master Audio")]
public class MasterAudioStopTransformSound : FsmStateAction {
	[RequiredField]
	[Tooltip("The Game Object to stop sounds made by")]
	public FsmOwnerDefault gameObject = new FsmOwnerDefault();
	
	[Tooltip("Check this to perform action on all Sound Groups")]
	public FsmBool allGroups;	

    [Tooltip("Name of Master Audio Sound Group")]
	public FsmString soundGroupName;
	
	public override void OnEnter() {
		if (!allGroups.Value && string.IsNullOrEmpty(soundGroupName.Value)) {
			Debug.LogError("You must either check 'All Groups' or enter the Sound Group Name");
			return;
		}
		
		GameObject go = gameObject.OwnerOption == OwnerDefaultOption.UseOwner ? this.Owner.gameObject : gameObject.GameObject.Value.gameObject;
		
		if (allGroups.Value) {
			MasterAudio.StopAllSoundsOfTransform(go.transform);
		} else {
			if (string.IsNullOrEmpty(soundGroupName.Value)) {
				Debug.LogError("You must either check 'All Groups' or enter the Sound Group Name");
			}
			MasterAudio.StopSoundGroupOfTransform(go.transform, soundGroupName.Value);
		}
		
		Finish();
	}
	
	public override void Reset() {
		gameObject = new FsmOwnerDefault();
		allGroups = new FsmBool(false);
		soundGroupName = new FsmString(string.Empty);
	}
}

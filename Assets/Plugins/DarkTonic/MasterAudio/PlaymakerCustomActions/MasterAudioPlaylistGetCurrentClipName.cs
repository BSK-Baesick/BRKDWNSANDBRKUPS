using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Get the name of the currently playing Audio Clip in a Playlist in Master Audio")]
public class MasterAudioPlaylistGetCurrentClipName : FsmStateAction {
	[Tooltip("Name of Playlist Controller. Not required if you only have one Playlist Controller.")]
	public FsmString playlistControllerName;
	
	[Tooltip("Name of Variable to store the current clip name in.")]
	[RequiredField]
	[UIHint(UIHint.Variable)]
	public FsmString storeResult;
	
	public override void OnEnter() {
		PlaylistController controller = null;
		
		if (!string.IsNullOrEmpty(playlistControllerName.Value)) {
			controller = PlaylistController.InstanceByName(playlistControllerName.Value);
		} else {
			controller = MasterAudio.OnlyPlaylistController;
		}
		
		var clip = controller.CurrentPlaylistClip;
		
		storeResult.Value = clip == null ? string.Empty : clip.name;
				
		Finish();
	}
	
	public override void Reset() {
		playlistControllerName = new FsmString(string.Empty);
		storeResult = new FsmString(string.Empty);
	}
}

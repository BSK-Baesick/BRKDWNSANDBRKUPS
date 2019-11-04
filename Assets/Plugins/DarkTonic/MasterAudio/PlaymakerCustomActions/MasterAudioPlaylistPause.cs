using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Pause a Playlist in Master Audio")]
public class MasterAudioPlaylistPause : FsmStateAction {
	[Tooltip("Check this to perform action on all Playlist Controllers")]
	public FsmBool allPlaylistControllers;	

	[Tooltip("Name of Playlist Controller containing the Playlist. Not required if you only have one Playlist Controller.")]
	public FsmString playlistControllerName;

	public override void OnEnter() {
		if (allPlaylistControllers.Value) {
			var pcs = PlaylistController.Instances;
			
			for (var i = 0; i < pcs.Count; i++) {
				MasterAudio.PausePlaylist(pcs[i].name);
			}
		} else {
			if (string.IsNullOrEmpty(playlistControllerName.Value)) {
				MasterAudio.PausePlaylist();
			} else {
				MasterAudio.PausePlaylist(playlistControllerName.Value);
			}
		}
		
		Finish();
	}
	
	public override void Reset() {
		allPlaylistControllers = new FsmBool(false);
		playlistControllerName = new FsmString(string.Empty);
	}
}

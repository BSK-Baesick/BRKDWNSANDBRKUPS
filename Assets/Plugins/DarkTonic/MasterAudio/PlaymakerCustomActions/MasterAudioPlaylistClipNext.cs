using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Play the next clip in a Playlist in Master Audio")]
public class MasterAudioPlaylistClipNext : FsmStateAction {
	[Tooltip("Check this to perform action on all Playlist Controllers")]
	public FsmBool allPlaylistControllers;	

	[Tooltip("Name of Playlist Controller containing the Playlist. Not required if you only have one Playlist Controller.")]
	public FsmString playlistControllerName;

	public override void OnEnter() {
		if (allPlaylistControllers.Value) {
			var pcs = PlaylistController.Instances;
			
			for (var i = 0; i < pcs.Count; i++) {
				MasterAudio.TriggerNextPlaylistClip(pcs[i].name);
			}
		} else {
			if (string.IsNullOrEmpty(playlistControllerName.Value)) {
				MasterAudio.TriggerNextPlaylistClip();
			} else {
				MasterAudio.TriggerNextPlaylistClip(playlistControllerName.Value);
			}
		}
		
		Finish();
	}
	
	public override void Reset() {
		allPlaylistControllers = new FsmBool(false);
		playlistControllerName = new FsmString(string.Empty);
	}
}

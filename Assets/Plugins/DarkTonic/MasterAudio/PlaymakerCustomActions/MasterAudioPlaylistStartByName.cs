using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Start a Playlist by name in Master Audio")]
public class MasterAudioPlaylistStartByName : FsmStateAction {
    [Tooltip("Name of Playlist Controller to use. Not required if you only have one.")]
	public FsmString playlistControllerName;

	[RequiredField]
    [Tooltip("Name of playlist to start")]
	public FsmString playlistName;

	public override void OnEnter() {
		if (string.IsNullOrEmpty(playlistControllerName.Value)) {
			MasterAudio.StartPlaylist(playlistName.Value);
		} else {
			MasterAudio.StartPlaylist(playlistControllerName.Value, playlistName.Value);
		}
		
		Finish();
	}
	
	public override void Reset() {
		playlistControllerName = new FsmString(string.Empty);
		playlistName = new FsmString(string.Empty);
	}
}

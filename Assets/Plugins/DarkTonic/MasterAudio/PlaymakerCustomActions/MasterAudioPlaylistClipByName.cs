using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Play a clip in the current Playlist by name in Master Audio")]
public class MasterAudioPlaylistClipByName : FsmStateAction {
    [Tooltip("Name of Playlist Controller to use. Not required if you only have one.")]
	public FsmString playlistControllerName;

    [RequiredField]
    [Tooltip("Name of playlist clip to play.")]
	public FsmString clipName; 

	public override void OnEnter() {
		if (string.IsNullOrEmpty(playlistControllerName.Value)) {
			MasterAudio.TriggerPlaylistClip(clipName.Value);
		} else {
			MasterAudio.TriggerPlaylistClip(playlistControllerName.Value, clipName.Value);
		}
		
		Finish();
	}
	
	public override void Reset() {
		clipName = new FsmString(string.Empty);
		playlistControllerName = new FsmString(string.Empty);
	}
}

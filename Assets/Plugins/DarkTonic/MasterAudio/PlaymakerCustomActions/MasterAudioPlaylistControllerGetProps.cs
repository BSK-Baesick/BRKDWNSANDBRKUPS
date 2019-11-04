using DarkTonic.MasterAudio;
using HutongGames.PlayMaker;
using UnityEngine;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Get properties of a Playlist Controller in Master Audio.")]
// ReSharper disable once CheckNamespace
public class MasterAudioPlaylistControllerGetProps : FsmStateAction {
    [Tooltip("Name of Playlist Controller. Not required if you only have one Playlist Controller.")]
    public FsmString PlaylistControllerName;

    [Tooltip("Name of Variable to store the controller volume name in.")]
    [UIHint(UIHint.Variable)]
    public FsmFloat ControllerVolume;

    [Tooltip("Name of Variable to store the controller's current audio clip pitch in.")]
    [UIHint(UIHint.Variable)]
    public FsmFloat CurrentAudioClipPitch;

    [Tooltip("Name of Variable to store the controller's current audio clip volume in.")]
    [UIHint(UIHint.Variable)]
    public FsmFloat CurrentAudioClipVolume;

    public override void OnEnter() {
        if (string.IsNullOrEmpty(PlaylistControllerName.Value) && PlaylistController.Instances.Count > 1) {
            Debug.LogError("You must enter the Playlist Controller Name when you have more than one.");
            return;
        }

        var controller = PlaylistController.InstanceByName(PlaylistControllerName.Value);
        if (controller == null) {
            Debug.LogError("The Playlist Controller '" + PlaylistControllerName.Value + "' was not found.");
            return;
        }

        if (ControllerVolume.UsesVariable) {
            ControllerVolume.Value = controller.PlaylistVolume;
        }

        var aud = controller.ActiveAudioSource;

        if (CurrentAudioClipPitch.UsesVariable) {
            CurrentAudioClipPitch.Value = aud == null ? 0 : aud.pitch;
        }

        if (CurrentAudioClipVolume.UsesVariable) {
            CurrentAudioClipVolume.Value = aud == null ? 0 : aud.volume;
        }

        Finish();
    }

    public override void Reset() {
        PlaylistControllerName = new FsmString(string.Empty);
        ControllerVolume = new FsmFloat();
        CurrentAudioClipPitch = new FsmFloat();
        CurrentAudioClipVolume = new FsmFloat();
    }
}

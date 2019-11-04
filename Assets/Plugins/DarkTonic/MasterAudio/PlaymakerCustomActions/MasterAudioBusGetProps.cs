using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Get properties of a Bus in Master Audio.")]
// ReSharper disable once CheckNamespace
public class MasterAudioBusGetProps : FsmStateAction {
    [RequiredField]
    [Tooltip("Name of Master Audio Bus")]
    public FsmString busName;

    [Tooltip("Name of Variable to store the bus volume name in.")]
    [UIHint(UIHint.Variable)]
    public FsmFloat BusVolume;

    [Tooltip("Name of Variable to store the bus muted status in.")]
    [UIHint(UIHint.Variable)]
    public FsmBool BusIsMuted;

    [Tooltip("Name of Variable to store the bus soloed status in.")]
    [UIHint(UIHint.Variable)]
    public FsmBool BusIsSoloed;

    [Tooltip("Name of Variable to store the bus voice limit in.")]
    [UIHint(UIHint.Variable)]
    public FsmInt VoiceLimit;

    [Tooltip("Name of Variable to store the bus active voice count in.")]
    [UIHint(UIHint.Variable)]
    public FsmInt ActiveVoices;

    [Tooltip("Name of Variable to store the bus voice limit reached status in.")]
    [UIHint(UIHint.Variable)]
    public FsmBool VoiceLimitReached;

    public override void OnEnter() {
        if (string.IsNullOrEmpty(busName.Value)) {
            Debug.LogError("You must enter the Bus Name");
            return;
        }

        var aBus = MasterAudio.GrabBusByName(busName.Value);
		if (aBus == null) {
			Debug.LogError("The bus '" + busName.Value + "' was not found.");
			return;
		}

		if (BusVolume.UsesVariable) {
            BusVolume.Value = aBus.volume;
        }

        if (BusIsMuted.UsesVariable) {
            BusIsMuted.Value = aBus.isMuted;
        }

        if (BusIsSoloed.UsesVariable) {
            BusIsSoloed.Value = aBus.isMuted;
        }

        if (VoiceLimit.UsesVariable) {
            VoiceLimit.Value = aBus.voiceLimit;
        }

        if (ActiveVoices.UsesVariable) {
            ActiveVoices.Value = aBus.ActiveVoices;
        }

        if (VoiceLimitReached.UsesVariable) {
            VoiceLimitReached.Value = aBus.BusVoiceLimitReached;
        }

        Finish();
    }

    public override void Reset() {
        busName = new FsmString(string.Empty);
        BusVolume = new FsmFloat();
        BusIsMuted = new FsmBool();
        BusIsSoloed = new FsmBool();
        VoiceLimit = new FsmInt();
        ActiveVoices = new FsmInt();
        VoiceLimitReached = new FsmBool();
    }
}

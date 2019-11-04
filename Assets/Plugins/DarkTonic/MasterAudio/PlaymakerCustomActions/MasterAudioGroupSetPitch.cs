using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Set a single Sound Group's pitch in Master Audio")]
public class MasterAudioGroupSetPitch : FsmStateAction {

    [RequiredField]
    [Tooltip("Name of Master Audio Sound Group")]
    public FsmString soundGroupName;

    [Tooltip("Check this to perform action on all variations in Sound Group")]
    public FsmBool allVariations;

    [Tooltip("Name of variation in Sound Group. Leave blank if using all.")]
    public FsmString variationName;

    [RequiredField]
    [HasFloatSlider(0, 3)]
    [Tooltip("New pitch value to use.")]
    public FsmFloat pitch = new FsmFloat(1f);

    [Tooltip("Repeat every frame while the state is active.")]
    public bool everyFrame;

    public override void Reset() {

        soundGroupName = new FsmString(string.Empty);
        pitch = 1f;
        allVariations = true;
        variationName = null;

    }

    public override void OnEnter() {
        SetPitch();

        if (!everyFrame) {
            Finish();
        }
    }

    public override void OnUpdate() {
        SetPitch();
    }

    private void SetPitch() {
        if (string.IsNullOrEmpty(soundGroupName.Value)) {
            Debug.LogError("You must enter the Sound Group Name");
            return;
        }

        if (!allVariations.Value && string.IsNullOrEmpty(variationName.Value)) {
            Debug.LogError("You must either check all variations true or enter the Variation Name");
            return;
        }

        MasterAudio.ChangeVariationPitch(soundGroupName.Value, allVariations.Value, variationName.Value, pitch.Value);
    }
}

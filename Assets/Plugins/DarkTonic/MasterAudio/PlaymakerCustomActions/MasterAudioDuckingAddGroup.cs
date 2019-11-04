using DarkTonic.MasterAudio;
using HutongGames.PlayMaker;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Add a Sound Group in Master Audio to the list of sounds that cause music ducking.")]
// ReSharper disable once CheckNamespace
public class MasterAudioDuckingAddGroup : FsmStateAction {
    [RequiredField]
    [Tooltip("Name of Master Audio Sound Group")]
	public FsmString soundGroupName;

    [RequiredField]
    [HasFloatSlider(0, 1)]
    [Tooltip("Percentage of time length to start unducking.")]
    public FsmFloat riseVolumeStart;
    
    [RequiredField]
	[HasFloatSlider(0, 1)]
    [Tooltip("Percentage of original volume.")]
	public FsmFloat beginUnduck;

    [RequiredField]
    [HasFloatSlider(0, 1)]
    [Tooltip("Amount of time to return music to original volume.")]
    public FsmFloat unduckTime;

	public override void OnEnter() {
        MasterAudio.AddSoundGroupToDuckList(soundGroupName.Value, beginUnduck.Value, riseVolumeStart.Value, unduckTime.Value);
		
		Finish();
	}
	
	public override void Reset() {
		soundGroupName = new FsmString(string.Empty);
		var defaultRise = .5f;
			
		var ma = MasterAudio.Instance;
		if (ma != null) {
			defaultRise = ma.defaultRiseVolStart;
		}
			
		beginUnduck = new FsmFloat(defaultRise);
        riseVolumeStart = new FsmFloat(defaultRise);
        unduckTime = new FsmFloat(1);
	}
}

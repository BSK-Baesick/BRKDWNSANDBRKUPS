using DarkTonic.MasterAudio;
using UnityEngine;

public static class ResonanceAudioHelper {
	public static bool ResonanceAudioOptionExists {
		get {
#if UNITY_2018_1_OR_NEWER
            return true;
#else 
			return false;
#endif
		}
	}

    public static bool AddResonanceAudioSourceToAllVariations() {
		return false;
	}

    public static bool RemoveResonanceAudioSourceFromAllVariations() {
		return false;
	}

    public static void CopyResonanceAudioSource(DynamicGroupVariation sourceVariation, DynamicGroupVariation destVariation) {
		return;
	}

    public static void CopyResonanceAudioSource(DynamicGroupVariation sourceVariation, SoundGroupVariation destVariation) {
		return;
	}

    public static void CopyResonanceAudioSource(SoundGroupVariation sourceVariation, DynamicGroupVariation destVariation) {
		return;
	}
}

/*! \cond PRIVATE */
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    public static class SpatializerHelper {
        private const string OculusSpatializer = "OculusSpatializer";
        private const string ResonanceAudioSpatializer = "Resonance Audio";

        public static bool IsSupportedSpatializer {
            get {
                switch (SelectedSpatializer) {
                    case OculusSpatializer:
                        return true;
                    case ResonanceAudioSpatializer:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public static bool IsResonanceAudioSpatializer {
            get {
                return SelectedSpatializer == ResonanceAudioSpatializer;
            }
        }

        public static string SelectedSpatializer {
            get {
#if UNITY_2017_1_OR_NEWER
                return AudioSettings.GetSpatializerPluginName();
#else
                return string.Empty;
#endif
            }
        }

        public static bool SpatializerOptionExists {
            get {
#if UNITY_2017_1_OR_NEWER
                return true;
#else
				return false;
#endif
            }
        }

        public static void TurnOnSpatializerIfEnabled(AudioSource source) {
            if (!SpatializerOptionExists) {
                return; // no spatializer option!
            }

            // hopefully, there's a way later to detect if the option is turned on, in AudioManager!

            if (MasterAudio.SafeInstance == null) {
                return;
            }

            if (!MasterAudio.Instance.useSpatializer) {
                return;
            }

#if UNITY_2017_1_OR_NEWER
            source.spatialize = true;
#else
			// no spatializer!
#endif

            if (!ResonanceAudioHelper.ResonanceAudioOptionExists) {
                return;
            }

            if (!MasterAudio.Instance.useSpatializerPostFX) {
                return;
            }

#if UNITY_2018_1_OR_NEWER
            source.spatializePostEffects = true;
#else
			// no spatializer post FX!
#endif
        }
    }
}
/*! \endcond */

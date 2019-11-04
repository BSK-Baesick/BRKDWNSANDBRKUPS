/*! \cond PRIVATE */
using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    [Serializable]
    // ReSharper disable once CheckNamespace
    public class MusicSetting {
        // ReSharper disable InconsistentNaming
        public string alias = string.Empty;
        public MasterAudio.AudioLocation audLocation = MasterAudio.AudioLocation.Clip;
        public AudioClip clip;
        public string songName = string.Empty;
        public string resourceFileName = string.Empty;
        public float volume = 1f;
        public float pitch = 1f;
        public bool isExpanded = true;
        public bool isLoop;
        public bool isChecked = true;
        public List<SongMetadataStringValue> metadataStringValues = new List<SongMetadataStringValue>();
        public List<SongMetadataBoolValue> metadataBoolValues = new List<SongMetadataBoolValue>();
        public List<SongMetadataIntValue> metadataIntValues = new List<SongMetadataIntValue>();
        public List<SongMetadataFloatValue> metadataFloatValues = new List<SongMetadataFloatValue>();

        public bool metadataExpanded = true; 

        public MasterAudio.CustomSongStartTimeMode songStartTimeMode = MasterAudio.CustomSongStartTimeMode.Beginning;
        public float customStartTime;
        public float customStartTimeMax;
        public int lastKnownTimePoint = 0;
		public bool wasLastKnownTimePointSet = false;
		public int songIndex = 0; 
        public bool songStartedEventExpanded;
        public string songStartedCustomEvent = string.Empty;
        public bool songChangedEventExpanded;
        public string songChangedCustomEvent = string.Empty;

        public MusicSetting() {
            songChangedEventExpanded = false;
        }

        public bool HasMetadataProperties {
            get {
                return MetadataPropertyCount > 0;
            }
        }

        public int MetadataPropertyCount {
            get {
                return metadataStringValues.Count + metadataBoolValues.Count + metadataIntValues.Count + metadataFloatValues.Count;
            }
        }

        public float SongStartTime {
            get {
                switch (songStartTimeMode) {
                    default:
                    case MasterAudio.CustomSongStartTimeMode.Beginning:
                        return 0f;
                    case MasterAudio.CustomSongStartTimeMode.SpecificTime:
                        return customStartTime;
                    case MasterAudio.CustomSongStartTimeMode.RandomTime:
                        return UnityEngine.Random.Range(customStartTime, customStartTimeMax);
                }
            }
        }

        public static MusicSetting Clone(MusicSetting mus, MasterAudio.Playlist aList) {
            var clone = new MusicSetting {
                alias = mus.alias,
                audLocation = mus.audLocation,
                clip = mus.clip,
                songName = mus.songName,
                resourceFileName = mus.resourceFileName,
                volume = mus.volume,
                pitch = mus.pitch,
                isExpanded = mus.isExpanded,
                isLoop = mus.isLoop,
                isChecked = mus.isChecked,
                customStartTime = mus.customStartTime,
                songStartedEventExpanded = mus.songStartedEventExpanded,
                songStartedCustomEvent = mus.songStartedCustomEvent,
                songChangedEventExpanded = mus.songChangedEventExpanded,
                songChangedCustomEvent = mus.songChangedCustomEvent,
                metadataExpanded = mus.metadataExpanded
            };

            SongMetadataProperty prop = null;

            for (var i = 0; i < mus.metadataStringValues.Count; i++) {
                var valToClone = mus.metadataStringValues[i];
                prop = aList.songMetadataProps.Find(delegate (SongMetadataProperty p) {
                    return p.PropertyName == valToClone.PropertyName;
                });
                var sVal = new SongMetadataStringValue(prop);
                sVal.Value = valToClone.Value;
                clone.metadataStringValues.Add(sVal);
            }

            for (var i = 0; i < mus.metadataFloatValues.Count; i++) {
                var valToClone = mus.metadataFloatValues[i];
                prop = aList.songMetadataProps.Find(delegate (SongMetadataProperty p) {
                    return p.PropertyName == valToClone.PropertyName;
                });
                var fVal = new SongMetadataFloatValue(prop);
                fVal.Value = valToClone.Value;
                clone.metadataFloatValues.Add(fVal);
            }

            for (var i = 0; i < mus.metadataBoolValues.Count; i++) {
                var valToClone = mus.metadataBoolValues[i];
                prop = aList.songMetadataProps.Find(delegate (SongMetadataProperty p) {
                    return p.PropertyName == valToClone.PropertyName;
                });
                var bVal = new SongMetadataBoolValue(prop);
                bVal.Value = valToClone.Value;
                clone.metadataBoolValues.Add(bVal);
            }

            for (var i = 0; i < mus.metadataIntValues.Count; i++) {
                var valToClone = mus.metadataIntValues[i];
                prop = aList.songMetadataProps.Find(delegate (SongMetadataProperty p) {
                    return p.PropertyName == valToClone.PropertyName;
                });
                var iVal = new SongMetadataIntValue(prop);
                iVal.Value = valToClone.Value;
                clone.metadataIntValues.Add(iVal);
            }

            return clone;
            // ReSharper restore InconsistentNaming
        }
    }
}
/*! \endcond */

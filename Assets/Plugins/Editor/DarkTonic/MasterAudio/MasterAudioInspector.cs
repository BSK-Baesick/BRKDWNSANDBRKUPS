using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using DarkTonic.MasterAudio;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
#if UNITY_5 || UNITY_2017_1_OR_NEWER
using UnityEngine.Audio;
#endif

[CustomEditor(typeof(MasterAudio))]
// ReSharper disable once CheckNamespace
public class MasterAudioInspector : Editor {
    public const string NewBusName = "[NEW BUS]";
    public const string RenameMeBusName = "[BUS NAME]";

    public const string SpatialBlendSliderText = "For the slider below, 0 is fully 2D and 1 is fully 3D.";

    private const int NarrowWidth = 40;
    private const string NoMuteSoloAllowed = "You cannot mute or solo this Group because the bus it uses is soloed or muted. Please unmute or unsolo the bus instead.";

    public static readonly Color InactiveClr = new Color(.00f, .77f, .33f);
    public static readonly Color ActiveClr = new Color(.33f, .99f, .66f);

    private static MasterAudio _sounds;

    private bool _isValid = true;

    // ReSharper disable once InconsistentNaming
    public List<MasterAudioGroup> groups;

    private List<string> _groupTemplateNames = new List<string>();
    private List<string> _audioSourceTemplateNames = new List<string>();
    private List<string> _playlistNames = new List<string>();
    private bool _isDirty;
    private string invalidReason = string.Empty;

    // ReSharper disable once InconsistentNaming
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static List<MAObjectContext> allChangePersisters = new List<MAObjectContext>();
    private static bool _persistChanges;

    private readonly List<float> _reevaluatePriorityTimes = new List<float>() {
        .1f,
        .2f,
        .3f,
        .4f,
        .5f,
        .6f,
        .7f,
        .8f,
        .9f,
        1.0f
    };

    #region Change Persisting Code
    public void OnEnable() {
#if UNITY_2017_2_OR_NEWER
        EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
#else
        // subtract delegate so we don't get more than one.
        // ReSharper disable once DelegateSubtraction
        EditorApplication.playmodeStateChanged -= PlaymodeStateChanged;
        EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
#endif
    }

    private static void AddAllChangePersistingObjects() {
        var ma = MasterAudio.Instance;

        _persistChanges = ma.saveRuntimeChanges;

        AddPersistingChangeObject(ma.transform);

        for (var i = 0; i < ma.transform.childCount; i++) {
            var aGroup = ma.transform.GetChild(i);

            AddPersistingChangeObject(aGroup);

            for (var v = 0; v < aGroup.transform.childCount; v++) {
                var aVar = aGroup.transform.GetChild(v);

                AddPersistingChangeObject(aVar.transform);
            }
        }

        var controllers = PlaylistController.Instances;
        foreach (var aController in controllers) {
            AddPersistingChangeObject(aController.transform);
        }
    }

    private static void AddPersistingChangeObject(Transform trans) {
        var context = new MAObjectContext();
        allChangePersisters.Add(context);
        context.SetContext(trans);
    }

#if UNITY_2017_2_OR_NEWER
    /// <summary>
    /// This is the new one for Unity 2017. Old one deprecated
    /// </summary>
    /// <param name="state"></param>
    private static void PlayModeStateChanged(PlayModeStateChange state) {
        PlaymodeStateChanged();
    }
#endif

    private static void PlaymodeStateChanged() {
        if (EditorApplication.isPlaying || EditorApplication.isPaused) {
            if (allChangePersisters.Count == 0) {
                AddAllChangePersistingObjects();
            }

            foreach (var t in allChangePersisters) {
                t.GameObjectSetting.StoreAllSelectedSettings();
            }

            return;
        }

        // pressed stop
        if (!_persistChanges || allChangePersisters.Count == 0) {
            allChangePersisters.Clear(); // make sure we don't accidentally restore while entering Play mode.
            return;
        }

        foreach (var t in allChangePersisters) {
            var listOfComponentsChanged = t.GameObjectSetting.RestoreAllSelectedSettings();

            if (listOfComponentsChanged.Count <= 0) {
                continue;
            }

            if (DTGUIHelper.IsNonPrefab(t.GameObj)) {
                continue;
            }

            foreach (var changedComp in listOfComponentsChanged) {
                EditorUtility.SetDirty(changedComp);
            }
        }

        allChangePersisters.Clear();
    }
    #endregion

    public override void OnInspectorGUI() {
        EditorGUI.indentLevel = 0;

        _sounds = (MasterAudio)target;

        if (MasterAudioInspectorResources.LogoTexture != null) {
            DTGUIHelper.ShowHeaderTexture(MasterAudioInspectorResources.LogoTexture);
        }

        if (!_isValid) {
            if (!string.IsNullOrEmpty(invalidReason)) {
                DTGUIHelper.ShowRedError(invalidReason);
            }
            return;
        }

        if (_sounds.transform.parent != null) {
            DTGUIHelper.ShowRedError("You have the Master Audio game object as a child of another game object.");
            DTGUIHelper.ShowRedError("This is not a supported scenario and some things might break. Please unparent it.");
        }

        ScanGroups();

        DTGUIHelper.HelpHeader("http://www.dtdevtools.com/docs/masteraudio/MasterAudioGO.htm");

        _isDirty = false;
        AudioSource previewer;

        var sliderIndicatorChars = 6;
        var sliderWidth = 40;

        if (MasterAudio.UseDbScaleForVolume) {
            sliderIndicatorChars = 9;
            sliderWidth = 56;
        }

        var fakeDirty = false;
        var allowPreview = !DTGUIHelper.IsPrefabInProjectView(_sounds);

        if (!allowPreview) {
            DTGUIHelper.ShowLargeBarAlert("You are in Project View or have not made your own prefab and cannot use this Game Object.");
            DTGUIHelper.ShowRedError("Create this prefab from Master Audio Manager window. Do not drag into Scene! Then make your own prefab.");
            return;
        }

        if (!DTGUIHelper.IsPrefabInProjectView(_sounds) && !Application.isPlaying && _sounds.gameObject.layer != 2) { // "ignore raycast layer"
            _sounds.gameObject.layer = 2;
            _isDirty = true;
        }

        _playlistNames = new List<string>();

        var maxPlaylistNameChars = 11;
        foreach (var t in _sounds.musicPlaylists) {
            var pList = t;

            _playlistNames.Add(pList.playlistName);
            if (pList.playlistName.Length > maxPlaylistNameChars) {
                maxPlaylistNameChars = pList.playlistName.Length;
            }
        }

        var groupNameList = GroupNameList;

        var busFilterList = new List<string> { MasterAudio.AllBusesName, MasterAudioGroup.NoBus };

        var maxChars = 9;
        var busList = new List<string> { MasterAudioGroup.NoBus, NewBusName };

        var busVoiceLimitList = new List<string> { MasterAudio.NoVoiceLimitName };

        for (var i = 1; i <= 32; i++) {
            busVoiceLimitList.Add(i.ToString());
        }

        foreach (var t in _sounds.groupBuses) {
            var bus = t;
            busList.Add(bus.busName);
            busFilterList.Add(bus.busName);

            if (bus.busName.Length > maxChars) {
                maxChars = bus.busName.Length;
            }
        }
        var busListWidth = 9 * maxChars;
        var playlistListWidth = 9 * maxPlaylistNameChars;
        var extraPlaylistLength = 0;
        if (maxPlaylistNameChars > 11) {
            extraPlaylistLength = 9 * (11 - maxPlaylistNameChars);
        }

        PlaylistController.Instances = null;
        var pcs = PlaylistController.Instances;
        var plControllerInScene = pcs.Count > 0;

        var labelWidth = 163;
        if (!MasterAudio.UseDbScaleForVolume) {
            labelWidth = 138;
        }

        // mixer master volume!
        EditorGUILayout.BeginHorizontal();
        var volumeBefore = _sounds._masterAudioVolume;
        GUILayout.Label(DTGUIHelper.LabelVolumeField("Master Mixer Volume"), GUILayout.Width(labelWidth));

        GUILayout.Space(5);

        var topSliderWidth = _sounds.MixerWidth;
        if (topSliderWidth == MasterAudio.MixerWidthMode.Wide) {
            topSliderWidth = MasterAudio.MixerWidthMode.Normal;
        }

        var newMasterVol = DTGUIHelper.DisplayVolumeField(_sounds._masterAudioVolume, DTGUIHelper.VolumeFieldType.GlobalVolume, topSliderWidth);
        if (newMasterVol != _sounds._masterAudioVolume) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Master Mixer Volume");
            if (Application.isPlaying) {
                MasterAudio.MasterVolumeLevel = newMasterVol;
            } else {
                _sounds._masterAudioVolume = newMasterVol;
            }
        }

        var mixerButtonPressed = DTGUIHelper.AddMasterMixerButtons("Mixer", _sounds);

        switch (mixerButtonPressed) {
            case DTGUIHelper.DTFunctionButtons.Pause:
                MasterAudio.PauseMixer();
                break;
            case DTGUIHelper.DTFunctionButtons.Play:
                MasterAudio.UnpauseMixer();
                break;
            case DTGUIHelper.DTFunctionButtons.Stop:
                MasterAudio.StopMixer();
                break;
            case DTGUIHelper.DTFunctionButtons.Mute:
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Mixer Mute");

                _sounds.mixerMuted = !_sounds.mixerMuted;

                // unsolo all buses
                foreach (var aBus in _sounds.groupBuses) {
                    if (aBus.isSoloed) {
                        MasterAudio.UnsoloBus(aBus.busName, false);
                    } else if (aBus.isMuted) {
                        MasterAudio.UnmuteBus(aBus.busName, false);
                    }
                }

                if (Application.isPlaying) {
                    MasterAudio.MixerMuted = _sounds.mixerMuted;
                    MasterAudio.SilenceOrUnsilenceGroupsFromSoloChange();
                } else {
                    foreach (var aGroup in groups) {
                        aGroup.isMuted = _sounds.mixerMuted;

                        aGroup.isSoloed = false;
                    }
                }

                break;
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (volumeBefore != _sounds._masterAudioVolume) {
            // fix it for realtime adjustments!
            MasterAudio.MasterVolumeLevel = _sounds._masterAudioVolume;
        }

        // playlist master volume!
        if (plControllerInScene) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(DTGUIHelper.LabelVolumeField("Master Playlist Volume"), GUILayout.Width(labelWidth));
            GUILayout.Space(5);
            var newPlaylistVol = DTGUIHelper.DisplayVolumeField(_sounds._masterPlaylistVolume, DTGUIHelper.VolumeFieldType.GlobalVolume, topSliderWidth);
            if (newPlaylistVol != _sounds._masterPlaylistVolume) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Master Playlist Volume");
                if (Application.isPlaying) {
                    MasterAudio.PlaylistMasterVolume = newPlaylistVol;
                } else {
                    _sounds._masterPlaylistVolume = newPlaylistVol;
                }
            }

            var playlistButtonPressed = DTGUIHelper.AddMasterPlaylistButtons("All Playlists", _sounds);

            switch (playlistButtonPressed) {
                case DTGUIHelper.DTFunctionButtons.Pause:
                    MasterAudio.PauseAllPlaylists();
                    break;
                case DTGUIHelper.DTFunctionButtons.Play:
                    MasterAudio.UnpauseAllPlaylists();
                    break;
                case DTGUIHelper.DTFunctionButtons.Stop:
                    MasterAudio.StopAllPlaylists();
                    break;
                case DTGUIHelper.DTFunctionButtons.Mute:
                    if (Application.isPlaying) {
                        MasterAudio.PlaylistsMuted = !MasterAudio.PlaylistsMuted;
                    } else {
                        _sounds.playlistsMuted = !_sounds.playlistsMuted;

                        foreach (var t in pcs) {
                            if (_sounds.playlistsMuted) {
                                t.MutePlaylist();
                            } else {
                                t.UnmutePlaylist();
                            }

                            EditorUtility.SetDirty(t);
                        }
                    }
                    break;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Master Crossfade Time", GUILayout.Width(labelWidth));

            GUILayout.Space(5);
            var width = topSliderWidth == MasterAudio.MixerWidthMode.Narrow ? 61 : 198;
            var newCrossTime = GUILayout.HorizontalSlider(_sounds.crossFadeTime, 0f, MasterAudio.MaxCrossFadeTimeSeconds, GUILayout.Width(width));
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (newCrossTime != _sounds.crossFadeTime) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Master Crossfade Time");
                _sounds.crossFadeTime = newCrossTime;
            }

            var newCross = EditorGUILayout.FloatField(_sounds.crossFadeTime, GUILayout.Width(50));
            if (newCross != _sounds.crossFadeTime) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Master Crossfade Time");
                if (newCross < 0) {
                    newCross = 0;
                } else if (newCross > 10) {
                    newCross = 10;
                }
                _sounds.crossFadeTime = newCross;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // jukebox controls
            if (Application.isPlaying) {
                DisplayJukebox(PlaylistController.Instances, _playlistNames);
            }
        } else {
            DTGUIHelper.VerticalSpace(2);
        }

        // Localization section Start
        EditorGUI.indentLevel = 0;

        var state = _sounds.showLocalization;
        var text = "Languages";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTGUIHelper.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTGUIHelper.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

        text = "<b><size=11>" + text + "</size></b>";

        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        if (state != _sounds.showLocalization) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Languages");
            _sounds.showLocalization = state;
        }

        DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/Localization.htm");

        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;

        if (_sounds.showLocalization) {
            DTGUIHelper.BeginGroupedControls();
            if (Application.isPlaying) {
                DTGUIHelper.ShowColorWarning("Language settings cannot be changed during runtime");
            } else {
                int? langToRemove = null;
                int? langToAdd = null;

                for (var i = 0; i < _sounds.supportedLanguages.Count; i++) {
                    var aLang = _sounds.supportedLanguages[i];

                    DTGUIHelper.StartGroupHeader();
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("Supported Lang. " + (i + 1), GUILayout.Width(120));
                    var newLang = (SystemLanguage)EditorGUILayout.EnumPopup("", aLang, GUILayout.Width(130));
                    if (newLang != aLang) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Supported Language");
                        _sounds.supportedLanguages[i] = newLang;
                    }
                    GUILayout.FlexibleSpace();

                    var buttonPressed = DTGUIHelper.AddFoldOutListItemButtonItems(i, _sounds.supportedLanguages.Count, "Supported Language", true);

                    switch (buttonPressed) {
                        case DTGUIHelper.DTFunctionButtons.Remove:
                            langToRemove = i;
                            break;
                        case DTGUIHelper.DTFunctionButtons.Add:
                            langToAdd = i;
                            break;
                    }

                    EditorGUILayout.EndHorizontal();
                    DTGUIHelper.EndGroupHeader();
                    DTGUIHelper.AddSpaceForNonU5(2);
                }

                if (langToAdd.HasValue) {
                    _sounds.supportedLanguages.Insert(langToAdd.Value + 1, SystemLanguage.Unknown);
                } else if (langToRemove.HasValue) {
                    if (_sounds.supportedLanguages.Count <= 1) {
                        DTGUIHelper.ShowAlert("You cannot delete the last Supported Language, although you do not have to use Localization.");
                    } else {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Delete Supported Language");
                        _sounds.supportedLanguages.RemoveAt(langToRemove.Value);
                    }
                }

                if (!_sounds.supportedLanguages.Contains(_sounds.defaultLanguage)) {
                    DTGUIHelper.ShowLargeBarAlert("Please add your default language under Supported Languages as well.");
                }

                var newLang2 = (SystemLanguage)EditorGUILayout.EnumPopup(new GUIContent("Default Language", "This language will be used if the user's current language is not supported."), _sounds.defaultLanguage);
                if (newLang2 != _sounds.defaultLanguage) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Default Language");
                    _sounds.defaultLanguage = newLang2;
                }

                var newMode = (MasterAudio.LanguageMode)EditorGUILayout.EnumPopup("Language Mode", _sounds.langMode);
                if (newMode != _sounds.langMode) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Language Mode");
                    _sounds.langMode = newMode;
                    AudioResourceOptimizer.ClearSupportLanguageFolder();
                }

                if (_sounds.langMode == MasterAudio.LanguageMode.SpecificLanguage) {
                    var newLang = (SystemLanguage)EditorGUILayout.EnumPopup(new GUIContent("Use Specific Language", "This language will be used instead of your computer's setting. This is useful for testing other languages."), _sounds.testLanguage);
                    if (newLang != _sounds.testLanguage) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Use Specific");
                        _sounds.testLanguage = newLang;
                        AudioResourceOptimizer.ClearSupportLanguageFolder();
                    }

                    if (_sounds.supportedLanguages.Contains(_sounds.testLanguage)) {
                        DTGUIHelper.ShowLargeBarAlert("Please select your Specific Language under Supported Languages as well.");
                        DTGUIHelper.ShowLargeBarAlert("If you do not, it will use your Default Language instead.");
                    }
                } else if (_sounds.langMode == MasterAudio.LanguageMode.DynamicallySet) {
                    DTGUIHelper.ShowLargeBarAlert("Dynamic Language currently set to: " + MasterAudio.DynamicLanguage.ToString());
                }
            }
            DTGUIHelper.EndGroupedControls();
        }

        // Advanced section Start
        DTGUIHelper.VerticalSpace(3);

        EditorGUI.indentLevel = 0;

        state = _sounds.showAdvancedSettings;
        text = "Advanced Settings";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTGUIHelper.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTGUIHelper.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

        text = "<b><size=11>" + text + "</size></b>";

        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        if (state != _sounds.showAdvancedSettings) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Advanced Settings");
            _sounds.showAdvancedSettings = state;
        }

        DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/AdvancedSettings.htm");

        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;

        if (_sounds.showAdvancedSettings) {
            DTGUIHelper.BeginGroupedControls();
            if (!Application.isPlaying) {
                var newPersist = EditorGUILayout.Toggle(new GUIContent("Persist Across Scenes", "Turn this on only if you need music or other sounds to play across Scene changes. If not, create a different Master Audio prefab in each Scene."), _sounds.persistBetweenScenes);
                if (newPersist != _sounds.persistBetweenScenes) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Persist Across Scenes");
                    _sounds.persistBetweenScenes = newPersist;
                }

                if (_sounds.persistBetweenScenes) {
                    EditorGUI.indentLevel = 1;
                    var newLogDestroys = EditorGUILayout.Toggle(new GUIContent("Log Destroys Of Dupes", "Turning this on will log to the Console whenever a new Scene is loaded and another Master Audio game object is destroyed because there's a persistent one already (and also the same for Playlist Controllers). This is purely for troubleshooting purposes."), _sounds.shouldLogDestroys);
                    if (newLogDestroys != _sounds.shouldLogDestroys) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Log Destroys Of Dupes");
                        _sounds.shouldLogDestroys = newLogDestroys;
                    }

                    if (plControllerInScene) {
                        DTGUIHelper.ShowColorWarning("Playlist Controller(s) will also persist between scenes!");
                    }
                }
            }

            EditorGUI.indentLevel = 0;

            var newGap = EditorGUILayout.Toggle(new GUIContent("Gapless Music Switching", "Turn this option on if you need perfect gapless transitions between clips in your Playlists. For Resource Files, this will take more audio memory as 2 clips are generally loaded into memory at the same time, so only use it if you need to."), _sounds.useGaplessPlaylists);
            if (newGap != _sounds.useGaplessPlaylists) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Gapless Music Switching");
                _sounds.useGaplessPlaylists = newGap;
            }

            if (_sounds.useGaplessPlaylists && _sounds.crossFadeTime > 0) {
                DTGUIHelper.ShowRedError("Gapless Music Switching will not work properly with crossfading. Please turn Master Crossfade Time to 0.");
            }

            var newSave = EditorGUILayout.Toggle(new GUIContent("Save Runtime Changes", "Turn this on if you want to do real time adjustments to the mix, Master Audio prefab, Groups and Playlist Controllers and have the changes stick after you stop playing."), _sounds.saveRuntimeChanges);
            if (newSave != _sounds.saveRuntimeChanges) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Save Runtime Changes");
                _sounds.saveRuntimeChanges = newSave;
                _persistChanges = newSave;
            }

            var newIgnore = EditorGUILayout.Toggle(new GUIContent("Ignore Time Scale", "Turn this option on only if you need to use DelayBetweenSongs"), _sounds.ignoreTimeScale);
            if (newIgnore != _sounds.ignoreTimeScale) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Ignore Time Scale");
                _sounds.ignoreTimeScale = newIgnore;
            }

            DTGUIHelper.StartGroupHeader();
            EditorGUILayout.BeginHorizontal();
            var newAutoPrioritize = GUILayout.Toggle(_sounds.prioritizeOnDistance, new GUIContent(" Use Distance Priority", "Turn this on to have Master Audio automatically assign Priority to all audio, based on distance from the Audio Listener. Playlist Controller and 2D sounds are unaffected."));
            if (newAutoPrioritize != _sounds.prioritizeOnDistance) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Use Distance Priority");
                _sounds.prioritizeOnDistance = newAutoPrioritize;
            }

            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/AdvancedSettings.htm#DistancePriority");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (_sounds.prioritizeOnDistance) {
                EditorGUI.indentLevel = 0;

                var reevalIndex = _sounds.rePrioritizeEverySecIndex;

                var evalTimes = new List<string>();
                foreach (var t in _reevaluatePriorityTimes) {
                    if (t == 1) {
                        evalTimes.Add(t + ".0 seconds");
                        continue;
                    }
                    evalTimes.Add(t + " seconds");
                }

                var newRepri = EditorGUILayout.Popup("Reprioritize Time Gap", reevalIndex, evalTimes.ToArray());
                if (newRepri != reevalIndex) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Re-evaluate time");
                    _sounds.rePrioritizeEverySecIndex = newRepri;
                }

                var newContinual = EditorGUILayout.Toggle("Use Clip Age Priority", _sounds.useClipAgePriority);
                if (newContinual != _sounds.useClipAgePriority) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Use Clip Age Priority");
                    _sounds.useClipAgePriority = newContinual;
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel = 1;

            DTGUIHelper.AddSpaceForNonU5(2);

#if UNITY_5 || UNITY_2017_1_OR_NEWER
            DTGUIHelper.StartGroupHeader();
            EditorGUILayout.BeginHorizontal();
            var newOcclude = GUILayout.Toggle(_sounds.useOcclusion, new GUIContent(" Use Occlusion"));
            if (newOcclude != _sounds.useOcclusion) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Use Occlusion");
                _sounds.useOcclusion = newOcclude;
            }

            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/Occlusion.htm");

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (_sounds.useOcclusion) {
                EditorGUI.indentLevel = 0;

                var newGrpSelType = (MasterAudio.OcclusionSelectionType)EditorGUILayout.EnumPopup("Sound Group Usage", _sounds.occlusionSelectType);
                if (newGrpSelType != _sounds.occlusionSelectType) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Sound Group Usage");
                    _sounds.occlusionSelectType = newGrpSelType;
                }

                var newMaxCutoff = EditorGUILayout.Slider(new GUIContent("Max Occl. Cutoff Freq.", "This frequency will be used for cutoff for maximum occlusion (occluded nearest to sound emitter)"),
                    _sounds.occlusionMaxCutoffFreq, AudioUtil.DefaultMaxOcclusionCutoffFrequency, _sounds.occlusionMinCutoffFreq);
                if (newMaxCutoff != _sounds.occlusionMaxCutoffFreq) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Max Occl. Cutoff Freq.");
                    _sounds.occlusionMaxCutoffFreq = newMaxCutoff;
                }

                var newMinCutoff = EditorGUILayout.Slider(new GUIContent("Min Occl. Cutoff Freq.", "This frequency will be used for no occlusion (nothing blocking the sound emitter from the AudioListener)"),
                    _sounds.occlusionMinCutoffFreq, _sounds.occlusionMaxCutoffFreq, AudioUtil.DefaultMinOcclusionCutoffFrequency);
                if (newMinCutoff != _sounds.occlusionMinCutoffFreq) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Min Occl. Cutoff Freq.");
                    _sounds.occlusionMinCutoffFreq = newMinCutoff;
                }

                var newSpeedFrames = EditorGUILayout.Slider(new GUIContent("Freq. Change Time (sec)", "The number of seconds changing to the new occlusion cutoff frequency will take."), _sounds.occlusionFreqChangeSeconds, 0f, 5f);
                if (newSpeedFrames != _sounds.occlusionFreqChangeSeconds) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Freq. Change Time (sec)");
                    _sounds.occlusionFreqChangeSeconds = newSpeedFrames;
                }

                var newRayMode = (MasterAudio.RaycastMode)EditorGUILayout.EnumPopup("Ray Cast Mode", _sounds.occlusionRaycastMode);
                if (newRayMode != _sounds.occlusionRaycastMode) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Ray Cast Mode");
                    _sounds.occlusionRaycastMode = newRayMode;
                }

                var newMaxRayCasts = EditorGUILayout.IntSlider("Max Ray Casts Per Frame", _sounds.occlusionMaxRayCastsPerFrame, 1, 32);
                if (newMaxRayCasts != _sounds.occlusionMaxRayCastsPerFrame) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Max Ray Casts Per Frame");
                    _sounds.occlusionMaxRayCastsPerFrame = newMaxRayCasts;
                }

                var newOffset = EditorGUILayout.Slider(new GUIContent("Ray Cast Origin Offset", "Adjust how much closer to the Audio Listener the Ray Cast starts instead of exactly at the Audio Source position."), _sounds.occlusionRayCastOffset, 0f, 500f);
                if (newOffset != _sounds.occlusionRayCastOffset) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Ray Cast Origin Offset");
                    _sounds.occlusionRayCastOffset = newOffset;
                }

#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1
                // no option
#else
                if (_sounds.occlusionRaycastMode == MasterAudio.RaycastMode.Physics2D) {
                    var newStart = EditorGUILayout.Toggle("Include Ray Cast Origin Collider", _sounds.occlusionIncludeStartRaycast2DCollider);
                    if (newStart != _sounds.occlusionIncludeStartRaycast2DCollider) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Include Ray Cast Origin Collider");
                        _sounds.occlusionIncludeStartRaycast2DCollider = newStart;
                    }
                }

                var newTrigger = EditorGUILayout.Toggle("Ray Casts Hit Triggers", _sounds.occlusionRaycastsHitTriggers);
                if (newTrigger != _sounds.occlusionRaycastsHitTriggers) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Ray Casts Hit Triggers");
                    _sounds.occlusionRaycastsHitTriggers = newTrigger;
                }
#endif

                var newShowCat = EditorGUILayout.Toggle("Show Diagnostic Buckets", _sounds.occlusionShowCategories);
                if (newShowCat != _sounds.occlusionShowCategories) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change toggle Show Diagnostic Buckets");
                    _sounds.occlusionShowCategories = newShowCat;
                }

                var newShowRays = EditorGUILayout.Toggle("Show Ray Casts", _sounds.occlusionShowRaycasts);
                if (newShowRays != _sounds.occlusionShowRaycasts) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change toggle Show Ray Casts");
                    _sounds.occlusionShowRaycasts = newShowRays;
                }

                var newUseLayer = EditorGUILayout.Toggle("Use Layer Mask", _sounds.occlusionUseLayerMask);
                if (newUseLayer != _sounds.occlusionUseLayerMask) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change toggle Use Layer Mask");
                    _sounds.occlusionUseLayerMask = newUseLayer;
                }

                if (_sounds.occlusionUseLayerMask) {
                    var newMask = DTGUIHelper.LayerMaskField("Layer Mask", _sounds.occlusionLayerMask);
                    if (newMask != _sounds.occlusionLayerMask) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Layer Mask");
                        _sounds.occlusionLayerMask = newMask;
                    }
                }

                if (Application.isPlaying && _sounds.occlusionShowCategories) {
                    var oldContentColor = GUI.contentColor;
                    var oldBGColor = GUI.backgroundColor;

                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                    GUILayout.Label("Actively Playing Sources");

                    GUI.contentColor = Color.green;
                    GUI.backgroundColor = Color.green;

                    if (GUILayout.Button(new GUIContent("In Range:" + MasterAudio.InRangeOcclusionSources.Count.ToString(), "Click to select objects in Hierarchy"), EditorStyles.miniButton, GUILayout.Height(16))) {
                        Selection.objects = MasterAudio.InRangeOcclusionSources.ToArray();
                    }

                    GUI.contentColor = Color.yellow;
                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button(new GUIContent("Out of Range: " + MasterAudio.OutOfRangeOcclusionSources.Count.ToString(), "Click to select objects in Hierarchy"), EditorStyles.miniButton, GUILayout.Height(16))) {
                        Selection.objects = MasterAudio.OutOfRangeOcclusionSources.ToArray();
                    }

                    GUI.contentColor = Color.red;
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button(new GUIContent("Occluded: " + MasterAudio.BlockedOcclusionSources.Count.ToString(), "Click to select objects in Hierarchy"), EditorStyles.miniButton, GUILayout.Height(16))) {
                        Selection.objects = MasterAudio.BlockedOcclusionSources.ToArray();
                    }

                    EditorGUILayout.EndHorizontal();
                    GUI.contentColor = oldContentColor;
                    GUI.backgroundColor = oldBGColor;
                }

                DTGUIHelper.ShowColorWarning("Note: Only 3D Audio Clips will calculate for occlusion.");
            }
            EditorGUILayout.EndVertical();

            DTGUIHelper.AddSpaceForNonU5(2);
#endif

            if (SpatializerHelper.SpatializerOptionExists) {
                EditorGUI.indentLevel = 1;
                DTGUIHelper.StartGroupHeader();
                EditorGUILayout.BeginHorizontal();
                var labelName = "VR Settings";

                if (!string.IsNullOrEmpty(SpatializerHelper.SelectedSpatializer)) {
                    labelName += " (" + SpatializerHelper.SelectedSpatializer + ")";
                } else {
                    labelName += " (No Spatializer selected)";
                }

                var newVR = DTGUIHelper.Foldout(_sounds.vrSettingsExpanded, labelName);
                if (newVR != _sounds.vrSettingsExpanded) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle VR Settings");
                    _sounds.vrSettingsExpanded = newVR;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                if (_sounds.vrSettingsExpanded) {
                    EditorGUI.indentLevel = 0;
                    if (!SpatializerHelper.IsSupportedSpatializer) {
                        DTGUIHelper.ShowLargeBarAlert("You must select a supported Spatializer Plugin on the Audio Settings dialog before settings here will have any effect. Oculus and Resonance Audio are currently supported.");
                    }

                    var newSpatialize = EditorGUILayout.Toggle(new GUIContent("Use Spatializer", "Turn this on if you have selected OculusSpatializer for the Spatializer Plugin on the AudioManager settings screen. All Sound Group Audio Sources will automatically turn on 'Spatialize'."), _sounds.useSpatializer);
                    if (newSpatialize != _sounds.useSpatializer) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Use Spatializer");
                        _sounds.useSpatializer = newSpatialize;
                    }

                    if (SpatializerHelper.IsResonanceAudioSpatializer) {
                        if (_sounds.useSpatializer) {
                            var newwPost = EditorGUILayout.Toggle(new GUIContent("Spatialize Post FX", "Turn this on to Spatialize Post Effects (Resonance Audio option)."), _sounds.useSpatializerPostFX);
                            if (newwPost != _sounds.useSpatializerPostFX) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Spatialize Post FX");
                                _sounds.useSpatializerPostFX = newwPost;
                            }
                        }

                        EditorGUILayout.BeginHorizontal();

                        GUI.contentColor = DTGUIHelper.BrightButtonColor;
                        if (GUILayout.Button(new GUIContent("Add Resonance Audio Src to Variations"), EditorStyles.toolbarButton, GUILayout.Width(210))) {
                            if (!ResonanceAudioHelper.AddResonanceAudioSourceToAllVariations()) {
                                DTGUIHelper.ShowAlert("Install the optional package 'MA_ResonanceAudio' to get this button to work.");
                            } else {
                                _isDirty = true;
                            }
                        }
                        GUILayout.Space(6);
                        if (GUILayout.Button(new GUIContent("Delete Resonance Audio Src from Variations"), EditorStyles.toolbarButton, GUILayout.Width(234))) {
                            if (!ResonanceAudioHelper.RemoveResonanceAudioSourceFromAllVariations()) {
                                DTGUIHelper.ShowAlert("Install the optional package 'MA_ResonanceAudio' to get this button to work.");
                            } else {
                                _isDirty = true;
                            }
                        }

                        EditorGUILayout.EndHorizontal();

                        GUI.contentColor = Color.white;
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel = 1;
                DTGUIHelper.AddSpaceForNonU5(2);
            }

            EditorGUI.indentLevel = 1;
            DTGUIHelper.StartGroupHeader();

            EditorGUILayout.BeginHorizontal();
            var newAmb = DTGUIHelper.Foldout(_sounds.ambientAdvancedExpanded, "Ambient Sound Settings");
            if (newAmb != _sounds.ambientAdvancedExpanded) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Ambient Sound Settings");
                _sounds.ambientAdvancedExpanded = newAmb;
            }
            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/AdvancedSettings.htm#AmbientSoundSettings");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (_sounds.ambientAdvancedExpanded) {
                EditorGUI.indentLevel = 0;

                var newRecalcs = EditorGUILayout.IntSlider("Max Pos. Recalcs / Frame", _sounds.ambientMaxRecalcsPerFrame, 1, 32);
                if (newRecalcs != _sounds.ambientMaxRecalcsPerFrame) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Max Pos. Recalcs / Frame");
                    _sounds.ambientMaxRecalcsPerFrame = newRecalcs;
                }
            }
            EditorGUILayout.EndVertical();
            DTGUIHelper.AddSpaceForNonU5(2);

            EditorGUI.indentLevel = 1;
            DTGUIHelper.StartGroupHeader();

            EditorGUILayout.BeginHorizontal();
            var newVisual = DTGUIHelper.Foldout(_sounds.visualAdvancedExpanded, "Visual Settings");
            if (newVisual != _sounds.visualAdvancedExpanded) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Visual Settings");
                _sounds.visualAdvancedExpanded = newVisual;
            }
            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/AdvancedSettings.htm#VisualSettings");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (_sounds.visualAdvancedExpanded) {
                EditorGUI.indentLevel = 0;

                var newGiz2 = EditorGUILayout.Toggle(new GUIContent("Show All Range Gizmos", "Turning this option on will show you the max distance of your selected Sound Group in Ambient Sound and Event Sounds scripts so you can see how far the range is."), _sounds.showRangeSoundGizmos);
                if (newGiz2 != _sounds.showRangeSoundGizmos) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Show All Range Gizmos");
                    _sounds.showRangeSoundGizmos = newGiz2;
                }

                if (_sounds.showRangeSoundGizmos) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Range Gizmo Color", GUILayout.Width(150));
                    var newColor = EditorGUILayout.ColorField(_sounds.rangeGizmoColor);
                    if (newColor != _sounds.rangeGizmoColor) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Range Gizmo Color");
                        _sounds.rangeGizmoColor = newColor;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                var newGiz3 = EditorGUILayout.Toggle(new GUIContent("Show Sel. Range Gizmos", "For the selected Game Object in Hierarchy. Turning this option on will show you the max distance of your selected Sound Group in Ambient Sound and Event Sounds scripts so you can see how far the range is."), _sounds.showSelectedRangeSoundGizmos);
                if (newGiz3 != _sounds.showSelectedRangeSoundGizmos) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Show Sel. Range Gizmos");
                    _sounds.showSelectedRangeSoundGizmos = newGiz3;
                }

                if (_sounds.showSelectedRangeSoundGizmos) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Sel. Range Gizmo Color", GUILayout.Width(150));
                    var newColor2 = EditorGUILayout.ColorField(_sounds.selectedRangeGizmoColor);
                    if (newColor2 != _sounds.selectedRangeGizmoColor) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Sel. Range Gizmo Color");
                        _sounds.selectedRangeGizmoColor = newColor2;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                var newWidth = (MasterAudio.MixerWidthMode)EditorGUILayout.EnumPopup("Inspector Width", _sounds.MixerWidth);
                if (newWidth != _sounds.MixerWidth) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Inspector Width");
                    _sounds.MixerWidth = newWidth;
                }

                if (_sounds.MixerWidth == MasterAudio.MixerWidthMode.Narrow) {
                    var showBus = EditorGUILayout.Toggle("Show Buses in Narrow", _sounds.BusesShownInNarrow);
                    if (showBus != _sounds.BusesShownInNarrow) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Show Buses in Narrow");
                        _sounds.BusesShownInNarrow = showBus;
                    }
                }
            }
            EditorGUILayout.EndVertical();
            DTGUIHelper.AddSpaceForNonU5(2);

            EditorGUI.indentLevel = 1;
            DTGUIHelper.StartGroupHeader();

            EditorGUILayout.BeginHorizontal();
            var newListener = DTGUIHelper.Foldout(_sounds.listenerAdvancedExpanded, "Audio Listener Settings");
            if (newListener != _sounds.listenerAdvancedExpanded) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Audio Listener Settings");
                _sounds.listenerAdvancedExpanded = newListener;
            }
            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/AdvancedSettings.htm#AudioListenerSettings");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel = 0;
            if (_sounds.listenerAdvancedExpanded) {
                var newFollowType = (MasterAudio.VariationFollowerType)EditorGUILayout.EnumPopup(new GUIContent("Variation Update Method", "You may want to change this to Fixed Update if your Audio Listener is on a physics object, to eliminate audio glitches."), _sounds.variationFollowerType);
                if (newFollowType != _sounds.variationFollowerType) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Variation Update Method");
                    _sounds.variationFollowerType = newFollowType;
                }

                var newRB = EditorGUILayout.Toggle("Follower Has RigidBody", _sounds.listenerFollowerHasRigidBody);
                if (newRB != _sounds.listenerFollowerHasRigidBody) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Listener Follower RigidBody");
                    _sounds.listenerFollowerHasRigidBody = newRB;
                }
                DTGUIHelper.ShowColorWarning("This single non-gravity RigidBody is necessary for Ambient Sounds script to work.");
            }

            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel = 1;
            DTGUIHelper.AddSpaceForNonU5(2);
            DTGUIHelper.StartGroupHeader();
            EditorGUILayout.BeginHorizontal();
            var exp = DTGUIHelper.Foldout(_sounds.showFadingSettings, "Fading Settings");
            if (exp != _sounds.showFadingSettings) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Fading Settings");
                _sounds.showFadingSettings = exp;
            }
            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/AdvancedSettings.htm#FadingSettings");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (_sounds.showFadingSettings) {
                EditorGUI.indentLevel = 0;

                var newFade = EditorGUILayout.Slider("Bus Stop Oldest Fade Time", _sounds.stopOldestBusFadeTime, 0f, 1f);
                if (newFade != _sounds.stopOldestBusFadeTime) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bus Stop Oldest Fade Time");
                    _sounds.stopOldestBusFadeTime = newFade;
                }

                DTGUIHelper.ShowColorWarning("If checked, fading to zero volume on the following causes their audio to stop.");

                var newStop = EditorGUILayout.Toggle("Buses", _sounds.stopZeroVolumeBuses);
                if (newStop != _sounds.stopZeroVolumeBuses) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Buses Stop");
                    _sounds.stopZeroVolumeBuses = newStop;
                }

                newStop = EditorGUILayout.Toggle("Sound Groups", _sounds.stopZeroVolumeGroups);
                if (newStop != _sounds.stopZeroVolumeGroups) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Sound Groups Stop");
                    _sounds.stopZeroVolumeGroups = newStop;
                }

                newStop = EditorGUILayout.Toggle(new GUIContent("Playlist Controllers", "Automatic crossfading will not trigger stop."), _sounds.stopZeroVolumePlaylists);
                if (newStop != _sounds.stopZeroVolumePlaylists) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Playlist Controllers Stop");
                    _sounds.stopZeroVolumePlaylists = newStop;
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel = 1;
            DTGUIHelper.AddSpaceForNonU5(2);
            DTGUIHelper.StartGroupHeader();

            EditorGUILayout.BeginHorizontal();
            var newResource = DTGUIHelper.Foldout(_sounds.resourceAdvancedExpanded, "Resource File / Audio Clip Settings");
            if (newResource != _sounds.resourceAdvancedExpanded) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Resource File / Audio Clip Settings");
                _sounds.resourceAdvancedExpanded = newResource;
            }
            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/AdvancedSettings.htm#ResourceSettings");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            if (_sounds.resourceAdvancedExpanded) {
                EditorGUI.indentLevel = 0;
                if (MasterAudio.HasAsyncResourceLoaderFeature()) {
                    var newAsync = EditorGUILayout.Toggle(new GUIContent("Always Load Res. Async", "Checking this means that you will not have this control per Sound Group or Playlist. All Resource files will be loaded asynchronously."), _sounds.resourceClipsAllLoadAsync);
                    if (newAsync != _sounds.resourceClipsAllLoadAsync) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Always Load Resources Async");
                        _sounds.resourceClipsAllLoadAsync = newAsync;
                    }
                }

                var newResourcePause = EditorGUILayout.Toggle(new GUIContent("Keep Paused Resources", "If you check this box, Resource files and Audio Clips with Preload Audio Data = false will not be automatically unloaded from memory when you pause them. Enable at your own risk!"), _sounds.resourceClipsPauseDoNotUnload);
                if (newResourcePause != _sounds.resourceClipsPauseDoNotUnload) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Keep Paused Resources");
                    _sounds.resourceClipsPauseDoNotUnload = newResourcePause;
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel = 1;
            DTGUIHelper.AddSpaceForNonU5(2);
            DTGUIHelper.StartGroupHeader();
            EditorGUILayout.BeginHorizontal();
            var newLog = DTGUIHelper.Foldout(_sounds.logAdvancedExpanded, "Logging Settings");
            if (newLog != _sounds.logAdvancedExpanded) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Logging Settings");
                _sounds.logAdvancedExpanded = newLog;
            }
            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/AdvancedSettings.htm#LoggingSettings");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            if (_sounds.logAdvancedExpanded) {
                EditorGUI.indentLevel = 0;
                newLog = EditorGUILayout.Toggle("Disable Logging", _sounds.disableLogging);
                if (newLog != _sounds.disableLogging) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Disable Logging");
                    _sounds.disableLogging = newLog;
                }

                if (!_sounds.disableLogging) {
                    newLog = EditorGUILayout.Toggle("Log Sounds", _sounds.LogSounds);
                    if (newLog != _sounds.LogSounds) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Log Sounds");
                        if (Application.isPlaying) {
                            MasterAudio.LogSoundsEnabled = _sounds.LogSounds;
                        }
                        _sounds.LogSounds = newLog;
                    }

                    newLog = EditorGUILayout.Toggle("Log No Voices Left", _sounds.logOutOfVoices);
                    if (newLog != _sounds.logOutOfVoices) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Log No Voices Left");
                        if (Application.isPlaying) {
                            MasterAudio.LogOutOfVoices = _sounds.logOutOfVoices;
                        }
                        _sounds.logOutOfVoices = newLog;
                    }

                    newLog = EditorGUILayout.Toggle("Log Custom Events", _sounds.logCustomEvents);
                    if (newLog != _sounds.logCustomEvents) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Log Custom Events");
                        _sounds.logCustomEvents = newLog;
                    }
                } else {
                    DTGUIHelper.ShowColorWarning("Logging is disabled.");
                }
            }
            EditorGUILayout.EndVertical();

            DTGUIHelper.EndGroupedControls();
        }

        DTGUIHelper.ResetColors();
        // Music Ducking Start

        DTGUIHelper.VerticalSpace(3);

        EditorGUI.indentLevel = 0;

        state = _sounds.showMusicDucking;
        text = "Music Ducking";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTGUIHelper.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTGUIHelper.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

        text = "<b><size=11>" + text + "</size></b>";

        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        if (state != _sounds.showMusicDucking) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Show Music Ducking");
            _sounds.showMusicDucking = state;
        }

        DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/MusicDucking.htm");

        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;

        if (_sounds.showMusicDucking) {
            DTGUIHelper.BeginGroupedControls();
            var newEnableDuck = EditorGUILayout.BeginToggleGroup("Enable Ducking", _sounds.EnableMusicDucking);
            if (newEnableDuck != _sounds.EnableMusicDucking) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Enable Ducking");
                _sounds.EnableMusicDucking = newEnableDuck;
            }

            EditorGUILayout.Separator();

            var newMult = EditorGUILayout.Slider("Default Vol. Cut (dB)", _sounds.defaultDuckedVolumeCut, DTGUIHelper.MinDb, DTGUIHelper.MaxDb);
            if (newMult != _sounds.defaultDuckedVolumeCut) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Default Vol. Cut (dB)");
                _sounds.defaultDuckedVolumeCut = newMult;
            }

            var newDefault = EditorGUILayout.Slider("Default Unduck Time (sec)", _sounds.defaultUnduckTime, 0f, 5f);
            if (newDefault != _sounds.defaultUnduckTime) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Default Unduck Time (sec)");
                _sounds.defaultUnduckTime = newDefault;
            }

            newDefault = EditorGUILayout.Slider("Default Begin Unduck (%)", _sounds.defaultRiseVolStart, 0f, 1f);
            if (newDefault != _sounds.defaultRiseVolStart) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Default Begin Unduck (%)");
                _sounds.defaultRiseVolStart = newDefault;
            }

            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(6);

            if (GUILayout.Button(new GUIContent("Add Duck Group"), EditorStyles.toolbarButton, GUILayout.Width(100))) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Add Duck Group");
                _sounds.musicDuckingSounds.Add(new DuckGroupInfo() {
                    soundType = MasterAudio.NoGroupName,
                    riseVolStart = _sounds.defaultRiseVolStart,
                    duckedVolumeCut = _sounds.defaultDuckedVolumeCut,
                    unduckTime = _sounds.defaultUnduckTime
                });
            }

            EditorGUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
            EditorGUILayout.Separator();

            if (_sounds.musicDuckingSounds.Count == 0) {
                DTGUIHelper.ShowLargeBarAlert("You currently have no ducking sounds set up.");
            } else {
                int? duckSoundToRemove = null;

                var spacerWidth = _sounds.MixerWidth == MasterAudio.MixerWidthMode.Wide ? 60 : 0;

                if (_sounds.musicDuckingSounds.Count > 0) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Sound Group", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(new GUIContent("Vol. Cut (dB)", "Amount to duck the music volume."), EditorStyles.boldLabel);
                    GUILayout.Space(6 + spacerWidth);
                    GUILayout.Label(new GUIContent("Beg. Unduck", "Begin Unducking after this amount of the sound has been played."), EditorStyles.boldLabel);
                    GUILayout.Space(13 + spacerWidth);
                    GUILayout.Label(new GUIContent("Unduck Time", "Unducking will take X seconds."), EditorStyles.boldLabel);
                    GUILayout.Space(54 + spacerWidth);
                    EditorGUILayout.EndHorizontal();
                }

                var slidWidth = _sounds.MixerWidth == MasterAudio.MixerWidthMode.Wide ? 120 : 60;

                var duckingList = new List<string>(_sounds.musicDuckingSounds.Count);

                for (var i = 0; i < _sounds.musicDuckingSounds.Count; i++) {
                    var duckSound = _sounds.musicDuckingSounds[i];
                    var index = groupNameList.IndexOf(duckSound.soundType);
                    if (index == -1) {
                        index = 0;
                    }

                    var groupName = groupNameList[index];
                    if (groupName != MasterAudio.NoGroupName && duckingList.Contains(groupName)) {
                        DTGUIHelper.ShowRedError("You have more than one Duck Group for Sound Group '" + groupName + "'. Please delete all duplicates as only one of the dupes will be seen when ducking code runs.");
                    }

                    duckingList.Add(groupName);

                    DTGUIHelper.StartGroupHeader();

                    EditorGUILayout.BeginHorizontal();
                    var newIndex = EditorGUILayout.Popup(index, groupNameList.ToArray(), GUILayout.MaxWidth(200));
                    if (newIndex >= 0) {
                        if (index != newIndex) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Duck Group");
                        }
                        duckSound.soundType = groupNameList[newIndex];
                    }

                    GUILayout.FlexibleSpace();

                    GUI.contentColor = DTGUIHelper.BrightButtonColor;
                    GUILayout.TextField(duckSound.duckedVolumeCut.ToString("N1"), 20, EditorStyles.miniLabel);

                    var newDuckMult = GUILayout.HorizontalSlider(duckSound.duckedVolumeCut, DTGUIHelper.MinDb, DTGUIHelper.MaxDb, GUILayout.Width(slidWidth));
                    if (newDuckMult != duckSound.duckedVolumeCut) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Ducked Vol Cut");
                        duckSound.duckedVolumeCut = newDuckMult;
                    }
                    GUI.contentColor = Color.white;

                    GUI.contentColor = DTGUIHelper.BrightButtonColor;
                    GUILayout.TextField(duckSound.riseVolStart.ToString("N2"), 20, EditorStyles.miniLabel);

                    var newUnduck = GUILayout.HorizontalSlider(duckSound.riseVolStart, 0f, 1f, GUILayout.Width(slidWidth));
                    if (newUnduck != duckSound.riseVolStart) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Begin Unduck");
                        duckSound.riseVolStart = newUnduck;
                    }
                    GUI.contentColor = Color.white;

                    GUILayout.Space(4);
                    GUILayout.TextField(duckSound.unduckTime.ToString("N2"), 20, EditorStyles.miniLabel);
                    var newTime = GUILayout.HorizontalSlider(duckSound.unduckTime, 0f, 5f, GUILayout.Width(slidWidth));
                    if (newTime != duckSound.unduckTime) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Unduck Time");
                        duckSound.unduckTime = newTime;
                    }

                    GUILayout.Space(10);
                    if (DTGUIHelper.AddDeleteIcon("Duck Sound")) {
                        duckSoundToRemove = i;
                    }

                    EditorGUILayout.EndHorizontal();
                    DTGUIHelper.EndGroupHeader();
                    DTGUIHelper.AddSpaceForNonU5(2);
                }

                if (duckSoundToRemove.HasValue) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "delete Duck Group");
                    _sounds.musicDuckingSounds.RemoveAt(duckSoundToRemove.Value);
                }

            }
            EditorGUILayout.EndToggleGroup();

            DTGUIHelper.EndGroupedControls();
        }
        // Music Ducking End

        DTGUIHelper.ResetColors();
        // Sound Groups Start		
        EditorGUI.indentLevel = 0;  // Space will handle this for the header
        DTGUIHelper.VerticalSpace(3);

        state = _sounds.areGroupsExpanded;
        text = "Group Mixer";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTGUIHelper.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTGUIHelper.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

        text = "<b><size=11>" + text + "</size></b>";

        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        if (state != _sounds.areGroupsExpanded) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Group Mixer");
            _sounds.areGroupsExpanded = state;
        }

        GUI.color = Color.white;

        DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/GroupMixer.htm");

        EditorGUILayout.EndHorizontal();

        GameObject groupToDelete = null;
        // ReSharper disable once TooWideLocalVariableScope
        // ReSharper disable once RedundantAssignment
        var audSrcTemplateIndex = -1;
        var applyTemplateToAll = false;

        if (_sounds.areGroupsExpanded) {
            DTGUIHelper.BeginGroupedControls();

            EditorGUI.indentLevel = 1;

            DTGUIHelper.StartGroupHeader();
            EditorGUILayout.BeginHorizontal();

            var newShow = DTGUIHelper.Foldout(_sounds.showGroupCreation, "Group Creation");
            if (newShow != _sounds.showGroupCreation) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Group Creation");
                _sounds.showGroupCreation = !_sounds.showGroupCreation;
            }
            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/GroupMixer.htm#GroupCreation");
            EditorGUILayout.EndHorizontal();

            GUI.contentColor = Color.white;
            EditorGUILayout.EndVertical();

            if (_sounds.showGroupCreation) {
                EditorGUI.indentLevel = 0;
                var groupTemplateIndex = -1;

                if (_sounds.audioSourceTemplates.Count > 0 && !Application.isPlaying && _sounds.showGroupCreation && _sounds.transform.childCount > 0) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(2);
                    GUI.contentColor = DTGUIHelper.BrightButtonColor;
                    if (GUILayout.Button("Apply Audio Source Template to All", EditorStyles.toolbarButton, GUILayout.Width(190))) {
                        applyTemplateToAll = true;
                    }

                    GUI.contentColor = Color.white;
                    EditorGUILayout.EndHorizontal();
                }

                var newTemp = EditorGUILayout.Toggle("Use Group Templates", _sounds.useGroupTemplates);
                if (newTemp != _sounds.useGroupTemplates) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Use Group Templates");
                    _sounds.useGroupTemplates = newTemp;
                }

                if (_sounds.useGroupTemplates) {
                    _groupTemplateNames = new List<string>();

                    foreach (var temp in _sounds.groupTemplates) {
                        if (temp == null) {
                            continue;
                        }
                        _groupTemplateNames.Add(temp.name);
                    }

                    var templatesMissing = false;

                    if (Directory.Exists(MasterAudio.GroupTemplateFolder)) {
                        var grpTemplates = Directory.GetFiles(MasterAudio.GroupTemplateFolder, "*.prefab").Length;
                        if (grpTemplates > _sounds.groupTemplates.Count) {
                            templatesMissing = true;
                            DTGUIHelper.ShowLargeBarAlert("There are " + (grpTemplates - _sounds.groupTemplates.Count) + " Group Template(s) that aren't set up in this MA prefab. Locate them in Plugins/DarkTonic/MasterAudio/Sources/Prefabs/GroupTemplates and drag them in below.");
                        }
                    }

                    if (templatesMissing) {
                        // create groups start
                        EditorGUILayout.BeginVertical();
                        var aEvent = Event.current;

                        GUI.color = DTGUIHelper.DragAreaColor;

                        var dragArea = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
                        GUI.Box(dragArea, "Drag prefabs here from Project View to create Group Templates!");

                        GUI.color = Color.white;

                        switch (aEvent.type) {
                            case EventType.DragUpdated:
                            case EventType.DragPerform:
                                if (!dragArea.Contains(aEvent.mousePosition)) {
                                    break;
                                }

                                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                                if (aEvent.type == EventType.DragPerform) {
                                    DragAndDrop.AcceptDrag();

                                    foreach (var dragged in DragAndDrop.objectReferences) {
                                        var temp = dragged as GameObject;
                                        if (temp == null) {
                                            continue;
                                        }

                                        AddGroupTemplate(temp);
                                    }
                                }
                                Event.current.Use();
                                break;
                        }
                        EditorGUILayout.EndVertical();
                        // create groups end
                    }

                    if (_groupTemplateNames.Count == 0) {
                        DTGUIHelper.ShowRedError("You cannot create Groups without Templates. Drag them in or disable Group Templates.");
                    } else {
                        groupTemplateIndex = _groupTemplateNames.IndexOf(_sounds.groupTemplateName);
                        if (groupTemplateIndex < 0) {
                            groupTemplateIndex = 0;
                            _sounds.groupTemplateName = _groupTemplateNames[0];
                        }

                        var newIndex = EditorGUILayout.Popup("Group Template", groupTemplateIndex, _groupTemplateNames.ToArray());
                        if (newIndex != groupTemplateIndex) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Group Template");
                            _sounds.groupTemplateName = _groupTemplateNames[newIndex];
                        }
                    }
                }

                _audioSourceTemplateNames = new List<string>();

                foreach (var temp in _sounds.audioSourceTemplates) {
                    if (temp == null) {
                        continue;
                    }
                    _audioSourceTemplateNames.Add(temp.name);
                }

                var audTemplatesMissing = false;

                if (Directory.Exists(MasterAudio.AudioSourceTemplateFolder)) {
                    var audioSrcTemplates = Directory.GetFiles(MasterAudio.AudioSourceTemplateFolder, "*.prefab").Length;
                    if (audioSrcTemplates > _sounds.audioSourceTemplates.Count) {
                        audTemplatesMissing = true;
                        DTGUIHelper.ShowLargeBarAlert("There are " + (audioSrcTemplates - _sounds.audioSourceTemplates.Count) + " Audio Source Template(s) that aren't set up in this MA prefab. Locate them in Plugins/DarkTonic/MasterAudio/Sources/Prefabs/AudioSourceTemplates and drag them in below.");
                    }
                }

                if (audTemplatesMissing) {
                    // create groups start
                    EditorGUILayout.BeginVertical();
                    var aEvent = Event.current;

                    GUI.color = DTGUIHelper.DragAreaColor;

                    var dragArea = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
                    GUI.Box(dragArea, "Drag prefabs here from Project View to create Audio Source Templates!");

                    GUI.color = Color.white;

                    switch (aEvent.type) {
                        case EventType.DragUpdated:
                        case EventType.DragPerform:
                            if (!dragArea.Contains(aEvent.mousePosition)) {
                                break;
                            }

                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (aEvent.type == EventType.DragPerform) {
                                DragAndDrop.AcceptDrag();

                                foreach (var dragged in DragAndDrop.objectReferences) {
                                    var temp = dragged as GameObject;
                                    if (temp == null) {
                                        continue;
                                    }

                                    AddAudioSourceTemplate(temp);
                                }
                            }
                            Event.current.Use();
                            break;
                    }
                    EditorGUILayout.EndVertical();
                    // create groups end
                }

                if (_audioSourceTemplateNames.Count == 0) {
                    DTGUIHelper.ShowRedError("You have no Audio Source Templates. Drag them in to create them.");
                } else {
                    audSrcTemplateIndex = _audioSourceTemplateNames.IndexOf(_sounds.audioSourceTemplateName);
                    if (audSrcTemplateIndex < 0) {
                        audSrcTemplateIndex = 0;
                        _sounds.audioSourceTemplateName = _audioSourceTemplateNames[0];
                    }

                    var newIndex = EditorGUILayout.Popup("Audio Source Template", audSrcTemplateIndex, _audioSourceTemplateNames.ToArray());
                    if (newIndex != audSrcTemplateIndex) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Audio Source Template");
                        _sounds.audioSourceTemplateName = _audioSourceTemplateNames[newIndex];
                    }
                }

                if (_sounds.useGroupTemplates) {
                    DTGUIHelper.ShowColorWarning("Bulk Creation Mode is ONE GROUP PER CLIP when using Group Templates.");
                } else {
                    var newGroupMode = (MasterAudio.DragGroupMode)EditorGUILayout.EnumPopup("Bulk Creation Mode", _sounds.curDragGroupMode);
                    if (newGroupMode != _sounds.curDragGroupMode) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bulk Creation Mode");
                        _sounds.curDragGroupMode = newGroupMode;
                    }
                }

                var newBulkMode = DTGUIHelper.GetRestrictedAudioLocation("Variation Create Mode", _sounds.bulkLocationMode);
                if (newBulkMode != _sounds.bulkLocationMode) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bulk Variation Mode");
                    _sounds.bulkLocationMode = newBulkMode;
                }

                if (_sounds.bulkLocationMode == MasterAudio.AudioLocation.ResourceFile) {
                    DTGUIHelper.ShowColorWarning("Resource mode: make sure to drag from Resource folders only.");
                }

                var cannotCreateGroups = _sounds.useGroupTemplates && _sounds.groupTemplates.Count == 0;
                MasterAudioGroup createdGroup = null;

                if (!cannotCreateGroups) {
                    // create groups start
                    var anEvent = Event.current;

                    var useGroupTemplate = _sounds.useGroupTemplates && groupTemplateIndex > -1;

                    GUI.color = DTGUIHelper.DragAreaColor;
                    EditorGUILayout.BeginVertical();
                    var dragArea = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
                    GUI.Box(dragArea, MasterAudio.DragAudioTip + " to create Groups!");

                    GUI.color = Color.white;

                    switch (anEvent.type) {
                        case EventType.DragUpdated:
                        case EventType.DragPerform:
                            if (!dragArea.Contains(anEvent.mousePosition)) {
                                break;
                            }

                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (anEvent.type == EventType.DragPerform) {
                                DragAndDrop.AcceptDrag();

                                Transform groupTrans = null;

                                foreach (var dragged in DragAndDrop.objectReferences) {
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_2017_1_OR_NEWER
                                    if (dragged is DefaultAsset) {
                                        var assetPaths = AssetDatabase.FindAssets("t:AudioClip", DragAndDrop.paths);
                                        foreach (var assetPath in assetPaths) {
                                            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(assetPath));
                                            if (clip == null) {
                                                continue;
                                            }

                                            if (useGroupTemplate) {
                                                createdGroup = CreateSoundGroupFromTemplate(clip, groupTemplateIndex);
                                                continue;
                                            }

                                            if (_sounds.curDragGroupMode == MasterAudio.DragGroupMode.OneGroupPerClip) {
                                                CreateSoundGroup(clip);
                                            } else {
                                                if (groupTrans == null) { // one group with variations
                                                    groupTrans = CreateSoundGroup(clip);
                                                } else {
                                                    CreateVariation(groupTrans, clip);
                                                    // create the variations
                                                }
                                            }
                                        }

                                        continue;
                                    }
#endif

                                    var aClip = dragged as AudioClip;
                                    if (aClip == null) {
                                        continue;
                                    }

                                    if (useGroupTemplate) {
                                        createdGroup = CreateSoundGroupFromTemplate(aClip, groupTemplateIndex);
                                        continue;
                                    }

                                    if (_sounds.curDragGroupMode == MasterAudio.DragGroupMode.OneGroupPerClip) {
                                        CreateSoundGroup(aClip);
                                    } else {
                                        if (groupTrans == null) { // one group with variations
                                            groupTrans = CreateSoundGroup(aClip);
                                        } else {
                                            CreateVariation(groupTrans, aClip);
                                            // create the variations
                                        }
                                    }
                                }
                            }
                            Event.current.Use();
                            break;
                    }
                    EditorGUILayout.EndVertical();

                    // create groups end
                }

                if (createdGroup != null) {
                    MasterAudioGroupInspector.RescanChildren(createdGroup);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel = 0;

            DTGUIHelper.AddSpaceForNonU5(2);
            DTGUIHelper.StartGroupHeader();
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Group Control");

            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/GroupMixer.htm#GroupControl");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            DTGUIHelper.ResetColors();

#if UNITY_5 || UNITY_2017_1_OR_NEWER
            DTGUIHelper.StartGroupHeader(1, false);

            var newSpatialType = (MasterAudio.AllMixerSpatialBlendType)EditorGUILayout.EnumPopup("Group Spatial Blend Rule", _sounds.mixerSpatialBlendType);
            if (newSpatialType != _sounds.mixerSpatialBlendType) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Group Spatial Blend Rule");
                _sounds.mixerSpatialBlendType = newSpatialType;

                if (Application.isPlaying) {
                    _sounds.SetSpatialBlendForMixer();
                } else {
                    SetSpatialBlendForGroupsEdit();
                }
            }

            switch (_sounds.mixerSpatialBlendType) {
                case MasterAudio.AllMixerSpatialBlendType.ForceAllToCustom:
                    DTGUIHelper.ShowLargeBarAlert(SpatialBlendSliderText);
                    var newBlend = EditorGUILayout.Slider("Group Spatial Blend", _sounds.mixerSpatialBlend, 0f, 1f);
                    if (newBlend != _sounds.mixerSpatialBlend) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Group Spatial Blend");
                        _sounds.mixerSpatialBlend = newBlend;
                        if (Application.isPlaying) {
                            _sounds.SetSpatialBlendForMixer();
                        } else {
                            SetSpatialBlendForGroupsEdit();
                        }
                    }
                    break;
                case MasterAudio.AllMixerSpatialBlendType.AllowDifferentPerGroup:
                    DTGUIHelper.ShowLargeBarAlert("Go to each Group's settings to change Spatial Blend. Defaults below are for new Groups.");

                    var newDefType = (MasterAudio.ItemSpatialBlendType)EditorGUILayout.EnumPopup("Default Blend Type", _sounds.newGroupSpatialType);
                    if (newDefType != _sounds.newGroupSpatialType) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Default Blend Type");
                        _sounds.newGroupSpatialType = newDefType;
                    }

                    if (_sounds.newGroupSpatialType == MasterAudio.ItemSpatialBlendType.ForceToCustom) {
                        newBlend = EditorGUILayout.Slider("Default Spatial Blend", _sounds.newGroupSpatialBlend, 0f, 1f);
                        if (newBlend != _sounds.newGroupSpatialBlend) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Default  Spatial Blend");
                            _sounds.newGroupSpatialBlend = newBlend;
                        }
                    }
                    break;
            }
            EditorGUILayout.EndVertical();
            DTGUIHelper.ResetColors();
#endif

            EditorGUI.indentLevel = 0;
            var newBusFilterIndex = -1;
            var busFilterActive = false;

            if (_sounds.groupBuses.Count > 0) {
                busFilterActive = true;
                var oldBusFilter = busFilterList.IndexOf(_sounds.busFilter);
                if (oldBusFilter == -1) {
                    oldBusFilter = 0;
                }

                newBusFilterIndex = EditorGUILayout.Popup("Bus Filter", oldBusFilter, busFilterList.ToArray());

                var newBusFilter = busFilterList[newBusFilterIndex];

                if (_sounds.busFilter != newBusFilter) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bus Filter");
                    _sounds.busFilter = newBusFilter;
                }
            }

            if (groups.Count > 0) {
                var newUseTextGroupFilter = EditorGUILayout.Toggle("Use Text Group Filter", _sounds.useTextGroupFilter);
                if (newUseTextGroupFilter != _sounds.useTextGroupFilter) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Use Text Group Filter");
                    _sounds.useTextGroupFilter = newUseTextGroupFilter;
                }

                if (_sounds.useTextGroupFilter) {
                    EditorGUI.indentLevel = 1;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    GUILayout.Label("Text Group Filter", GUILayout.Width(140));
                    var newTextFilter = GUILayout.TextField(_sounds.textGroupFilter, GUILayout.Width(180));
                    if (newTextFilter != _sounds.textGroupFilter) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Text Group Filter");
                        _sounds.textGroupFilter = newTextFilter;
                    }
                    GUILayout.Space(10);
                    GUI.contentColor = DTGUIHelper.BrightButtonColor;
                    if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(70))) {
                        _sounds.textGroupFilter = string.Empty;
                    }
                    GUI.contentColor = Color.white;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Separator();
                }

                if (Application.isPlaying) {
                    var newHide = EditorGUILayout.Toggle("Filter Out Inactive", _sounds.hideGroupsWithNoActiveVars);
                    if (newHide != _sounds.hideGroupsWithNoActiveVars) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Filter Out Inactive");
                        _sounds.hideGroupsWithNoActiveVars = newHide;
                    }
                }
            }

            EditorGUI.indentLevel = 0;
            var groupButtonPressed = DTGUIHelper.DTFunctionButtons.None;

            MasterAudioGroup aGroup = null;
            var filteredGroups = new List<MasterAudioGroup>();

            filteredGroups.AddRange(groups);

            if (busFilterActive && !string.IsNullOrEmpty(_sounds.busFilter)) {
                if (newBusFilterIndex == 0) {
                    // no filter
                } else if (newBusFilterIndex == 1) {
                    filteredGroups.RemoveAll(delegate (MasterAudioGroup obj) {
                        return obj.busIndex != 0;
                    });
                } else {
                    filteredGroups.RemoveAll(delegate (MasterAudioGroup obj) {
                        return obj.busIndex != newBusFilterIndex;
                    });
                }
            }

            if (_sounds.useTextGroupFilter) {
                if (!string.IsNullOrEmpty(_sounds.textGroupFilter)) {
                    filteredGroups.RemoveAll(delegate (MasterAudioGroup obj) {
                        return !obj.transform.name.ToLower().Contains(_sounds.textGroupFilter.ToLower());
                    });
                }
            }

            if (Application.isPlaying && _sounds.hideGroupsWithNoActiveVars) {
                filteredGroups.RemoveAll(delegate (MasterAudioGroup obj) {
                    return obj.ActiveVoices == 0;
                });
            }

            var totalVoiceCount = 0;

            var selectAll = false;
            var deselectAll = false;
            var applyAudioSourceTemplate = false;
            var applyDuckingBulk = false;

            var bulkSelectedGrps = new List<MasterAudioGroup>(filteredGroups.Count);

            if (groups.Count == 0) {
                DTGUIHelper.ShowLargeBarAlert("You currently have zero Sound Groups.");
            } else {
                var groupsFiltered = groups.Count - filteredGroups.Count;
                if (groupsFiltered > 0) {
                    DTGUIHelper.ShowLargeBarAlert(string.Format("{0}/{1} Group(s) filtered out.", groupsFiltered, groups.Count));
                }

                var allFilteredOut = groupsFiltered == groups.Count;

                if (_sounds.groupBuses.Count > 0 && !allFilteredOut) {
                    var newGroupByBus = EditorGUILayout.Toggle("Group by Bus", _sounds.groupByBus);
                    if (newGroupByBus != _sounds.groupByBus) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Group by Bus");
                        _sounds.groupByBus = newGroupByBus;
                    }
                }

                if (filteredGroups.Count > 0) {
                    var newSelectGrp = EditorGUILayout.Toggle("Bulk Group Changes", _sounds.showGroupSelect);
                    if (newSelectGrp != _sounds.showGroupSelect) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Bulk Group Changes");
                        _sounds.showGroupSelect = newSelectGrp;
                    }

                    if (_sounds.showGroupSelect) {
                        GUI.contentColor = DTGUIHelper.BrightButtonColor;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(0);
                        if (GUILayout.Button("Select All", EditorStyles.toolbarButton, GUILayout.Width(70))) {
                            selectAll = true;
                        }
                        GUILayout.Space(6);
                        if (GUILayout.Button("Deselect All", EditorStyles.toolbarButton, GUILayout.Width(80))) {
                            deselectAll = true;
                        }

                        foreach (var t in filteredGroups) {
                            if (t.isSelected) {
                                bulkSelectedGrps.Add(t);
                            }
                        }

                        if (!Application.isPlaying && _audioSourceTemplateNames.Count > 0 && bulkSelectedGrps.Count > 0) {
                            GUILayout.Space(6);
                            GUI.contentColor = DTGUIHelper.BrightButtonColor;
                            if (GUILayout.Button("Apply Audio Source Template", EditorStyles.toolbarButton, GUILayout.Width(160))) {
                                applyAudioSourceTemplate = true;
                            }

                            GUILayout.Space(6);
                            if (GUILayout.Button("Add To Ducking", EditorStyles.toolbarButton, GUILayout.Width(100))) {
                                applyDuckingBulk = true;
                            }

                        }
                        EditorGUILayout.EndHorizontal();
                        GUI.contentColor = Color.white;

                        if (bulkSelectedGrps.Count > 0) {
                            DTGUIHelper.ShowLargeBarAlert("Bulk editing - adjustments to a selected mixer Group will affect all " + bulkSelectedGrps.Count + " selected Group(s).");
                        } else {
                            EditorGUILayout.Separator();
                        }
                    }
                }

                var isBulkMute = false;
                var isBulkSolo = false;
                float? bulkVolume = null;
                int? bulkBusIndex = null;
                int? bulkBusToCreate = null;
                int? singleBusToCreate = null;

                DTGUIHelper.ResetColors();

                for (var l = 0; l < filteredGroups.Count; l++) {
                    EditorGUI.indentLevel = 0;
                    aGroup = filteredGroups[l];

                    var groupDirty = false;
                    var isBulkEdit = bulkSelectedGrps.Count > 0 && aGroup.isSelected;

                    var sType = string.Empty;
                    if (Application.isPlaying) {
                        sType = aGroup.name;
                    }

                    if (string.IsNullOrEmpty(sType)) { } // get rid of warning

                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                    var groupName = aGroup.name;

                    if (Application.isPlaying) {
                        var groupVoices = aGroup.ActiveVoices;
                        var totalVoices = aGroup.TotalVoices;

                        GUI.color = DTGUIHelper.BrightTextColor;

                        if (groupVoices == 0) {
                            GUI.contentColor = Color.white;
                            GUI.backgroundColor = Color.white;
                            GUI.color = DTGUIHelper.InactiveMixerGroupColor;
                        } else if (groupVoices >= totalVoices) {
                            GUI.contentColor = Color.red;
                            GUI.backgroundColor = Color.red;
                        }

                        if (GUILayout.Button(new GUIContent(string.Format("[{0}]", groupVoices), "Click to select Variations"), EditorStyles.toolbarButton)) {
                            SelectActiveVariationsInGroup(aGroup);
                        }
                        DTGUIHelper.ResetColors();

                        switch (aGroup.GroupLoadStatus) {
                            case MasterAudio.InternetFileLoadStatus.Loading:
                                GUILayout.Button(
                                    new GUIContent(MasterAudioInspectorResources.LoadingTexture,
                                        "Downloading 1 or more Internet Files"), EditorStyles.toolbarButton, GUILayout.Width(24));
                                break;
                            case MasterAudio.InternetFileLoadStatus.Failed:
                                GUILayout.Button(
                                    new GUIContent(MasterAudioInspectorResources.ErrorTexture,
                                        "1 or more Internet Files failed download"), EditorStyles.toolbarButton, GUILayout.Width(24));
                                break;
                            case MasterAudio.InternetFileLoadStatus.Loaded:
                                GUILayout.Button(
                                    new GUIContent(MasterAudioInspectorResources.ReadyTexture,
                                        "All audio is ready to play"), EditorStyles.toolbarButton, GUILayout.Width(24));
                                break;
                        }

                        totalVoiceCount += groupVoices;
                    }

                    if (_sounds.showGroupSelect) {
                        var newChecked = EditorGUILayout.Toggle(aGroup.isSelected, EditorStyles.toggleGroup, GUILayout.Width(16));
                        if (newChecked != aGroup.isSelected) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref groupDirty, aGroup, "toggle Select Group");
                            aGroup.isSelected = newChecked;
                        }
                    }

                    var minWidth = 100;
                    if (_sounds.MixerWidth == MasterAudio.MixerWidthMode.Narrow) {
                        minWidth = NarrowWidth;
                    }

                    EditorGUILayout.LabelField(groupName, EditorStyles.label, GUILayout.MinWidth(minWidth));

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(50));

                    if (_sounds.MixerWidth != MasterAudio.MixerWidthMode.Narrow || _sounds.BusesShownInNarrow) {
                        // find bus.
                        var selectedBusIndex = aGroup.busIndex == -1 ? 0 : aGroup.busIndex;

                        GUI.contentColor = Color.white;
                        GUI.color = DTGUIHelper.BrightButtonColor;

                        var busIndex = EditorGUILayout.Popup("", selectedBusIndex, busList.ToArray(),
                            GUILayout.Width(busListWidth));
                        if (busIndex == -1) {
                            busIndex = 0;
                        }

                        if (aGroup.busIndex != busIndex && busIndex != 1) {
                            if (isBulkEdit) {
                                // don't change the index here for bulk edit, so undo will work.
                                bulkBusIndex = busIndex;
                            } else if (!Application.isPlaying) {  // no undoing this unless in edit mode.
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref groupDirty, aGroup, "change Group Bus");
                            }
                        }

                        GUI.color = Color.white;

                        if (selectedBusIndex != busIndex) {
                            if (busIndex == 0) {
                                if (Application.isPlaying) {
                                    MasterAudio.RouteGroupToBus(sType, null);
                                } else {
                                    aGroup.busIndex = busIndex;
                                }
                            } else if (busIndex == 1) {
                                if (isBulkEdit) {
                                    bulkBusToCreate = l;
                                } else {
                                    singleBusToCreate = l;
                                }
                            } else if (busIndex >= MasterAudio.HardCodedBusOptions && !isBulkEdit) {
                                var newBus = _sounds.groupBuses[busIndex - MasterAudio.HardCodedBusOptions];
                                if (Application.isPlaying) {
                                    MasterAudio.RouteGroupToBus(aGroup.name, newBus.busName);
                                } else {
                                    aGroup.busIndex = busIndex;

                                    // check if bus soloed or muted and copy that only.
                                    if (newBus.isMuted) {
                                        aGroup.isMuted = true;
                                        aGroup.isSoloed = false;
                                    } else if (newBus.isSoloed) {
                                        aGroup.isMuted = false;
                                        aGroup.isSoloed = true;
                                    }
                                }
                            }
                        }
                    }

                    GUI.contentColor = DTGUIHelper.BrightTextColor;

                    if (_sounds.MixerWidth != MasterAudio.MixerWidthMode.Narrow) {
                        GUILayout.TextField(
                            DTGUIHelper.DisplayVolumeNumber(aGroup.groupMasterVolume, sliderIndicatorChars),
                            sliderIndicatorChars, EditorStyles.miniLabel, GUILayout.Width(sliderWidth));
                    }

                    var newVol = DTGUIHelper.DisplayVolumeField(aGroup.groupMasterVolume, DTGUIHelper.VolumeFieldType.MixerGroup, _sounds.MixerWidth);
                    if (newVol != aGroup.groupMasterVolume) {
                        if (isBulkEdit) {
                            bulkVolume = newVol;
                        } else {
                            SetMixerGroupVolume(aGroup, ref groupDirty, newVol, false);
                        }
                    }

                    GUI.contentColor = Color.white;
                    DTGUIHelper.AddLedSignalLight(_sounds, groupName);

                    groupButtonPressed = DTGUIHelper.AddMixerButtons(aGroup, "Group");

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();

                    switch (groupButtonPressed) {
                        case DTGUIHelper.DTFunctionButtons.Find:
                            DTGUIHelper.ShowFilteredRelationsGraph(aGroup.name);
                            break;
                        case DTGUIHelper.DTFunctionButtons.Play:
                            previewer = GetPreviewer();

                            if (Application.isPlaying) {
                                if (previewer != null) {
                                    MasterAudio.PlaySound3DAtVector3AndForget(aGroup.name, previewer.transform.position);
                                }
                            } else {
                                _isDirty = true;

                                var rndIndex = Random.Range(0, aGroup.groupVariations.Count);
                                var rndVar = aGroup.groupVariations[rndIndex];

                                var randPitch = SoundGroupVariationInspector.GetRandomPreviewPitch(rndVar);
                                var varVol = SoundGroupVariationInspector.GetRandomPreviewVolume(rndVar);

                                if (rndVar.audLocation != MasterAudio.AudioLocation.FileOnInternet) {
                                    if (previewer != null) {
                                        StopPreviewer();
                                        previewer.pitch = randPitch;
                                    }
                                }

                                var calcVolume = aGroup.groupMasterVolume * varVol;

                                switch (rndVar.audLocation) {
                                    case MasterAudio.AudioLocation.ResourceFile:
                                        if (previewer != null) {
                                            var fileName = AudioResourceOptimizer.GetLocalizedFileName(rndVar.useLocalization, rndVar.resourceFileName);
                                            previewer.PlayOneShot(Resources.Load(fileName) as AudioClip, calcVolume);
                                        }
                                        break;
                                    case MasterAudio.AudioLocation.Clip:
                                        if (previewer != null) {
                                            previewer.PlayOneShot(rndVar.VarAudio.clip, calcVolume);
                                        }
                                        break;
                                    case MasterAudio.AudioLocation.FileOnInternet:
                                        if (!string.IsNullOrEmpty(rndVar.internetFileUrl)) {
                                            Application.OpenURL(rndVar.internetFileUrl);
                                        }
                                        break;
                                }
                            }
                            break;
                        case DTGUIHelper.DTFunctionButtons.Stop:
                            if (Application.isPlaying) {
                                MasterAudio.StopAllOfSound(aGroup.name);
                            } else {
                                StopPreviewer();
                            }
                            break;
                        case DTGUIHelper.DTFunctionButtons.Mute:
                            if (isBulkEdit) {
                                isBulkMute = true;
                            } else {
                                MuteMixerGroup(aGroup, ref groupDirty, false, false);
                            }
                            break;
                        case DTGUIHelper.DTFunctionButtons.Solo:
                            if (isBulkEdit) {
                                isBulkSolo = true;
                            } else {
                                SoloMixerGroup(aGroup, ref groupDirty, false, false);
                            }
                            break;
                        case DTGUIHelper.DTFunctionButtons.Go:
                            Selection.activeObject = aGroup.transform;
                            break;
                        case DTGUIHelper.DTFunctionButtons.Remove:
                            groupToDelete = aGroup.transform.gameObject;
                            break;
                    }

                    if (groupDirty) {
                        EditorUtility.SetDirty(aGroup);
                    }
                }

                if (isBulkMute) {
                    AudioUndoHelper.RecordObjectsForUndo(bulkSelectedGrps.ToArray(), "Bulk Mute");

                    var wasWarningShown = false;

                    foreach (var grp in bulkSelectedGrps) {
                        wasWarningShown = MuteMixerGroup(grp, ref fakeDirty, true, wasWarningShown);
                        EditorUtility.SetDirty(grp);
                    }
                }

                if (isBulkSolo) {
                    AudioUndoHelper.RecordObjectsForUndo(bulkSelectedGrps.ToArray(), "Bulk Solo");

                    var wasWarningShown = false;

                    foreach (var grp in bulkSelectedGrps) {
                        wasWarningShown = SoloMixerGroup(grp, ref fakeDirty, true, wasWarningShown);
                        EditorUtility.SetDirty(grp);
                    }
                }

                if (bulkVolume.HasValue) {
                    AudioUndoHelper.RecordObjectsForUndo(bulkSelectedGrps.ToArray(), "Bulk Volume Adjustment");

                    foreach (var grp in bulkSelectedGrps) {
                        SetMixerGroupVolume(grp, ref fakeDirty, bulkVolume.Value, true);
                        EditorUtility.SetDirty(grp);
                    }
                }

                if (bulkBusIndex.HasValue) {
                    if (!Application.isPlaying) { // no undoing this unless in edit mode.
                        AudioUndoHelper.RecordObjectsForUndo(bulkSelectedGrps.ToArray(), "Bulk Bus Assignment");
                    }

                    foreach (var grp in bulkSelectedGrps) {
                        string busName = null;
                        GroupBus theBus = null;
                        if (bulkBusIndex.Value >= MasterAudio.HardCodedBusOptions) {
                            theBus = _sounds.groupBuses[bulkBusIndex.Value - MasterAudio.HardCodedBusOptions];
                            busName = theBus.busName;
                        }

                        if (Application.isPlaying) {
                            MasterAudio.RouteGroupToBus(grp.name, busName);
                        } else {
                            grp.busIndex = bulkBusIndex.Value;

                            if (theBus != null) {
                                if (theBus.isMuted) {
                                    grp.isMuted = true;
                                    grp.isSoloed = false;
                                } else if (theBus.isSoloed) {
                                    grp.isMuted = false;
                                    grp.isSoloed = true;
                                }
                            }
                        }

                        EditorUtility.SetDirty(grp);
                    }
                }

                if (singleBusToCreate.HasValue) {
                    CreateBus(singleBusToCreate.Value);
                } else if (bulkBusToCreate.HasValue) {
                    if (!Application.isPlaying) {
                        AudioUndoHelper.RecordObjectsForUndo(bulkSelectedGrps.ToArray(), "Bulk Bus Assignment");
                    }

                    var i = 0;
                    var newBusName = string.Empty;

                    foreach (var grp in bulkSelectedGrps) {
                        if (i == 0) {
                            newBusName = CreateBus(bulkBusToCreate.Value);
                        } else {
                            MasterAudio.RouteGroupToBus(grp.name, newBusName);
                        }

                        EditorUtility.SetDirty(grp);
                        i++;
                    }
                }

                if (groupToDelete != null) {
                    var grpName = groupToDelete.name;
                    if (Application.isPlaying) {
                        var grp = MasterAudio.GrabGroup(grpName, false);
                        if (grp != null && grp.isSoloed) {
                            MasterAudio.UnsoloGroup(grpName);
                        }
                    }

                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Delete Duck Group");
                    _sounds.musicDuckingSounds.RemoveAll(delegate (DuckGroupInfo obj) {
                        return obj.soundType == grpName;
                    });
                    AudioUndoHelper.DestroyForUndo(groupToDelete);
                    MasterAudio.RescanGroupsNow();
                }

                if (Application.isPlaying) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(9);
                    GUI.color = DTGUIHelper.BrightTextColor;
                    EditorGUILayout.LabelField(string.Format("[{0}] Total Active Voices", totalVoiceCount));
                    GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();

                if (_sounds.MixerWidth != MasterAudio.MixerWidthMode.Narrow) {
                    GUILayout.Space(10);
                }

                GUI.contentColor = DTGUIHelper.BrightButtonColor;
                if (GUILayout.Button(new GUIContent("Mute/Solo Reset", "Turn off all Group mute and solo switches"), EditorStyles.toolbarButton, GUILayout.Width(100))) {
                    MuteSoloReset();
                }

                GUILayout.Space(6);

                if (GUILayout.Button(new GUIContent("Max Grp. Volumes", "Reset all group volumes to full"), EditorStyles.toolbarButton, GUILayout.Width(104))) {
                    AudioUndoHelper.RecordObjectsForUndo(groups.ToArray(), "Max Grp. Volumes");

                    foreach (var t in groups) {
                        aGroup = t;
                        aGroup.groupMasterVolume = 1f;
                    }
                }

                GUI.contentColor = Color.white;

                EditorGUILayout.EndHorizontal();

                if (_sounds.MixerWidth == MasterAudio.MixerWidthMode.Narrow) {
                    DTGUIHelper.ShowColorWarning("Some controls are hidden from the Group Mixer in narrow mode.");
                }
            }

            if (selectAll) {
                AudioUndoHelper.RecordObjectsForUndo(filteredGroups.ToArray(), "Select Groups");

                foreach (var myGroup in filteredGroups) {
                    myGroup.isSelected = true;
                }
            }
            if (deselectAll) {
                AudioUndoHelper.RecordObjectsForUndo(filteredGroups.ToArray(), "Deselect Groups");

                foreach (var myGroup in filteredGroups) {
                    myGroup.isSelected = false;
                }
            }
            if (applyAudioSourceTemplate) {
                AudioUndoHelper.RecordObjectsForUndo(filteredGroups.ToArray(), "Apply Audio Source Template");

                foreach (var myGroup in filteredGroups) {
                    if (!myGroup.isSelected) {
                        continue;
                    }

                    for (var v = 0; v < myGroup.transform.childCount; v++) {
                        var aVar = myGroup.transform.GetChild(v);
                        var oldAudio = aVar.GetComponent<AudioSource>();
                        CopyFromAudioSourceTemplate(oldAudio, true);
                    }
                }
            }

            if (applyDuckingBulk) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Add to Ducking");

                foreach (var myGroup in filteredGroups) {
                    if (!myGroup.isSelected) {
                        continue;
                    }

                    if (_sounds.musicDuckingSounds.Find(delegate (DuckGroupInfo dg) {
                        return dg.soundType == myGroup.name;
                    }) != null) {
                        continue;
                    }

                    _sounds.musicDuckingSounds.Add(new DuckGroupInfo() {
                        soundType = myGroup.name,
                        riseVolStart = _sounds.defaultRiseVolStart,
                        duckedVolumeCut = _sounds.defaultDuckedVolumeCut,
                        unduckTime = _sounds.defaultUnduckTime
                    });
                }
            }

            if (applyTemplateToAll) {
                AudioUndoHelper.RecordObjectsForUndo(filteredGroups.ToArray(), "Apply Audio Source Template to All");

                foreach (var myGroup in filteredGroups) {
                    for (var v = 0; v < myGroup.transform.childCount; v++) {
                        var aVar = myGroup.transform.GetChild(v);
                        var oldAudio = aVar.GetComponent<AudioSource>();
                        CopyFromAudioSourceTemplate(oldAudio, true);
                    }
                }
            }
            EditorGUILayout.EndVertical();
            // Sound Groups End

            // Buses
            if (_sounds.groupBuses.Count > 0) {
                DTGUIHelper.VerticalSpace(3);

                var voiceLimitedBuses = _sounds.groupBuses.FindAll(delegate (GroupBus obj) {
                    return obj.voiceLimit >= 0;
                });

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Bus Control", GUILayout.Width(74));
                if (voiceLimitedBuses.Count > 0) {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Stop Oldest", GUILayout.Width(100));
                    var endSpace = 284;
                    switch (_sounds.MixerWidth) {
                        case MasterAudio.MixerWidthMode.Wide:
                            endSpace = 488;
                            break;
                        case MasterAudio.MixerWidthMode.Narrow:
                            endSpace = 214;
                            break;
                    }
                    GUILayout.Space(endSpace);
                }
                DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/GroupMixer.htm#BusMixer", true);
                EditorGUILayout.EndHorizontal();

                GroupBus aBus = null;
                var busButtonPressed = DTGUIHelper.DTFunctionButtons.None;
                int? busToDelete = null;
                int? busToSolo = null;
                int? busToMute = null;
                int? busToStop = null;

                for (var i = 0; i < _sounds.groupBuses.Count; i++) {
                    aBus = _sounds.groupBuses[i];

                    DTGUIHelper.StartGroupHeader(1, false);

                    if (_sounds.ShouldShowUnityAudioMixerGroupAssignments) {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                    } else {
                        EditorGUILayout.BeginHorizontal();
                    }

                    GUI.color = Color.gray;

                    if (Application.isPlaying) {
                        GUI.color = DTGUIHelper.BrightTextColor;
                        if (aBus.BusVoiceLimitReached) {
                            GUI.contentColor = Color.red;
                            GUI.backgroundColor = Color.red;
                        }
                        if (GUILayout.Button(string.Format("[{0:D2}]", aBus.ActiveVoices), EditorStyles.toolbarButton)) {
                            SelectActiveVariationsInBus(aBus);
                        }
                        DTGUIHelper.ResetColors();
                    }

                    GUI.color = Color.white;
                    var nameWidth = 170;
                    if (_sounds.MixerWidth == MasterAudio.MixerWidthMode.Narrow) {
                        nameWidth = NarrowWidth;
                    }

                    var newBusName = EditorGUILayout.TextField("", aBus.busName, GUILayout.MinWidth(nameWidth));
                    if (newBusName != aBus.busName) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bus Name");
                        aBus.busName = newBusName;
                    }

                    GUILayout.FlexibleSpace();
                    if (voiceLimitedBuses.Contains(aBus)) {
                        GUI.color = DTGUIHelper.BrightButtonColor;
                        var newMono = GUILayout.Toggle(aBus.stopOldest, new GUIContent("", "Checking this box will make it so when the voice limit is already reached and you play a sound, the oldest sound will first be stopped."));
                        if (newMono != aBus.stopOldest) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Stop Oldest");
                            aBus.stopOldest = newMono;
                        }
                    }

                    GUI.color = Color.white;
                    GUILayout.Label("Voices");

                    GUI.color = DTGUIHelper.BrightButtonColor;
                    var oldLimitIndex = busVoiceLimitList.IndexOf(aBus.voiceLimit.ToString());
                    if (oldLimitIndex == -1) {
                        oldLimitIndex = 0;
                    }
                    var busVoiceLimitIndex = EditorGUILayout.Popup("", oldLimitIndex, busVoiceLimitList.ToArray(), GUILayout.MaxWidth(70));
                    if (busVoiceLimitIndex != oldLimitIndex) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bus Voice Limit");
                        aBus.voiceLimit = busVoiceLimitIndex <= 0 ? -1 : busVoiceLimitIndex;
                    }

                    GUI.color = Color.white;

                    EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(50));

                    if (_sounds.MixerWidth != MasterAudio.MixerWidthMode.Narrow) {
                        GUI.color = DTGUIHelper.BrightTextColor;
                        GUILayout.TextField(DTGUIHelper.DisplayVolumeNumber(aBus.volume, sliderIndicatorChars),
                            sliderIndicatorChars, EditorStyles.miniLabel, GUILayout.Width(sliderWidth));
                    }

                    GUI.color = Color.white;
                    var newBusVol = DTGUIHelper.DisplayVolumeField(aBus.volume, DTGUIHelper.VolumeFieldType.Bus, _sounds.MixerWidth);
                    if (newBusVol != aBus.volume) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bus Volume");
                        aBus.volume = newBusVol;
                        if (Application.isPlaying) {
                            MasterAudio.SetBusVolumeByName(aBus.busName, aBus.volume);
                        }
                    }

                    GUI.contentColor = Color.white;

                    busButtonPressed = DTGUIHelper.AddMixerBusButtons(aBus);

                    switch (busButtonPressed) {
                        case DTGUIHelper.DTFunctionButtons.Remove:
                            busToDelete = i;
                            break;
                        case DTGUIHelper.DTFunctionButtons.Solo:
                            busToSolo = i;
                            break;
                        case DTGUIHelper.DTFunctionButtons.Mute:
                            busToMute = i;
                            break;
                        case DTGUIHelper.DTFunctionButtons.Stop:
                            busToStop = i;
                            break;
                        case DTGUIHelper.DTFunctionButtons.Find:
                            DTGUIHelper.ShowFilteredRelationsGraph(null, aBus.busName);
                            break;
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();

#if UNITY_5 || UNITY_2017_1_OR_NEWER
                    if (_sounds.ShouldShowUnityAudioMixerGroupAssignments) {
                        var newChan = (AudioMixerGroup)EditorGUILayout.ObjectField(aBus.mixerChannel, typeof(AudioMixerGroup), false);
                        if (newChan != aBus.mixerChannel) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bus Mixer Group");
                            aBus.mixerChannel = newChan;
                            MasterAudio.RouteBusToUnityMixerGroup(aBus.busName, newChan);
                        }
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.BeginHorizontal();

                    if (_sounds.mixerSpatialBlendType != MasterAudio.AllMixerSpatialBlendType.ForceAllTo2D) {
                        var new2D = GUILayout.Toggle(aBus.forceTo2D, "Force to 2D");
                        if (new2D != aBus.forceTo2D) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Force to 2D");
                            aBus.forceTo2D = new2D;
                        }
                    }

                    if (MasterAudio.Instance.useOcclusion && MasterAudio.Instance.occlusionSelectType != MasterAudio.OcclusionSelectionType.AllGroups) {

                        var is2D = false;

#if UNITY_5 || UNITY_2017_1_OR_NEWER
                        switch (MasterAudio.Instance.mixerSpatialBlendType) {
                            case MasterAudio.AllMixerSpatialBlendType.ForceAllTo2D:
                                is2D = true;
                                break;
                            case MasterAudio.AllMixerSpatialBlendType.ForceAllToCustom:
                                is2D = MasterAudio.Instance.mixerSpatialBlend <= 0;
                                break;
                        }
#endif
                        if (!is2D && aBus.forceTo2D) {
                            is2D = true;
                        }

                        if (!is2D) {
                            var newOcc = GUILayout.Toggle(aBus.isUsingOcclusion, "Use Occlusion");
                            if (newOcc != aBus.isUsingOcclusion) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Use Occlusion");
                                aBus.isUsingOcclusion = newOcc;
                            }
                        }
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

#endif
                    EditorGUILayout.EndVertical();
                    DTGUIHelper.AddSpaceForNonU5(2);
                }

                if (busToDelete.HasValue) {
                    DeleteBus(busToDelete.Value);
                }
                if (busToMute.HasValue) {
                    MuteBus(busToMute.Value);
                }
                if (busToSolo.HasValue) {
                    SoloBus(busToSolo.Value);
                }
                if (busToStop.HasValue) {
                    MasterAudio.StopBus(_sounds.groupBuses[busToStop.Value].busName);
                }

                if (Application.isPlaying) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(9);
                    GUI.color = DTGUIHelper.BrightTextColor;
                    EditorGUILayout.LabelField(string.Format("[{0:D2}] Total Active Voices", totalVoiceCount));
                    GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(8);
                GUI.contentColor = DTGUIHelper.BrightButtonColor;
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button(new GUIContent("Mute/Solo Reset", "Turn off all bus mute and solo switches"), EditorStyles.toolbarButton, GUILayout.Width(100))) {
                    MuteSoloReset();
                }

                GUILayout.Space(6);

                if (GUILayout.Button(new GUIContent("Max Bus Volumes", "Reset all bus volumes to full"), EditorStyles.toolbarButton, GUILayout.Width(100))) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Max Bus Volumes");

                    foreach (var t in _sounds.groupBuses) {
                        aBus = t;
                        aBus.volume = 1f;
                    }
                }

#if UNITY_5 || UNITY_2017_1_OR_NEWER
                GUILayout.Space(6);

                GUI.contentColor = DTGUIHelper.BrightButtonColor;
                var buttonText = "Show Unity Mixer Groups";
                if (_sounds.showUnityMixerGroupAssignment) {
                    buttonText = "Hide Unity Mixer Groups";
                }
                if (GUILayout.Button(new GUIContent(buttonText), EditorStyles.toolbarButton, GUILayout.Width(140))) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, buttonText);
                    _sounds.showUnityMixerGroupAssignment = !_sounds.showUnityMixerGroupAssignment;
                }
#endif

                GUI.contentColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            DTGUIHelper.EndGroupedControls();
        }
        // Sound Buses End

        // Music playlist Start		
        DTGUIHelper.VerticalSpace(3);
        EditorGUILayout.BeginHorizontal();
        EditorGUI.indentLevel = 0;  // Space will handle this for the header

        DTGUIHelper.ResetColors();


        state = _sounds.playListExpanded;
        text = "Playlist Settings";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTGUIHelper.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTGUIHelper.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

        text = "<b><size=11>" + text + "</size></b>";

        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        var isExp = state;


        if (isExp != _sounds.playListExpanded) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Playlist Settings");
            _sounds.playListExpanded = isExp;
        }

        DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/PlaylistSettings.htm");

        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;

        EditorGUILayout.EndHorizontal();


        if (_sounds.playListExpanded) {
            DTGUIHelper.BeginGroupedControls();

            if (_sounds.MixerWidth == MasterAudio.MixerWidthMode.Narrow) {
                DTGUIHelper.ShowColorWarning("Some controls are hidden from Playlist Control in narrow mode.");
            }

#if UNITY_5 || UNITY_2017_1_OR_NEWER
            DTGUIHelper.StartGroupHeader(1, false);
            var newMusicSpatialType = (MasterAudio.AllMusicSpatialBlendType)EditorGUILayout.EnumPopup("Music Spatial Blend Rule", _sounds.musicSpatialBlendType);
            if (newMusicSpatialType != _sounds.musicSpatialBlendType) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Music Spatial Blend Rule");
                _sounds.musicSpatialBlendType = newMusicSpatialType;

                if (Application.isPlaying) {
                    SetSpatialBlendsForPlaylistControllers();
                } else {
                    SetSpatialBlendForPlaylistsEdit();
                }
            }

            switch (_sounds.musicSpatialBlendType) {
                case MasterAudio.AllMusicSpatialBlendType.ForceAllToCustom:
                    DTGUIHelper.ShowLargeBarAlert(SpatialBlendSliderText);
                    var newMusic3D = EditorGUILayout.Slider("Music Spatial Blend", _sounds.musicSpatialBlend, 0f, 1f);
                    if (newMusic3D != _sounds.musicSpatialBlend) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Music Spatial Blend");
                        _sounds.musicSpatialBlend = newMusic3D;
                        if (Application.isPlaying) {
                            SetSpatialBlendsForPlaylistControllers();
                        } else {
                            SetSpatialBlendForPlaylistsEdit();
                        }
                    }
                    break;
                case MasterAudio.AllMusicSpatialBlendType.AllowDifferentPerController:
                    DTGUIHelper.ShowLargeBarAlert("To set Spatial Blend, go to each Playlist Controller and change it there.");
                    break;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
#endif

            EditorGUI.indentLevel = 0;  // Space will handle this for the header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Playlist Controller Setup", EditorStyles.miniBoldLabel, GUILayout.Width(130));

            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/PlaylistSettings.htm#PlaylistControllerSetup", true);
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel = 0;
            DTGUIHelper.StartGroupHeader();
            EditorGUILayout.BeginHorizontal();
            const string labelText = "Name";
            labelWidth = 146;
            if (_sounds.MixerWidth == MasterAudio.MixerWidthMode.Narrow) {
                labelWidth = NarrowWidth;
            }
            EditorGUILayout.LabelField(labelText, EditorStyles.miniBoldLabel, GUILayout.Width(labelWidth));
            if (plControllerInScene) {
                GUILayout.FlexibleSpace();
                if (_sounds.MixerWidth != MasterAudio.MixerWidthMode.Narrow) {
                    EditorGUILayout.LabelField("Sync Grp.", EditorStyles.miniBoldLabel, GUILayout.Width(54));
                }
                EditorGUILayout.LabelField("Initial Playlist", EditorStyles.miniBoldLabel, GUILayout.Width(100));
                var endLength = 208;
                if (Application.isPlaying) {
                    endLength = 182;
                }
                if (_sounds.MixerWidth == MasterAudio.MixerWidthMode.Narrow) {
                    endLength -= 58;
                }

                GUILayout.Space(endLength - extraPlaylistLength);
            }
            EditorGUILayout.EndHorizontal();

            if (!plControllerInScene) {
                DTGUIHelper.ShowLargeBarAlert("There are no Playlist Controllers in the scene. Music will not play.");
            } else {
                int? indexToDelete = null;

                _playlistNames.Insert(0, MasterAudio.NoPlaylistName);

                var syncGroupList = new List<string>();
                for (var i = 0; i < 4; i++) {
                    syncGroupList.Add((i + 1).ToString());
                }
                syncGroupList.Insert(0, MasterAudio.NoGroupName);

                for (var i = 0; i < pcs.Count; i++) {
                    var controller = pcs[i];
                    DTGUIHelper.StartGroupHeader(1, false);

#if UNITY_5 || UNITY_2017_1_OR_NEWER
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
#else
                    EditorGUILayout.BeginHorizontal();
#endif

                    GUILayout.Label(controller.name, _sounds.MixerWidth == MasterAudio.MixerWidthMode.Narrow ? GUILayout.MinWidth(NarrowWidth) : GUILayout.MinWidth(105));

                    GUILayout.FlexibleSpace();

                    var ctrlDirty = false;

                    if (_sounds.MixerWidth != MasterAudio.MixerWidthMode.Narrow) {
                        GUI.color = DTGUIHelper.BrightButtonColor;
                        var syncIndex = syncGroupList.IndexOf(controller.syncGroupNum.ToString());
                        if (syncIndex == -1) {
                            syncIndex = 0;
                        }
                        var newSync = EditorGUILayout.Popup("", syncIndex, syncGroupList.ToArray(), GUILayout.Width(55));
                        if (newSync != syncIndex) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref ctrlDirty, controller,
                                "change Controller Sync Group");
                            controller.syncGroupNum = newSync;
                        }
                    }

                    var origIndex = _playlistNames.IndexOf(controller.startPlaylistName);
                    var isCustomPlaylist = false;
                    if (origIndex == -1) {
                        if (string.IsNullOrEmpty(controller.startPlaylistName) || controller.startPlaylistName.StartsWith("[")) {
                            origIndex = 0;
                            controller.startPlaylistName = string.Empty;
                        } else {
                            isCustomPlaylist = true;
                        }
                    }

                    if (isCustomPlaylist) {
                        EditorGUILayout.LabelField(controller.startPlaylistName, GUILayout.Width(playlistListWidth));
                    } else {
                        var newIndex = EditorGUILayout.Popup("", origIndex, _playlistNames.ToArray(),
                                GUILayout.Width(playlistListWidth));
                        if (newIndex != origIndex) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref ctrlDirty, controller,
                                "change Playlist Controller initial Playlist");
                            controller.startPlaylistName = _playlistNames[newIndex];
                        }
                    }
                    GUI.color = Color.white;

                    if (_sounds.MixerWidth != MasterAudio.MixerWidthMode.Narrow) {
                        GUI.contentColor = DTGUIHelper.BrightButtonColor;
                        GUILayout.TextField(
                            DTGUIHelper.DisplayVolumeNumber(controller._playlistVolume, sliderIndicatorChars),
                            sliderIndicatorChars, EditorStyles.miniLabel, GUILayout.Width(sliderWidth));
                    }
                    var newVol = DTGUIHelper.DisplayVolumeField(controller._playlistVolume, DTGUIHelper.VolumeFieldType.PlaylistController, _sounds.MixerWidth);

                    if (newVol != controller._playlistVolume) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref ctrlDirty, controller, "change Playlist Controller volume");
                        controller.PlaylistVolume = newVol;
                    }

                    GUI.contentColor = Color.white;

                    var buttonPressed = DTGUIHelper.AddPlaylistControllerSetupButtons(controller, "Playlist Controller", false);

                    EditorGUILayout.EndHorizontal();

#if UNITY_5 || UNITY_2017_1_OR_NEWER
                    if (_sounds.showUnityMixerGroupAssignment) {
                        var newChan = (AudioMixerGroup)EditorGUILayout.ObjectField(controller.mixerChannel, typeof(AudioMixerGroup), false);
                        if (newChan != controller.mixerChannel) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref ctrlDirty, controller, "change Playlist Controller Unity Mixer Group");
                            controller.mixerChannel = newChan;

                            if (Application.isPlaying) {
                                controller.RouteToMixerChannel(newChan);
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();
#endif

                    switch (buttonPressed) {
                        case DTGUIHelper.DTFunctionButtons.Go:
                            Selection.activeObject = controller.transform;
                            break;
                        case DTGUIHelper.DTFunctionButtons.Remove:
                            indexToDelete = i;
                            break;
                        case DTGUIHelper.DTFunctionButtons.Mute:
                            controller.ToggleMutePlaylist();
                            ctrlDirty = true;
                            break;
                    }

                    EditorGUILayout.EndVertical();
                    DTGUIHelper.AddSpaceForNonU5(1);

                    if (ctrlDirty) {
                        EditorUtility.SetDirty(controller);
                    }
                }

                if (indexToDelete.HasValue) {
                    AudioUndoHelper.DestroyForUndo(pcs[indexToDelete.Value].gameObject);
                }
            }

            EditorGUILayout.Separator();
            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(2);
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button(new GUIContent("Create Playlist Controller"), EditorStyles.toolbarButton, GUILayout.Width(150))) {
                // ReSharper disable once RedundantCast
                var go = (GameObject)Instantiate(_sounds.playlistControllerPrefab.gameObject);
                go.name = "PlaylistController";

                AudioUndoHelper.CreateObjectForUndo(go, "create Playlist Controller");
            }

            var buttonText = string.Empty;

#if UNITY_5 || UNITY_2017_1_OR_NEWER
            GUILayout.Space(6);
            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            buttonText = "Show Unity Mixer Groups";
            if (_sounds.showUnityMixerGroupAssignment) {
                buttonText = "Hide Unity Mixer Groups";
            }
            if (GUILayout.Button(new GUIContent(buttonText), EditorStyles.toolbarButton, GUILayout.Width(140))) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, buttonText);
                _sounds.showUnityMixerGroupAssignment = !_sounds.showUnityMixerGroupAssignment;
            }
#endif

            EditorGUILayout.EndHorizontal();
            DTGUIHelper.EndGroupHeader();

            GUI.contentColor = Color.white;

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Playlist Setup", EditorStyles.miniBoldLabel, GUILayout.Width(80));
            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/PlaylistSettings.htm#PlaylistSetup", true);
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel = 0;  // Space will handle this for the header

            if (_sounds.musicPlaylists.Count == 0) {
                DTGUIHelper.ShowLargeBarAlert("You currently have no Playlists set up.");
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUI.indentLevel = 1;
            var oldPlayExpanded = DTGUIHelper.Foldout(_sounds.playlistsExpanded, string.Format("Playlists ({0})", _sounds.musicPlaylists.Count));
            if (oldPlayExpanded != _sounds.playlistsExpanded) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Playlists");
                _sounds.playlistsExpanded = oldPlayExpanded;
            }

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));

            var addPressed = false;

            buttonText = "Click to add new Playlist at the end";
            // Add button - Process presses later
            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            addPressed = GUILayout.Button(new GUIContent("Add", buttonText),
                                               EditorStyles.toolbarButton);
            GUIContent content;
            GUI.contentColor = DTGUIHelper.BrightButtonColor;

            content = new GUIContent("Collapse", "Click to collapse all");
            var masterCollapse = GUILayout.Button(content, EditorStyles.toolbarButton);

            content = new GUIContent("Expand", "Click to expand all");
            var masterExpand = GUILayout.Button(content, EditorStyles.toolbarButton);
            if (masterExpand) {
                ExpandCollapseAllPlaylists(true);
            }
            if (masterCollapse) {
                ExpandCollapseAllPlaylists(false);
            }
            GUI.contentColor = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            if (_sounds.playlistsExpanded) {
                int? playlistToRemove = null;
                int? playlistToInsertAt = null;
                int? playlistToMoveUp = null;
                int? playlistToMoveDown = null;

                for (var i = 0; i < _sounds.musicPlaylists.Count; i++) {
                    var aList = _sounds.musicPlaylists[i];

                    DTGUIHelper.StartGroupHeader();

                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.BeginHorizontal();
                    aList.isExpanded = DTGUIHelper.Foldout(aList.isExpanded, "Playlist: " + aList.playlistName);

                    var playlistButtonPressed = DTGUIHelper.AddFoldOutListItemButtonItems(i, _sounds.musicPlaylists.Count, "playlist", false, false, true);

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();

                    if (aList.isExpanded) {
                        DTGUIHelper.StartGroupHeader(1);
                        EditorGUI.indentLevel = 0;
                        var exp = EditorGUILayout.BeginToggleGroup(" Show Song Metadata", aList.showMetadata);
                        if (exp != aList.showMetadata) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle expand Show Song Metadata");
                            aList.showMetadata = exp;
                        }
                        DTGUIHelper.EndGroupHeader();

                        if (aList.showMetadata) {
                            if (!Application.isPlaying) {
                                var newPropName = EditorGUILayout.TextField("Property Name", aList.newMetadataPropName);
                                if (newPropName != aList.newMetadataPropName) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Property Name");
                                    aList.newMetadataPropName = newPropName;
                                }

                                var newPropType = (SongMetadataProperty.MetadataPropertyType)EditorGUILayout.EnumPopup("Property Type", aList.newMetadataPropType);
                                if (newPropType != aList.newMetadataPropType) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Property Name");
                                    aList.newMetadataPropType = newPropType;
                                }

                                var newPropRequired = EditorGUILayout.Toggle("Is Required", aList.newMetadataPropRequired);
                                if (newPropRequired != aList.newMetadataPropRequired) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Is Required");
                                    aList.newMetadataPropRequired = newPropRequired;
                                }

                                var newPropMult = EditorGUILayout.Toggle("Song Can Have Multiple", aList.newMetadataPropCanHaveMult);
                                if (newPropMult != aList.newMetadataPropCanHaveMult) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Song Can Have Multiple");
                                    aList.newMetadataPropCanHaveMult = newPropMult;
                                }

                                GUI.contentColor = DTGUIHelper.BrightButtonColor;
                                if (GUILayout.Button(new GUIContent("Create Metadata Property"), EditorStyles.toolbarButton, GUILayout.Width(140))) {
                                    CreateMetadataProperty(aList, aList.newMetadataPropName, aList.newMetadataPropType, aList.newMetadataPropRequired, aList.newMetadataPropCanHaveMult);
                                }
                            }
                            GUI.contentColor = Color.white;

                            var metaLabelText = "Metadata Properties";
                            if (aList.songMetadataProps.Count > 0) {
                                metaLabelText += "(" + aList.songMetadataProps.Count + ")";
                            }

                            GUILayout.Label(metaLabelText, EditorStyles.boldLabel);

                            if (aList.songMetadataProps.Count == 0) {
                                DTGUIHelper.ShowColorWarning("You have no Metadata. Add some properties above.");
                            }

                            int? propIndexToDelete = null;

                            for (var s = 0; s < aList.songMetadataProps.Count; s++) {
                                var property = aList.songMetadataProps[s];

                                GUI.backgroundColor = Color.white;
                                DTGUIHelper.StartGroupHeader(0, false);

                                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                                if (property.IsEditing) {
                                    GUI.backgroundColor = DTGUIHelper.BrightTextColor;
                                    var propName = EditorGUILayout.TextField("", property.ProspectiveName);
                                    if (propName != property.ProspectiveName) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Property Name");
                                        property.ProspectiveName = propName;
                                    }

                                    var buttonPressed = DTGUIHelper.AddCancelSaveButtons("Song Metadata Property");

                                    switch (buttonPressed) {
                                        case DTGUIHelper.DTFunctionButtons.Cancel:
                                            property.IsEditing = false;
                                            property.ProspectiveName = property.PropertyName;
                                            _isDirty = true;
                                            break;
                                        case DTGUIHelper.DTFunctionButtons.Save:
                                            if (propName != null) {
                                                propName = propName.Replace(" ", "");
                                            }
                                            if (string.IsNullOrEmpty(propName)) {
                                                DTGUIHelper.ShowAlert("You must give a name to your new Meta Property.");
                                                break;
                                            }

                                            var match = aList.songMetadataProps.Find(delegate (SongMetadataProperty p) {
                                                return p.PropertyName == propName;
                                            });

                                            if (match != null) {
                                                DTGUIHelper.ShowAlert("You already have a Metadata Property named '" + propName + "'. Names must be unique.");
                                                break;
                                            }

                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Property Name");

                                            var newName = property.ProspectiveName;
                                            var oldName = property.PropertyName;

                                            // update all songs to have the new Property Name
                                            for (var t = 0; t < aList.MusicSettings.Count; t++) {
                                                var aSong = aList.MusicSettings[t];


                                                switch (property.PropertyType) {
                                                    case SongMetadataProperty.MetadataPropertyType.String:
                                                        var sMatches = aSong.metadataStringValues.FindAll(delegate (SongMetadataStringValue v) {
                                                            return v.PropertyName == oldName;
                                                        });

                                                        for (var loop = 0; loop < sMatches.Count; loop++) {
                                                            sMatches[loop].PropertyName = newName;
                                                        }
                                                        break;
                                                    case SongMetadataProperty.MetadataPropertyType.Boolean:
                                                        var bMatches = aSong.metadataBoolValues.FindAll(delegate (SongMetadataBoolValue v) {
                                                            return v.PropertyName == oldName;
                                                        });

                                                        for (var loop = 0; loop < bMatches.Count; loop++) {
                                                            bMatches[loop].PropertyName = newName;
                                                        }
                                                        break;
                                                    case SongMetadataProperty.MetadataPropertyType.Integer:
                                                        var iMatches = aSong.metadataIntValues.FindAll(delegate (SongMetadataIntValue v) {
                                                            return v.PropertyName == oldName;
                                                        });

                                                        for (var loop = 0; loop < iMatches.Count; loop++) {
                                                            iMatches[loop].PropertyName = newName;
                                                        }
                                                        break;
                                                    case SongMetadataProperty.MetadataPropertyType.Float:
                                                        var fMatches = aSong.metadataFloatValues.FindAll(delegate (SongMetadataFloatValue v) {
                                                            return v.PropertyName == oldName;
                                                        });

                                                        for (var loop = 0; loop < fMatches.Count; loop++) {
                                                            fMatches[loop].PropertyName = newName;
                                                        }
                                                        break;
                                                }
                                            }

                                            property.PropertyName = newName;
                                            property.IsEditing = false;
                                            break;
                                    }

                                } else {
                                    EditorGUI.indentLevel = 1;
                                    var newEditing = EditorGUILayout.Foldout(property.PropertyExpanded, property.PropertyName + " (" + property.PropertyType.ToString() + ")");
                                    if (newEditing != property.PropertyExpanded) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle expand Property");
                                        property.PropertyExpanded = newEditing;
                                    }

                                    GUILayout.FlexibleSpace();
                                    var settingsIcon = new GUIContent(MasterAudioInspectorResources.GearTexture, "Click to edit Property");

                                    if (!Application.isPlaying) {
                                        GUI.backgroundColor = Color.white;
                                        if (GUILayout.Button(settingsIcon, EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(16))) {
                                            property.IsEditing = true;
                                        }
                                        GUI.backgroundColor = DTGUIHelper.DeleteButtonColor;
                                        if (GUILayout.Button(new GUIContent("Delete", "Click to delete Property"), EditorStyles.miniButton, GUILayout.MaxWidth(45))) {
                                            propIndexToDelete = s;
                                        }
                                    }
                                }

                                GUI.backgroundColor = Color.white;

                                EditorGUILayout.EndHorizontal();

                                if (property.PropertyExpanded) {
                                    EditorGUI.indentLevel = 0;

                                    if (Application.isPlaying) {
                                        DTGUIHelper.ShowColorWarning("Can't edit at runtime.");
                                    } else {
                                        var newRequired = EditorGUILayout.Toggle("Is Required", property.AllSongsMustContain);
                                        if (newRequired != property.AllSongsMustContain) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Is Required");
                                            property.AllSongsMustContain = newRequired;
                                            if (newRequired) {
                                                MakePropertyRequired(property, aList);
                                            }
                                        }
                                        var newMult = EditorGUILayout.Toggle("Song Can Have Multiple", property.CanSongHaveMultiple);
                                        if (newMult != property.CanSongHaveMultiple) {
                                            var sbSongsWithMult = new StringBuilder();
                                            if (!newMult) { // turned off the checkbox
                                                for (var x = 0; x < aList.MusicSettings.Count; x++) {
                                                    var aSong = aList.MusicSettings[x];

                                                    switch (property.PropertyType) {
                                                        case SongMetadataProperty.MetadataPropertyType.String:
                                                            if (aSong.metadataStringValues.FindAll(delegate (SongMetadataStringValue v) {
                                                                return v.PropertyName == property.PropertyName;
                                                            }).Count > 1) {
                                                                if (sbSongsWithMult.Length > 0) {
                                                                    sbSongsWithMult.Append(",");
                                                                }
                                                                sbSongsWithMult.Append(aSong.songName);
                                                            }
                                                            break;
                                                        case SongMetadataProperty.MetadataPropertyType.Boolean:
                                                            if (aSong.metadataBoolValues.FindAll(delegate (SongMetadataBoolValue v) {
                                                                return v.PropertyName == property.PropertyName;
                                                            }).Count > 1) {
                                                                if (sbSongsWithMult.Length > 0) {
                                                                    sbSongsWithMult.Append(",");
                                                                }
                                                                sbSongsWithMult.Append(aSong.songName);
                                                            }
                                                            break;
                                                        case SongMetadataProperty.MetadataPropertyType.Integer:
                                                            if (aSong.metadataIntValues.FindAll(delegate (SongMetadataIntValue v) {
                                                                return v.PropertyName == property.PropertyName;
                                                            }).Count > 1) {
                                                                if (sbSongsWithMult.Length > 0) {
                                                                    sbSongsWithMult.Append(",");
                                                                }
                                                                sbSongsWithMult.Append(aSong.songName);
                                                            }
                                                            break;
                                                        case SongMetadataProperty.MetadataPropertyType.Float:
                                                            if (aSong.metadataFloatValues.FindAll(delegate (SongMetadataFloatValue v) {
                                                                return v.PropertyName == property.PropertyName;
                                                            }).Count > 1) {
                                                                if (sbSongsWithMult.Length > 0) {
                                                                    sbSongsWithMult.Append(",");
                                                                }
                                                                sbSongsWithMult.Append(aSong.songName);
                                                            }
                                                            break;
                                                    }
                                                }
                                            }

                                            if (sbSongsWithMult.Length > 0) {
                                                DTGUIHelper.ShowAlert("Cannot turn off 'Song Can Have Multiple' because you have song(s) with multiple values for this property: " + sbSongsWithMult.ToString() + ". We cannot choose which value to keep, so you must make sure there is no more than one before turning off this checkbox.");
                                            } else {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Song Can Have Multiple");
                                                property.CanSongHaveMultiple = newMult;
                                            }
                                        }
                                    }
                                }

                                EditorGUILayout.EndVertical();

                                DTGUIHelper.AddSpaceForNonU5(2);
                            }

                            if (propIndexToDelete.HasValue) {
                                DeleteMetadataProperty(aList, propIndexToDelete.Value);
                            }
                        }
                        EditorGUILayout.EndToggleGroup();
                        GUI.backgroundColor = Color.white;
                        EditorGUI.indentLevel = 0;

                        var newPlaylist = EditorGUILayout.TextField("Name", aList.playlistName);
                        if (newPlaylist != aList.playlistName) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Name");
                            aList.playlistName = newPlaylist;
                        }

                        var crossfadeMode = (MasterAudio.Playlist.CrossfadeTimeMode)EditorGUILayout.EnumPopup("Crossfade Mode", aList.crossfadeMode);
                        if (crossfadeMode != aList.crossfadeMode) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Crossfade Mode");
                            aList.crossfadeMode = crossfadeMode;
                        }
                        if (aList.crossfadeMode == MasterAudio.Playlist.CrossfadeTimeMode.Override) {
                            var newCf = EditorGUILayout.Slider("Crossfade time (sec)", aList.crossFadeTime, 0f, MasterAudio.MaxCrossFadeTimeSeconds);
                            if (newCf != aList.crossFadeTime) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Crossfade time (sec)");
                                aList.crossFadeTime = newCf;
                            }
                        }

                        var newFadeIn = EditorGUILayout.Toggle("Fade In First Song", aList.fadeInFirstSong);
                        if (newFadeIn != aList.fadeInFirstSong) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Fade In First Song");
                            aList.fadeInFirstSong = newFadeIn;
                        }

                        var newFadeOut = EditorGUILayout.Toggle("Fade Out Last Song", aList.fadeOutLastSong);
                        if (newFadeOut != aList.fadeOutLastSong) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Fade Out Last Song");
                            aList.fadeOutLastSong = newFadeOut;
                        }

                        var newBulk = EditorGUILayout.Toggle("Bulk Song Changes", aList.bulkEditMode);
                        if (newBulk != aList.bulkEditMode) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Bulk Song Changes");
                            aList.bulkEditMode = newBulk;
                        }

                        if (aList.bulkEditMode) {
                            GUI.contentColor = DTGUIHelper.BrightButtonColor;
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(0);
                            if (GUILayout.Button("Select All", EditorStyles.toolbarButton, GUILayout.Width(70))) {
                                CheckAllSongs(aList);
                            }
                            GUILayout.Space(6);
                            if (GUILayout.Button("Deselect All", EditorStyles.toolbarButton, GUILayout.Width(80))) {
                                UncheckAllSongs(aList);
                            }
                            EditorGUILayout.EndHorizontal();

                            GUI.contentColor = Color.white;
                            var totalSongs = aList.MusicSettings.Count;
                            var numSelected = GetNumCheckedSongs(aList);
                            DTGUIHelper.ShowLargeBarAlert(numSelected + " of " + totalSongs + " Songs selected - adjustments to a selected Song will affect all selected Songs.");
                        }

                        var newTransType = (MasterAudio.SongFadeInPosition)EditorGUILayout.EnumPopup("Song Transition Type", aList.songTransitionType);
                        if (newTransType != aList.songTransitionType) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Song Transition Type");
                            aList.songTransitionType = newTransType;
                        }
                        if (aList.songTransitionType == MasterAudio.SongFadeInPosition.SynchronizeClips) {
                            DTGUIHelper.ShowColorWarning("All clips must be of exactly the same length in this mode.");
                        }

                        EditorGUI.indentLevel = 0;
                        var newBulkMode = DTGUIHelper.GetRestrictedAudioLocation("Clip Create Mode", aList.bulkLocationMode);
                        if (newBulkMode != aList.bulkLocationMode) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Bulk Clip Mode");
                            aList.bulkLocationMode = newBulkMode;
                        }

                        var playlistHasResource = false;
                        foreach (var t in aList.MusicSettings) {
                            if (t.audLocation != MasterAudio.AudioLocation.ResourceFile) {
                                continue;
                            }

                            playlistHasResource = true;
                            break;
                        }

                        if (MasterAudio.HasAsyncResourceLoaderFeature() && playlistHasResource) {
                            if (!_sounds.resourceClipsAllLoadAsync) {
                                var newAsync = EditorGUILayout.Toggle(new GUIContent("Load Resources Async", "Checking this means Resource files in this Playlist will be loaded asynchronously."), aList.resourceClipsAllLoadAsync);
                                if (newAsync != aList.resourceClipsAllLoadAsync) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Load Resources Async");
                                    aList.resourceClipsAllLoadAsync = newAsync;
                                }
                            }
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(4);
                        GUI.contentColor = DTGUIHelper.BrightButtonColor;
                        if (GUILayout.Button(new GUIContent("Eq. Song Volumes"), EditorStyles.toolbarButton, GUILayout.Width(110))) {
                            EqualizePlaylistVolumes(aList.MusicSettings);
                        }

                        var hasExpanded = false;
                        foreach (var t in aList.MusicSettings) {
                            if (!t.isExpanded) {
                                continue;
                            }

                            hasExpanded = true;
                            break;
                        }

                        var theButtonText = hasExpanded ? "Collapse All" : "Expand All";

                        GUILayout.Space(10);
                        GUI.contentColor = DTGUIHelper.BrightButtonColor;
                        if (GUILayout.Button(new GUIContent(theButtonText), EditorStyles.toolbarButton, GUILayout.Width(76))) {
                            ExpandCollapseSongs(aList, !hasExpanded);
                        }
                        GUILayout.Space(10);
                        if (GUILayout.Button(new GUIContent("Sort Alpha"), EditorStyles.toolbarButton, GUILayout.Width(70))) {
                            SortSongsAlpha(aList);
                        }

                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        GUI.contentColor = Color.white;
                        EditorGUILayout.Separator();

                        EditorGUILayout.BeginVertical();
                        var anEvent = Event.current;

                        GUI.color = DTGUIHelper.DragAreaColor;

                        var dragArea = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
                        GUI.Box(dragArea, MasterAudio.DragAudioTip + " to add to playlist!");

                        GUI.color = Color.white;

                        switch (anEvent.type) {
                            case EventType.DragUpdated:
                            case EventType.DragPerform:
                                if (!dragArea.Contains(anEvent.mousePosition)) {
                                    break;
                                }

                                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                                if (anEvent.type == EventType.DragPerform) {
                                    DragAndDrop.AcceptDrag();

                                    foreach (var dragged in DragAndDrop.objectReferences) {
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_2017_1_OR_NEWER
                                        if (dragged is DefaultAsset) {
                                            var assetPaths = AssetDatabase.FindAssets("t:AudioClip", DragAndDrop.paths);
                                            foreach (var assetPath in assetPaths) {
                                                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(assetPath));
                                                if (clip == null) {
                                                    continue;
                                                }

                                                AddSongToPlaylist(aList, clip);
                                            }

                                            continue;
                                        }
#endif

                                        var aClip = dragged as AudioClip;
                                        if (aClip == null) {
                                            continue;
                                        }

                                        AddSongToPlaylist(aList, aClip);
                                    }
                                }
                                Event.current.Use();
                                break;
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUI.indentLevel = 2;

                        int? addIndex = null;
                        int? removeIndex = null;
                        int? moveUpIndex = null;
                        int? moveDownIndex = null;
                        int? indexToClone = null;

                        if (aList.MusicSettings.Count == 0) {
                            EditorGUI.indentLevel = 0;
                            DTGUIHelper.ShowLargeBarAlert("You currently have no songs in this Playlist.");
                        }

                        EditorGUI.indentLevel = 0;
                        for (var j = 0; j < aList.MusicSettings.Count; j++) {
                            DTGUIHelper.StartGroupHeader(1);

                            var aSong = aList.MusicSettings[j];
                            var clipName = "Empty";
                            switch (aSong.audLocation) {
                                case MasterAudio.AudioLocation.Clip:
                                    if (aSong.clip != null) {
                                        clipName = aSong.clip.name;
                                    }
                                    break;
                                case MasterAudio.AudioLocation.ResourceFile:
                                    if (!string.IsNullOrEmpty(aSong.resourceFileName)) {
                                        clipName = aSong.resourceFileName;
                                    }
                                    break;
                            }
                            EditorGUILayout.BeginHorizontal();
                            EditorGUI.indentLevel = 1;

                            aSong.songName = aSong.alias;
                            if (!string.IsNullOrEmpty(clipName) && string.IsNullOrEmpty(aSong.songName)) {
                                switch (aSong.audLocation) {
                                    case MasterAudio.AudioLocation.Clip:
                                        aSong.songName = clipName;
                                        break;
                                    case MasterAudio.AudioLocation.ResourceFile:
                                        aSong.songName = clipName;
                                        break;
                                }
                            }

                            var newSongExpanded = DTGUIHelper.Foldout(aSong.isExpanded, aSong.songName);
                            if (newSongExpanded != aSong.isExpanded) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Song expand");
                                aSong.isExpanded = newSongExpanded;
                            }

                            var songButtonPressed = DTGUIHelper.AddFoldOutListItemButtonItems(j, aList.MusicSettings.Count, "clip", false, true, true, allowPreview, aList.bulkEditMode, aSong.isChecked);
                            switch (songButtonPressed) {
                                case DTGUIHelper.DTFunctionButtons.Check:
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Song check");
                                    aSong.isChecked = true;
                                    break;
                                case DTGUIHelper.DTFunctionButtons.Uncheck:
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Song check");
                                    aSong.isChecked = false;
                                    break;
                            }

                            GUILayout.Space(4);
                            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/PlaylistSettings.htm#Song");
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();

                            if (aSong.isExpanded) {
                                EditorGUI.indentLevel = 0;

                                if (aList.showMetadata) {
                                    var oldBG = GUI.backgroundColor;

                                    DTGUIHelper.StartGroupHeader(0, false);

                                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                                    EditorGUI.indentLevel = 1;
                                    var metaLabelText = "Song Metadata";

                                    if (aSong.HasMetadataProperties) {
                                        metaLabelText += " (" + aSong.MetadataPropertyCount + ")";
                                    }

                                    var newExp = EditorGUILayout.Foldout(aSong.metadataExpanded, metaLabelText);
                                    if (newExp != aSong.metadataExpanded) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle expand Song Metadata");
                                        aSong.metadataExpanded = newExp;
                                    }

                                    EditorGUILayout.EndHorizontal();

                                    if (aSong.metadataExpanded) {
                                        EditorGUI.indentLevel = 0;

                                        var propertiesToAdd = new List<string>();
                                        propertiesToAdd.Add("[Select a Property]");
                                        for (var p = 0; p < aList.songMetadataProps.Count; p++) {
                                            var aProp = aList.songMetadataProps[p];
                                            if (aProp.CanSongHaveMultiple) {
                                                propertiesToAdd.Add(aProp.PropertyName);
                                                continue;
                                            }

                                            if (aProp.AllSongsMustContain) {
                                                continue;
                                            }

                                            // not required. If you don't have one, you can add.
                                            switch (aProp.PropertyType) {
                                                case SongMetadataProperty.MetadataPropertyType.Boolean:
                                                    if (aSong.metadataBoolValues.FindAll(delegate (SongMetadataBoolValue v) {
                                                        return v.PropertyName == aProp.PropertyName;
                                                    }).Count == 0) {
                                                        propertiesToAdd.Add(aProp.PropertyName);
                                                    }
                                                    break;
                                                case SongMetadataProperty.MetadataPropertyType.Integer:
                                                    if (aSong.metadataIntValues.FindAll(delegate (SongMetadataIntValue v) {
                                                        return v.PropertyName == aProp.PropertyName;
                                                    }).Count == 0) {
                                                        propertiesToAdd.Add(aProp.PropertyName);
                                                    }
                                                    break;
                                                case SongMetadataProperty.MetadataPropertyType.Float:
                                                    if (aSong.metadataFloatValues.FindAll(delegate (SongMetadataFloatValue v) {
                                                        return v.PropertyName == aProp.PropertyName;
                                                    }).Count == 0) {
                                                        propertiesToAdd.Add(aProp.PropertyName);
                                                    }
                                                    break;
                                                case SongMetadataProperty.MetadataPropertyType.String:
                                                    if (aSong.metadataStringValues.FindAll(delegate (SongMetadataStringValue v) {
                                                        return v.PropertyName == aProp.PropertyName;
                                                    }).Count == 0) {
                                                        propertiesToAdd.Add(aProp.PropertyName);
                                                    }
                                                    break;
                                            }
                                        }

                                        var oldBG2 = GUI.backgroundColor;

                                        if (!Application.isPlaying) {
                                            var propIndex = EditorGUILayout.Popup("Add Metadata Property", 0, propertiesToAdd.ToArray());
                                            if (propIndex != 0) {
                                                var propName = propertiesToAdd[propIndex];
                                                var propToAdd = aList.songMetadataProps.Find(delegate (SongMetadataProperty p) {
                                                    return p.PropertyName == propName;
                                                });

                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "add Property");

                                                switch (propToAdd.PropertyType) {
                                                    case SongMetadataProperty.MetadataPropertyType.Boolean:
                                                        aSong.metadataBoolValues.Add(new SongMetadataBoolValue(propToAdd));
                                                        break;
                                                    case SongMetadataProperty.MetadataPropertyType.String:
                                                        aSong.metadataStringValues.Add(new SongMetadataStringValue(propToAdd));
                                                        break;
                                                    case SongMetadataProperty.MetadataPropertyType.Integer:
                                                        aSong.metadataIntValues.Add(new SongMetadataIntValue(propToAdd));
                                                        break;
                                                    case SongMetadataProperty.MetadataPropertyType.Float:
                                                        aSong.metadataFloatValues.Add(new SongMetadataFloatValue(propToAdd));
                                                        break;
                                                }
                                            }
                                        }

                                        if (!aSong.HasMetadataProperties) {
                                            DTGUIHelper.ShowColorWarning("This song has no metadata.");
                                        }

                                        int? sIndexToDelete = null;
                                        for (var m = 0; m < aSong.metadataStringValues.Count; m++) {
                                            GUI.backgroundColor = oldBG2;
                                            var aVal = aSong.metadataStringValues[m];

                                            EditorGUILayout.BeginHorizontal();

                                            var newVal = EditorGUILayout.TextField(aVal.PropertyName, aVal.Value);
                                            if (newVal != aVal.Value) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Property value");
                                                aVal.Value = newVal;
                                            }

                                            GUI.backgroundColor = DTGUIHelper.DeleteButtonColor;
                                            if (GUILayout.Button(new GUIContent("Delete", "Click to delete Property"), EditorStyles.miniButton, GUILayout.MaxWidth(45))) {
                                                sIndexToDelete = m;
                                            }

                                            EditorGUILayout.EndHorizontal();
                                        }

                                        GUI.backgroundColor = oldBG2;
                                        int? bIndexToDelete = null;
                                        for (var m = 0; m < aSong.metadataBoolValues.Count; m++) {
                                            var aVal = aSong.metadataBoolValues[m];
                                            EditorGUILayout.BeginHorizontal();

                                            var newVal = EditorGUILayout.Toggle(aVal.PropertyName, aVal.Value);
                                            if (newVal != aVal.Value) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Property value");
                                                aVal.Value = newVal;
                                            }

                                            GUI.backgroundColor = DTGUIHelper.DeleteButtonColor;
                                            if (GUILayout.Button(new GUIContent("Delete", "Click to delete Property"), EditorStyles.miniButton, GUILayout.MaxWidth(45))) {
                                                bIndexToDelete = m;
                                            }

                                            EditorGUILayout.EndHorizontal();
                                        }

                                        GUI.backgroundColor = oldBG2;
                                        int? iIndexToDelete = null;
                                        for (var m = 0; m < aSong.metadataIntValues.Count; m++) {
                                            GUI.backgroundColor = oldBG2;
                                            var aVal = aSong.metadataIntValues[m];
                                            EditorGUILayout.BeginHorizontal();

                                            var newVal = EditorGUILayout.IntField(aVal.PropertyName, aVal.Value);
                                            if (newVal != aVal.Value) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Property value");
                                                aVal.Value = newVal;
                                            }

                                            GUI.backgroundColor = DTGUIHelper.DeleteButtonColor;
                                            if (GUILayout.Button(new GUIContent("Delete", "Click to delete Property"), EditorStyles.miniButton, GUILayout.MaxWidth(45))) {
                                                iIndexToDelete = m;
                                            }

                                            EditorGUILayout.EndHorizontal();
                                        }

                                        GUI.backgroundColor = oldBG2;
                                        int? fIndexToDelete = null;
                                        for (var m = 0; m < aSong.metadataFloatValues.Count; m++) {
                                            GUI.backgroundColor = oldBG2;
                                            var aVal = aSong.metadataFloatValues[m];
                                            EditorGUILayout.BeginHorizontal();

                                            var newVal = EditorGUILayout.FloatField(aVal.PropertyName, aVal.Value);
                                            if (newVal != aVal.Value) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Property value");
                                                aVal.Value = newVal;
                                            }

                                            GUI.backgroundColor = DTGUIHelper.DeleteButtonColor;
                                            if (GUILayout.Button(new GUIContent("Delete", "Click to delete Property"), EditorStyles.miniButton, GUILayout.MaxWidth(45))) {
                                                fIndexToDelete = m;
                                            }

                                            EditorGUILayout.EndHorizontal();
                                        }

                                        if (sIndexToDelete.HasValue) {
                                            var deadProp = aSong.metadataStringValues[sIndexToDelete.Value];
                                            var srcProp = aList.songMetadataProps.Find(delegate (SongMetadataProperty p) {
                                                return p.PropertyName == deadProp.PropertyName;
                                            });
                                            if (srcProp == null || !srcProp.AllSongsMustContain) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "delete Property");
                                                aSong.metadataStringValues.RemoveAt(sIndexToDelete.Value);
                                            } else {
                                                var propMatches = aSong.metadataStringValues.FindAll(delegate (SongMetadataStringValue s) {
                                                    return s.PropertyName == deadProp.PropertyName;
                                                });
                                                if (propMatches.Count <= 1) {
                                                    DTGUIHelper.ShowAlert("This Property is required. You cannot delete all instances of it from a song.");
                                                } else {
                                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "delete Property");
                                                    aSong.metadataStringValues.RemoveAt(sIndexToDelete.Value);
                                                }
                                            }
                                        }
                                        if (bIndexToDelete.HasValue) {
                                            var deadProp = aSong.metadataBoolValues[bIndexToDelete.Value];
                                            var srcProp = aList.songMetadataProps.Find(delegate (SongMetadataProperty p) {
                                                return p.PropertyName == deadProp.PropertyName;
                                            });
                                            if (srcProp == null || !srcProp.AllSongsMustContain) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "delete Property");
                                                aSong.metadataBoolValues.RemoveAt(bIndexToDelete.Value);
                                            } else {
                                                var propMatches = aSong.metadataBoolValues.FindAll(delegate (SongMetadataBoolValue s) {
                                                    return s.PropertyName == deadProp.PropertyName;
                                                });
                                                if (propMatches.Count <= 1) {
                                                    DTGUIHelper.ShowAlert("This Property is required. You cannot delete all instances of it from a song.");
                                                } else {
                                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "delete Property");
                                                    aSong.metadataBoolValues.RemoveAt(bIndexToDelete.Value);
                                                }
                                            }
                                        }
                                        if (iIndexToDelete.HasValue) {
                                            var deadProp = aSong.metadataIntValues[iIndexToDelete.Value];
                                            var srcProp = aList.songMetadataProps.Find(delegate (SongMetadataProperty p) {
                                                return p.PropertyName == deadProp.PropertyName;
                                            });
                                            if (srcProp == null || !srcProp.AllSongsMustContain) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "delete Property");
                                                aSong.metadataIntValues.RemoveAt(iIndexToDelete.Value);
                                            } else {
                                                var propMatches = aSong.metadataIntValues.FindAll(delegate (SongMetadataIntValue s) {
                                                    return s.PropertyName == deadProp.PropertyName;
                                                });
                                                if (propMatches.Count <= 1) {
                                                    DTGUIHelper.ShowAlert("This Property is required. You cannot delete all instances of it from a song.");
                                                } else {
                                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "delete Property");
                                                    aSong.metadataIntValues.RemoveAt(iIndexToDelete.Value);
                                                }
                                            }
                                        }
                                        if (fIndexToDelete.HasValue) {
                                            var deadProp = aSong.metadataFloatValues[fIndexToDelete.Value];
                                            var srcProp = aList.songMetadataProps.Find(delegate (SongMetadataProperty p) {
                                                return p.PropertyName == deadProp.PropertyName;
                                            });
                                            if (srcProp == null || !srcProp.AllSongsMustContain) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "delete Property");
                                                aSong.metadataFloatValues.RemoveAt(fIndexToDelete.Value);
                                            } else {
                                                var propMatches = aSong.metadataFloatValues.FindAll(delegate (SongMetadataFloatValue s) {
                                                    return s.PropertyName == deadProp.PropertyName;
                                                });
                                                if (propMatches.Count <= 1) {
                                                    DTGUIHelper.ShowAlert("This Property is required. You cannot delete all instances of it from a song.");
                                                } else {
                                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "delete Property");
                                                    aSong.metadataFloatValues.RemoveAt(fIndexToDelete.Value);
                                                }
                                            }
                                        }
                                    }

                                    EditorGUILayout.EndVertical();
                                    GUI.backgroundColor = oldBG;
                                }

                                EditorGUI.indentLevel = 0;
                                var newName = EditorGUILayout.TextField(new GUIContent("Song Id (optional)", "When you 'Play song by name', Song Id's will be searched first before audio file name."), aSong.alias);
                                if (newName != aSong.alias) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Song Id");
                                    aSong.alias = newName;
                                }

                                var oldLocation = aSong.audLocation;
                                var newClipSource = DTGUIHelper.GetRestrictedAudioLocation("Audio Origin", aSong.audLocation);
                                if (newClipSource != aSong.audLocation) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Audio Origin");
                                    aSong.audLocation = newClipSource;
                                }

                                switch (aSong.audLocation) {
                                    case MasterAudio.AudioLocation.Clip:
                                        var newClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", aSong.clip, typeof(AudioClip), true);
                                        if (newClip != aSong.clip) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Clip");
                                            aSong.clip = newClip;
                                            var cName = newClip == null ? "Empty" : newClip.name;
                                            aSong.songName = cName;
                                        }
                                        break;
                                    case MasterAudio.AudioLocation.ResourceFile:
                                        if (oldLocation != aSong.audLocation) {
                                            if (aSong.clip != null) {
                                                Debug.Log("Audio clip removed to prevent unnecessary memory usage on Resource file Playlist clip.");
                                            }
                                            aSong.clip = null;
                                            aSong.songName = string.Empty;
                                        }

                                        EditorGUILayout.BeginVertical();
                                        anEvent = Event.current;

                                        GUI.color = DTGUIHelper.DragAreaColor;
                                        dragArea = GUILayoutUtility.GetRect(0f, 20f, GUILayout.ExpandWidth(true));
                                        GUI.Box(dragArea, "Drag Resource Audio clip here to use its name!");
                                        GUI.color = Color.white;

                                        switch (anEvent.type) {
                                            case EventType.DragUpdated:
                                            case EventType.DragPerform:
                                                if (!dragArea.Contains(anEvent.mousePosition)) {
                                                    break;
                                                }

                                                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                                                if (anEvent.type == EventType.DragPerform) {
                                                    DragAndDrop.AcceptDrag();

                                                    foreach (var dragged in DragAndDrop.objectReferences) {
                                                        var aClip = dragged as AudioClip;
                                                        if (aClip == null) {
                                                            continue;
                                                        }

                                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Resource Filename");

                                                        var unused = false;
                                                        var resourceFileName = DTGUIHelper.GetResourcePath(aClip, ref unused, true);
                                                        if (string.IsNullOrEmpty(resourceFileName)) {
                                                            resourceFileName = aClip.name;
                                                        }

                                                        aSong.resourceFileName = resourceFileName;
                                                        aSong.songName = aClip.name;
                                                    }
                                                }
                                                Event.current.Use();
                                                break;
                                        }
                                        EditorGUILayout.EndVertical();

                                        var newFilename = EditorGUILayout.TextField("Resource Filename", aSong.resourceFileName);
                                        if (newFilename != aSong.resourceFileName) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Resource Filename");
                                            aSong.resourceFileName = newFilename;
                                        }

                                        break;
                                }

                                var newVol = DTGUIHelper.DisplayVolumeField(aSong.volume, DTGUIHelper.VolumeFieldType.None, _sounds.MixerWidth, 0f, true);
                                if (newVol != aSong.volume) {
                                    if (aList.bulkEditMode && aSong.isChecked) {
                                        CopySongVolumes(aList, newVol);
                                    } else {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Volume");
                                        aSong.volume = newVol;
                                    }
                                }

                                var newPitch = DTGUIHelper.DisplayPitchField(aSong.pitch);
                                if (newPitch != aSong.pitch) {
                                    if (aList.bulkEditMode && aSong.isChecked) {
                                        CopySongPitches(aList, newPitch);
                                    } else {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Pitch");
                                        aSong.pitch = newPitch;
                                    }
                                }

                                var crossFadetime = MasterAudio.Instance.MasterCrossFadeTime;
                                if (aList.crossfadeMode == MasterAudio.Playlist.CrossfadeTimeMode.Override) {
                                    crossFadetime = aList.crossFadeTime;
                                }

                                if (aList.songTransitionType == MasterAudio.SongFadeInPosition.SynchronizeClips && crossFadetime > 0) {
                                    DTGUIHelper.ShowLargeBarAlert("All songs must loop in Synchronized Playlists when crossfade time is not zero. Auto-advance is disabled.");
                                } else {
                                    var newLoop = EditorGUILayout.Toggle("Loop Clip", aSong.isLoop);
                                    if (newLoop != aSong.isLoop) {
                                        if (aList.bulkEditMode && aSong.isChecked) {
                                            CopySongLoops(aList, newLoop);
                                        } else {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Loop Clip");
                                            aSong.isLoop = newLoop;
                                        }
                                    }
                                }

                                var useLastKnownPosition = aList.songTransitionType == MasterAudio.SongFadeInPosition.NewClipFromLastKnownPosition;

                                if (aList.songTransitionType == MasterAudio.SongFadeInPosition.NewClipFromBeginning || useLastKnownPosition) {
                                    if (useLastKnownPosition && aSong.songStartTimeMode != MasterAudio.CustomSongStartTimeMode.Beginning) {
                                        DTGUIHelper.ShowLargeBarAlert("In this Song Transition Type, the Song Start Time Mode will only be used the first time a song is played.");
                                    }

                                    var startTimeMode = (MasterAudio.CustomSongStartTimeMode)EditorGUILayout.EnumPopup("Song Start Time Mode", aSong.songStartTimeMode);
                                    if (startTimeMode != aSong.songStartTimeMode) {
                                        if (aList.bulkEditMode && aSong.isChecked) {
                                            CopySongStartTimeMode(aList, startTimeMode);
                                        } else {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Song Start Time Mode");
                                            aSong.songStartTimeMode = startTimeMode;
                                        }
                                    }

                                    switch (aSong.songStartTimeMode) {
                                        case MasterAudio.CustomSongStartTimeMode.SpecificTime:
                                            var newStart = EditorGUILayout.FloatField("Start Time (seconds)", aSong.customStartTime, GUILayout.Width(300));
                                            if (newStart < 0) {
                                                newStart = 0f;
                                            }
                                            if (newStart != aSong.customStartTime) {
                                                if (aList.bulkEditMode && aSong.isChecked) {
                                                    CopySongCustomStartTime(aList, newStart);
                                                } else {
                                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Start Time (seconds)");
                                                    aSong.customStartTime = newStart;
                                                }
                                            }
                                            break;
                                        case MasterAudio.CustomSongStartTimeMode.RandomTime:
                                            var newStartMin = EditorGUILayout.FloatField("Start Time Min (seconds)", aSong.customStartTime, GUILayout.Width(300));
                                            if (newStartMin < 0) {
                                                newStartMin = 0f;
                                            } else if (newStartMin > aSong.customStartTimeMax) {
                                                newStartMin = aSong.customStartTimeMax;
                                            }
                                            if (newStartMin != aSong.customStartTime) {
                                                if (aList.bulkEditMode && aSong.isChecked) {
                                                    CopySongStartTimeMin(aList, newStartMin);
                                                } else {
                                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Start Time Min (seconds)");
                                                    aSong.customStartTime = newStartMin;
                                                }
                                            }

                                            var newMaxStart = EditorGUILayout.FloatField("Start Time Max (seconds)", aSong.customStartTimeMax, GUILayout.Width(300));
                                            if (newMaxStart < 0) {
                                                newMaxStart = 0f;
                                            } else if (newMaxStart < aSong.customStartTime) {
                                                newMaxStart = aSong.customStartTime;
                                            }
                                            if (newMaxStart != aSong.customStartTimeMax) {
                                                if (aList.bulkEditMode && aSong.isChecked) {
                                                    CopySongStartTimeMax(aList, newMaxStart);
                                                } else {
                                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Start Time Max (seconds)");
                                                    aSong.customStartTimeMax = newMaxStart;
                                                }
                                            }
                                            break;
                                    }
                                }

                                GUI.color = Color.white;
                                exp = EditorGUILayout.BeginToggleGroup(" Fire 'Song Started' Event", aSong.songStartedEventExpanded);
                                if (exp != aSong.songStartedEventExpanded) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle expand Fire 'Song Started' Event");
                                    aSong.songStartedEventExpanded = exp;
                                }
                                GUI.color = Color.white;

                                if (aSong.songStartedEventExpanded) {
                                    EditorGUI.indentLevel = 1;
                                    DTGUIHelper.ShowColorWarning("When song starts, fire Custom Event below.");

                                    var existingIndex = _sounds.CustomEventNames.IndexOf(aSong.songStartedCustomEvent);

                                    int? customEventIndex = null;

                                    var noEvent = false;
                                    var noMatch = false;

                                    if (existingIndex >= 1) {
                                        customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _sounds.CustomEventNames.ToArray());
                                        if (existingIndex == 1) {
                                            noEvent = true;
                                        }
                                    } else if (existingIndex == -1 && aSong.songStartedCustomEvent == MasterAudio.NoGroupName) {
                                        customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _sounds.CustomEventNames.ToArray());
                                    } else { // non-match
                                        noMatch = true;
                                        var newEventName = EditorGUILayout.TextField("Custom Event Name", aSong.songStartedCustomEvent);
                                        if (newEventName != aSong.songStartedCustomEvent) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Custom Event Name");
                                            aSong.songStartedCustomEvent = newEventName;
                                        }

                                        var newIndex = EditorGUILayout.Popup("All Custom Events", -1, _sounds.CustomEventNames.ToArray());
                                        if (newIndex >= 0) {
                                            customEventIndex = newIndex;
                                        }
                                    }

                                    if (noEvent) {
                                        DTGUIHelper.ShowRedError("No Custom Event specified. This section will do nothing.");
                                    } else if (noMatch) {
                                        DTGUIHelper.ShowRedError("Custom Event found no match. Type in or choose one.");
                                    }

                                    if (customEventIndex.HasValue) {
                                        if (existingIndex != customEventIndex.Value) {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Custom Event");
                                        }
                                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                        if (customEventIndex.Value == -1) {
                                            aSong.songStartedCustomEvent = MasterAudio.NoGroupName;
                                        } else {
                                            aSong.songStartedCustomEvent = _sounds.CustomEventNames[customEventIndex.Value];
                                        }
                                    }
                                }
                                EditorGUILayout.EndToggleGroup();

                                EditorGUI.indentLevel = 0;

                                if (_sounds.useGaplessPlaylists) {
                                    DTGUIHelper.ShowLargeBarAlert("Song Changed Event cannot be used with gapless transitions.");
                                } else {
                                    exp = EditorGUILayout.BeginToggleGroup(" Fire 'Song Changed' Event", aSong.songChangedEventExpanded);
                                    if (exp != aSong.songChangedEventExpanded) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle expand Fire 'Song Changed' Event");
                                        aSong.songChangedEventExpanded = exp;
                                    }
                                    GUI.color = Color.white;

                                    if (aSong.songChangedEventExpanded) {
                                        EditorGUI.indentLevel = 1;
                                        DTGUIHelper.ShowColorWarning("When song changes to another, fire Custom Event below.");

                                        var existingIndex = _sounds.CustomEventNames.IndexOf(aSong.songChangedCustomEvent);

                                        int? customEventIndex = null;

                                        var noEvent = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1) {
                                            customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _sounds.CustomEventNames.ToArray());
                                            if (existingIndex == 1) {
                                                noEvent = true;
                                            }
                                        } else if (existingIndex == -1 && aSong.songChangedCustomEvent == MasterAudio.NoGroupName) {
                                            customEventIndex = EditorGUILayout.Popup("Custom Event Name", existingIndex, _sounds.CustomEventNames.ToArray());
                                        } else { // non-match
                                            noMatch = true;
                                            var newEventName = EditorGUILayout.TextField("Custom Event Name", aSong.songChangedCustomEvent);
                                            if (newEventName != aSong.songChangedCustomEvent) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Custom Event Name");
                                                aSong.songChangedCustomEvent = newEventName;
                                            }

                                            var newIndex = EditorGUILayout.Popup("All Custom Events", -1, _sounds.CustomEventNames.ToArray());
                                            if (newIndex >= 0) {
                                                customEventIndex = newIndex;
                                            }
                                        }

                                        if (noEvent) {
                                            DTGUIHelper.ShowRedError("No Custom Event specified. This section will do nothing.");
                                        } else if (noMatch) {
                                            DTGUIHelper.ShowRedError("Custom Event found no match. Type in or choose one.");
                                        }

                                        if (customEventIndex.HasValue) {
                                            if (existingIndex != customEventIndex.Value) {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Custom Event");
                                            }
                                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                            if (customEventIndex.Value == -1) {
                                                aSong.songChangedCustomEvent = MasterAudio.NoGroupName;
                                            } else {
                                                aSong.songChangedCustomEvent = _sounds.CustomEventNames[customEventIndex.Value];
                                            }
                                        }
                                    }
                                    EditorGUILayout.EndToggleGroup();
                                }
                            }

                            switch (songButtonPressed) {
                                case DTGUIHelper.DTFunctionButtons.Add:
                                    addIndex = j;
                                    break;
                                case DTGUIHelper.DTFunctionButtons.Remove:
                                    removeIndex = j;
                                    break;
                                case DTGUIHelper.DTFunctionButtons.Clone:
                                    indexToClone = j;
                                    break;
                                case DTGUIHelper.DTFunctionButtons.ShiftUp:
                                    moveUpIndex = j;
                                    break;
                                case DTGUIHelper.DTFunctionButtons.ShiftDown:
                                    moveDownIndex = j;
                                    break;
                                case DTGUIHelper.DTFunctionButtons.Play:
                                    previewer = GetPreviewer();
                                    StopPreviewer();

                                    switch (aSong.audLocation) {
                                        case MasterAudio.AudioLocation.Clip:
                                            if (previewer != null) {
                                                previewer.PlayOneShot(aSong.clip, aSong.volume);
                                            }

                                            break;
                                        case MasterAudio.AudioLocation.ResourceFile:
                                            if (previewer != null) {
                                                previewer.PlayOneShot(Resources.Load(aSong.resourceFileName) as AudioClip, aSong.volume);
                                            }
                                            break;
                                    }
                                    break;
                                case DTGUIHelper.DTFunctionButtons.Stop:
                                    StopPreviewer();
                                    break;
                            }
                            EditorGUILayout.EndVertical();
                            DTGUIHelper.AddSpaceForNonU5(2);
                        }

                        if (addIndex.HasValue) {
                            var mus = new MusicSetting();
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "add song");
                            aList.MusicSettings.Insert(addIndex.Value + 1, mus);
                        } else if (removeIndex.HasValue) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "delete song");
                            aList.MusicSettings.RemoveAt(removeIndex.Value);
                        } else if (moveUpIndex.HasValue) {
                            var item = aList.MusicSettings[moveUpIndex.Value];

                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "shift up song");

                            aList.MusicSettings.Insert(moveUpIndex.Value - 1, item);
                            aList.MusicSettings.RemoveAt(moveUpIndex.Value + 1);
                        } else if (moveDownIndex.HasValue) {
                            var index = moveDownIndex.Value + 1;
                            var item = aList.MusicSettings[index];

                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "shift down song");

                            aList.MusicSettings.Insert(index - 1, item);
                            aList.MusicSettings.RemoveAt(index + 1);
                        } else if (indexToClone.HasValue) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "clone song");
                            aList.MusicSettings.Insert(indexToClone.Value, MusicSetting.Clone(aList.MusicSettings[indexToClone.Value], aList));
                        }
                    }

                    switch (playlistButtonPressed) {
                        case DTGUIHelper.DTFunctionButtons.Remove:
                            playlistToRemove = i;
                            break;
                        case DTGUIHelper.DTFunctionButtons.Add:
                            playlistToInsertAt = i;
                            break;
                        case DTGUIHelper.DTFunctionButtons.ShiftUp:
                            playlistToMoveUp = i;
                            break;
                        case DTGUIHelper.DTFunctionButtons.ShiftDown:
                            playlistToMoveDown = i;
                            break;
                    }

                    EditorGUILayout.EndVertical();
                    DTGUIHelper.AddSpaceForNonU5(4);
                }

                if (playlistToRemove.HasValue) {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "delete Playlist");
                    _sounds.musicPlaylists.RemoveAt(playlistToRemove.Value);
                }
                if (playlistToInsertAt.HasValue) {
                    var pl = new MasterAudio.Playlist();
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "add Playlist");
                    _sounds.musicPlaylists.Insert(playlistToInsertAt.Value + 1, pl);
                }
                if (playlistToMoveUp.HasValue) {
                    var item = _sounds.musicPlaylists[playlistToMoveUp.Value];
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "shift up Playlist");
                    _sounds.musicPlaylists.Insert(playlistToMoveUp.Value - 1, item);
                    _sounds.musicPlaylists.RemoveAt(playlistToMoveUp.Value + 1);
                }
                if (playlistToMoveDown.HasValue) {
                    var index = playlistToMoveDown.Value + 1;
                    var item = _sounds.musicPlaylists[index];

                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "shift down Playlist");

                    _sounds.musicPlaylists.Insert(index - 1, item);
                    _sounds.musicPlaylists.RemoveAt(index + 1);
                }
            }

            if (addPressed) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "add Playlist");
                _sounds.musicPlaylists.Add(new MasterAudio.Playlist());
            }

            DTGUIHelper.EndGroupedControls();
        }
        // Music playlist End

        // Custom Events Start
        EditorGUI.indentLevel = 0;
        DTGUIHelper.VerticalSpace(3);

        DTGUIHelper.ResetColors();

        state = _sounds.showCustomEvents;
        text = "Custom Events";

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!state) {
            GUI.backgroundColor = DTGUIHelper.InactiveHeaderColor;
        } else {
            GUI.backgroundColor = DTGUIHelper.ActiveHeaderColor;
        }

        GUILayout.BeginHorizontal();

        text = "<b><size=11>" + text + "</size></b>";

        if (state) {
            text = "\u25BC " + text;
        } else {
            text = "\u25BA " + text;
        }
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
            state = !state;
        }

        GUILayout.Space(2f);

        isExp = state;

        if (isExp != _sounds.showCustomEvents) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Custom Events");
            _sounds.showCustomEvents = isExp;
        }
        DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/CustomEvents.htm");

        EditorGUILayout.EndHorizontal();
        GUI.color = Color.white;

        if (_sounds.showCustomEvents) {
            var catNames = new List<string>(_sounds.customEventCategories.Count);
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _sounds.customEventCategories.Count; i++) {
                catNames.Add(_sounds.customEventCategories[i].CatName);
            }

            var selCatIndex = catNames.IndexOf(_sounds.addToCustomEventCategoryName);

            if (selCatIndex == -1) {
                selCatIndex = 0;
                _isDirty = true;
            }

            var defaultCat = catNames[selCatIndex];

            DTGUIHelper.BeginGroupedControls();
            DTGUIHelper.StartGroupHeader(0, false);
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
            var newEvent = EditorGUILayout.TextField("New Event Name", _sounds.newEventName);
            if (newEvent != _sounds.newEventName) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change New Event Name");
                _sounds.newEventName = newEvent;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(4);
            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            if (GUILayout.Button("Create New Event", EditorStyles.toolbarButton, GUILayout.Width(100))) {
                CreateCustomEvent(_sounds.newEventName, defaultCat);
            }
            GUILayout.Space(10);
            GUI.contentColor = DTGUIHelper.BrightButtonColor;

            GUI.contentColor = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            DTGUIHelper.AddSpaceForNonU5(2);

            DTGUIHelper.StartGroupHeader(0, false);
            DTGUIHelper.ResetColors();
            var newCat = EditorGUILayout.TextField("New Category Name", _sounds.newCustomEventCategoryName);
            if (newCat != _sounds.newCustomEventCategoryName) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change New Category Name");
                _sounds.newCustomEventCategoryName = newCat;
            }
            EditorGUILayout.BeginHorizontal();
            GUI.contentColor = DTGUIHelper.BrightButtonColor;
            GUILayout.Space(4);
            if (GUILayout.Button("Create New Category", EditorStyles.toolbarButton, GUILayout.Width(130))) {
                CreateCategory();
            }
            GUI.contentColor = Color.white;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            DTGUIHelper.ResetColors();

            DTGUIHelper.AddSpaceForNonU5(2);
            GUI.backgroundColor = DTGUIHelper.BrightButtonColor;

            var newIndex = EditorGUILayout.Popup("Default Event Category", selCatIndex, catNames.ToArray());
            if (newIndex != selCatIndex) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Default Event Category");
                _sounds.addToCustomEventCategoryName = catNames[newIndex];
            }

            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);

            var hasExpanded = false;
            foreach (var t in _sounds.customEvents) {
                if (string.IsNullOrEmpty(t.categoryName)) {
                    t.categoryName = defaultCat;
                }

                if (!t.eventExpanded) {
                    continue;
                }
                hasExpanded = true;
                break;
            }

            var buttonText = hasExpanded ? "Collapse All" : "Expand All";

            if (GUILayout.Button(buttonText, EditorStyles.toolbarButton, GUILayout.Width(80))) {
                ExpandCollapseCustomEvents(!hasExpanded);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            int? indexToShiftUp = null;
            int? indexToShiftDown = null;
            CustomEventCategory catEditing = null;
            CustomEventCategory catRenaming = null;
            CustomEventCategory catToDelete = null;
            CustomEvent eventEditing = null;
            CustomEvent eventToDelete = null;
            CustomEvent eventRenaming = null;

            DTGUIHelper.StartGroupHeader(1, true);

            for (var c = 0; c < _sounds.customEventCategories.Count; c++) {
                var cat = _sounds.customEventCategories[c];

                EditorGUI.indentLevel = 0;

                var matchingItems = new List<CustomEvent>();
                matchingItems.AddRange(_sounds.customEvents);
                matchingItems.RemoveAll(delegate (CustomEvent x) {
                    return x.categoryName != cat.CatName;
                });

                var hasItems = matchingItems.Count > 0;

                EditorGUILayout.BeginHorizontal();

                if (!cat.IsEditing || Application.isPlaying) {
                    var catName = cat.CatName;

                    catName += ": " + matchingItems.Count + " item" + ((matchingItems.Count != 1) ? "s" : "");

                    var state2 = cat.IsExpanded;
                    var text2 = catName;

                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (!state2) {
                        GUI.backgroundColor = DTGUIHelper.InactiveHeaderColor;
                    } else {
                        GUI.backgroundColor = DTGUIHelper.BrightButtonColor;
                    }

                    text2 = "<b><size=11>" + text2 + "</size></b>";

                    if (state2) {
                        text2 = "\u25BC " + text2;
                    } else {
                        text2 = "\u25BA " + text2;
                    }
                    if (!GUILayout.Toggle(true, text2, "dragtab", GUILayout.MinWidth(20f))) {
                        state2 = !state2;
                    }

                    GUILayout.Space(2f);

                    if (state2 != cat.IsExpanded) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle expand Custom Event Category");
                        cat.IsExpanded = state2;
                    }

                    var catItemsCollapsed = true;

                    for (var i = 0; i < matchingItems.Count; i++) {
                        var item = matchingItems[i];

                        if (!item.eventExpanded) {
                            continue;
                        }
                        catItemsCollapsed = false;
                        break;
                    }

                    GUI.backgroundColor = Color.white;

                    var tooltip = catItemsCollapsed ? "Click to expand all items in this category" : "Click to collapse all items in this category";
                    var btnText = catItemsCollapsed ? "Expand" : "Collapse";

                    GUI.contentColor = DTGUIHelper.BrightButtonColor;
                    if (GUILayout.Button(new GUIContent(btnText, tooltip), EditorStyles.toolbarButton, GUILayout.Width(60), GUILayout.Height(16))) {
                        ExpandCollapseCategory(cat.CatName, catItemsCollapsed);
                    }
                    GUI.contentColor = Color.white;

                    if (!Application.isPlaying) {
                        if (c > 0) {
                            // the up arrow.
                            var upArrow = MasterAudioInspectorResources.UpArrowTexture;
                            if (GUILayout.Button(new GUIContent(upArrow, "Click to shift Category up"),
                                EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(16))) {
                                indexToShiftUp = c;
                            }
                        } else {
                            GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(16));
                        }

                        if (c < _sounds.customEventCategories.Count - 1) {
                            // The down arrow will move things towards the end of the List
                            var dnArrow = MasterAudioInspectorResources.DownArrowTexture;
                            if (GUILayout.Button(new GUIContent(dnArrow, "Click to shift Category down"),
                                EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(16))) {
                                indexToShiftDown = c;
                            }
                        } else {
                            GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.Width(24), GUILayout.Height(16));
                        }

                        var settingsIcon = new GUIContent(MasterAudioInspectorResources.GearTexture,
                            "Click to edit Category");

                        GUI.backgroundColor = Color.white;
                        if (GUILayout.Button(settingsIcon, EditorStyles.toolbarButton, GUILayout.Width(24),
                            GUILayout.Height(16))) {
                            catEditing = cat;
                        }
                        GUI.backgroundColor = DTGUIHelper.DeleteButtonColor;
                        if (GUILayout.Button(new GUIContent("Delete", "Click to delete Category"),
                            EditorStyles.miniButton, GUILayout.MaxWidth(45))) {
                            catToDelete = cat;
                        }

                        GUILayout.Space(2);
                    } else {
                        GUILayout.Space(4);
                    }

                } else {
                    GUI.backgroundColor = DTGUIHelper.BrightTextColor;
                    var tex = EditorGUILayout.TextField("", cat.ProspectiveName);
                    if (tex != cat.ProspectiveName) {
                        cat.ProspectiveName = tex;
                        _isDirty = true;
                    }

                    var buttonPressed = DTGUIHelper.AddCancelSaveButtons("Custom Event Category");

                    switch (buttonPressed) {
                        case DTGUIHelper.DTFunctionButtons.Cancel:
                            cat.IsEditing = false;
                            cat.ProspectiveName = cat.CatName;
                            _isDirty = true;
                            break;
                        case DTGUIHelper.DTFunctionButtons.Save:
                            catRenaming = cat;
                            break;
                    }

                    GUILayout.Space(4);
                }

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                if (cat.IsEditing) {
                    DTGUIHelper.VerticalSpace(2);
                }

                matchingItems.Sort(delegate (CustomEvent x, CustomEvent y) {
                    // ReSharper disable PossibleNullReferenceException
                    return string.Compare(x.EventName, y.EventName, StringComparison.Ordinal);
                    // ReSharper restore PossibleNullReferenceException
                });

                if (!hasItems) {
                    DTGUIHelper.BeginGroupedControls();
                    DTGUIHelper.ShowLargeBarAlert("This Category is empty. Add / move some items or you may delete it.");
                    DTGUIHelper.EndGroupedControls();
                }

                GUI.contentColor = Color.white;

                if (cat.IsExpanded) {
                    if (hasItems) {
                        DTGUIHelper.BeginGroupedControls();
                    }

                    for (var i = 0; i < matchingItems.Count; i++) {
                        EditorGUI.indentLevel = 1;
                        var anEvent = matchingItems[i];

                        DTGUIHelper.AddSpaceForNonU5(2);
                        DTGUIHelper.StartGroupHeader();

                        EditorGUILayout.BeginHorizontal();

                        if (!anEvent.IsEditing || Application.isPlaying) {
                            var exp = DTGUIHelper.Foldout(anEvent.eventExpanded, anEvent.EventName);
                            if (exp != anEvent.eventExpanded) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle expand Custom Event");
                                anEvent.eventExpanded = exp;
                            }

                            GUILayout.FlexibleSpace();
                            if (!Application.isPlaying) {
                                GUI.backgroundColor = DTGUIHelper.BrightButtonColor;
                                var newCatIndex = catNames.IndexOf(anEvent.categoryName);
                                var newEventCat = EditorGUILayout.Popup(newCatIndex, catNames.ToArray(),
                                    GUILayout.Width(130));
                                if (newEventCat != newCatIndex) {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                        "change Custom Event Category");
                                    anEvent.categoryName = catNames[newEventCat];
                                }
                                GUI.backgroundColor = Color.white;
                            }

                            if (Application.isPlaying) {
                                var receivers = MasterAudio.ReceiversForEvent(anEvent.EventName);

                                GUI.contentColor = DTGUIHelper.BrightButtonColor;
                                if (receivers.Count > 0) {
                                    if (
                                        GUILayout.Button(
                                            new GUIContent("Select",
                                                "Click this button to select all Receivers in the Hierarchy"),
                                            EditorStyles.toolbarButton, GUILayout.Width(50))) {
                                        var matches = new List<GameObject>(receivers.Count);

                                        foreach (var t in receivers) {
                                            matches.Add(t.gameObject);
                                        }
                                        Selection.objects = matches.ToArray();
                                    }
                                }

                                if (GUILayout.Button("Fire!", EditorStyles.toolbarButton, GUILayout.Width(50))) {
                                    MasterAudio.FireCustomEvent(anEvent.EventName, _sounds.transform);
                                }

                                GUI.contentColor = DTGUIHelper.BrightButtonColor;
                                GUILayout.Label(string.Format("Receivers: {0}", receivers.Count));
                            } else {
                                GUI.backgroundColor = Color.white;
                                var settingsIcon = new GUIContent(MasterAudioInspectorResources.GearTexture,
                                    "Click to edit Custom Event Name");
                                if (GUILayout.Button(settingsIcon, EditorStyles.toolbarButton, GUILayout.Width(24),
                                    GUILayout.Height(16))) {
                                    eventEditing = anEvent;
                                }

                                GUI.backgroundColor = DTGUIHelper.DeleteButtonColor;
                                if (GUILayout.Button(new GUIContent("Delete", "Click to delete Event"),
                                    EditorStyles.miniButton, GUILayout.MaxWidth(45))) {
                                    eventToDelete = anEvent;
                                }
                            }
                            GUI.contentColor = Color.white;
                        } else {
                            EditorGUI.indentLevel = 0;
                            GUI.backgroundColor = DTGUIHelper.BrightTextColor;
                            var tex = EditorGUILayout.TextField("", anEvent.ProspectiveName);
                            if (tex != anEvent.ProspectiveName) {
                                anEvent.ProspectiveName = tex;
                                _isDirty = true;
                            }

                            var buttonPressed = DTGUIHelper.AddCancelSaveButtons("Custom Event");

                            switch (buttonPressed) {
                                case DTGUIHelper.DTFunctionButtons.Cancel:
                                    anEvent.IsEditing = false;
                                    anEvent.ProspectiveName = cat.CatName;
                                    _isDirty = true;
                                    break;
                                case DTGUIHelper.DTFunctionButtons.Save:
                                    eventRenaming = anEvent;
                                    break;
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();

                        if (!anEvent.eventExpanded) {
                            EditorGUILayout.EndVertical();
                            continue;
                        }

                        DTGUIHelper.ResetColors();
                        EditorGUI.indentLevel = 0;

                        var rcvMode =
                            (MasterAudio.CustomEventReceiveMode)
                                EditorGUILayout.EnumPopup("Send To Receivers", anEvent.eventReceiveMode);
                        if (rcvMode != anEvent.eventReceiveMode) {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Send To Receivers");
                            anEvent.eventReceiveMode = rcvMode;
                        }

                        if (rcvMode == MasterAudio.CustomEventReceiveMode.WhenDistanceLessThan ||
                            rcvMode == MasterAudio.CustomEventReceiveMode.WhenDistanceMoreThan) {
                            var newDist = EditorGUILayout.Slider("Distance Threshold", anEvent.distanceThreshold, 0f,
                                float.MaxValue);
                            if (newDist != anEvent.distanceThreshold) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                    "change Distance Threshold");
                                anEvent.distanceThreshold = newDist;
                            }
                        }

                        if (rcvMode != MasterAudio.CustomEventReceiveMode.Never) {
                            var rcvFilter =
                                (MasterAudio.EventReceiveFilter)
                                    EditorGUILayout.EnumPopup("Valid Receivers", anEvent.eventRcvFilterMode);
                            if (rcvFilter != anEvent.eventRcvFilterMode) {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Valid Receivers");
                                anEvent.eventRcvFilterMode = rcvFilter;
                            }

                            switch (anEvent.eventRcvFilterMode) {
                                case MasterAudio.EventReceiveFilter.Closest:
                                case MasterAudio.EventReceiveFilter.Random:
                                    var newQty = EditorGUILayout.IntField("Valid Qty", anEvent.filterModeQty);
                                    if (newQty != anEvent.filterModeQty) {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                                            "change Valid Qty");
                                        anEvent.filterModeQty = System.Math.Max(1, newQty);
                                    }
                                    break;
                            }
                        }

                        EditorGUILayout.EndVertical();
                    }

                    if (hasItems) {
                        DTGUIHelper.EndGroupedControls();
                    }
                }

                if (c < _sounds.customEventCategories.Count - 1) {
                    DTGUIHelper.VerticalSpace(3);
                }
            }
            DTGUIHelper.EndGroupHeader();

            if (eventToDelete != null) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Delete Custom Event");
                _sounds.customEvents.Remove(eventToDelete);
            }

            if (indexToShiftUp.HasValue) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "shift up Category");
                var item = _sounds.customEventCategories[indexToShiftUp.Value];
                _sounds.customEventCategories.Insert(indexToShiftUp.Value - 1, item);
                _sounds.customEventCategories.RemoveAt(indexToShiftUp.Value + 1);
                _isDirty = true;
            }

            if (indexToShiftDown.HasValue) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "shift down Category");
                var index = indexToShiftDown.Value + 1;
                var item = _sounds.customEventCategories[index];
                _sounds.customEventCategories.Insert(index - 1, item);
                _sounds.customEventCategories.RemoveAt(index + 1);
                _isDirty = true;
            }

            if (catToDelete != null) {
                if (_sounds.customEvents.FindAll(delegate (CustomEvent x) {
                    return x.categoryName == catToDelete.CatName;
                }).Count > 0) {
                    DTGUIHelper.ShowAlert("You cannot delete a Category with Custom Events in it. Move or delete the items first.");
                } else if (_sounds.customEventCategories.Count <= 1) {
                    DTGUIHelper.ShowAlert("You cannot delete the last Category.");
                } else {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Delete Category");
                    _sounds.customEventCategories.Remove(catToDelete);
                    _isDirty = true;
                }
            }

            if (catRenaming != null) {
                // ReSharper disable once ForCanBeConvertedToForeach
                var isValidName = true;

                if (string.IsNullOrEmpty(catRenaming.ProspectiveName)) {
                    isValidName = false;
                    DTGUIHelper.ShowAlert("You cannot have a blank Category name.");
                }

                if (isValidName) {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var c = 0; c < _sounds.customEventCategories.Count; c++) {
                        var cat = _sounds.customEventCategories[c];
                        // ReSharper disable once InvertIf
                        if (cat != catRenaming && cat.CatName == catRenaming.ProspectiveName) {
                            isValidName = false;
                            DTGUIHelper.ShowAlert("You already have a Category named '" + catRenaming.ProspectiveName + "'. Category names must be unique.");
                        }
                    }

                    if (isValidName) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Undo change Category name.");

                        // ReSharper disable once ForCanBeConvertedToForeach
                        for (var i = 0; i < _sounds.customEvents.Count; i++) {
                            var item = _sounds.customEvents[i];
                            if (item.categoryName == catRenaming.CatName) {
                                item.categoryName = catRenaming.ProspectiveName;
                            }
                        }

                        catRenaming.CatName = catRenaming.ProspectiveName;
                        catRenaming.IsEditing = false;
                        _isDirty = true;
                    }
                }
            }

            if (catEditing != null) {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var c = 0; c < _sounds.customEventCategories.Count; c++) {
                    var cat = _sounds.customEventCategories[c];
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (catEditing == cat) {
                        cat.IsEditing = true;
                    } else {
                        cat.IsEditing = false;
                    }

                    _isDirty = true;
                }
            }

            if (eventEditing != null) {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var c = 0; c < _sounds.customEvents.Count; c++) {
                    var evt = _sounds.customEvents[c];
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (eventEditing == evt) {
                        evt.IsEditing = true;
                    } else {
                        evt.IsEditing = false;
                    }

                    _isDirty = true;
                }
            }

            if (eventRenaming != null) {
                // ReSharper disable once ForCanBeConvertedToForeach
                var isValidName = true;

                if (string.IsNullOrEmpty(eventRenaming.ProspectiveName)) {
                    isValidName = false;
                    DTGUIHelper.ShowAlert("You cannot have a blank Custom Event name.");
                }

                if (isValidName) {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var c = 0; c < _sounds.customEvents.Count; c++) {
                        var evt = _sounds.customEvents[c];
                        // ReSharper disable once InvertIf
                        if (evt != eventRenaming && evt.EventName == eventRenaming.ProspectiveName) {
                            isValidName = false;
                            DTGUIHelper.ShowAlert("You already have a Custom Event named '" + eventRenaming.ProspectiveName + "'. Custom Event names must be unique.");
                        }
                    }

                    if (isValidName) {
                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds,
                            "Undo change Custom Event name.");

                        eventRenaming.EventName = eventRenaming.ProspectiveName;
                        eventRenaming.IsEditing = false;
                        _isDirty = true;
                    }
                }
            }

            DTGUIHelper.EndGroupedControls();
        }

        // Custom Events End

        if (GUI.changed || _isDirty) {
            EditorUtility.SetDirty(target);
        }

        //DrawDefaultInspector();
    }

#if UNITY_5 || UNITY_2017_1_OR_NEWER
    private static void SetSpatialBlendsForPlaylistControllers() {
        foreach (var t in PlaylistController.Instances) {
            t.SetSpatialBlend();
        }
    }

    private static void SetSpatialBlendForGroupsEdit() {
        for (var i = 0; i < _sounds.transform.childCount; i++) {
            var grp = _sounds.transform.GetChild(i);
            for (var c = 0; c < grp.childCount; c++) {
                var aVar = grp.GetChild(c).GetComponent<SoundGroupVariation>();
                aVar.SetSpatialBlend();
            }
        }
    }

    private static void SetSpatialBlendForPlaylistsEdit() {
        var controllers = FindObjectsOfType(typeof(PlaylistController));
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < controllers.Length; i++) {
            var cont = (controllers[i] as PlaylistController);
            // ReSharper disable once PossibleNullReferenceException
            cont.SetSpatialBlend();
        }
    }
#endif

    private void AddSongToPlaylist(MasterAudio.Playlist pList, AudioClip aClip) {
        MusicSetting lastClip = null;
        if (pList.MusicSettings.Count > 0) {
            lastClip = pList.MusicSettings[pList.MusicSettings.Count - 1];
        }

        MusicSetting mus;

        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "add Song");

        if (lastClip != null && lastClip.clip == null && pList.bulkLocationMode == MasterAudio.AudioLocation.Clip) {
            mus = lastClip;
            mus.clip = aClip;
        } else if (lastClip != null && string.IsNullOrEmpty(lastClip.resourceFileName) && pList.bulkLocationMode == MasterAudio.AudioLocation.ResourceFile) {
            mus = lastClip;
            var unused = false;
            var resourceFileName = DTGUIHelper.GetResourcePath(aClip, ref unused);
            if (string.IsNullOrEmpty(resourceFileName)) {
                // ReSharper disable once RedundantAssignment
                resourceFileName = aClip.name;
            } else {
                mus.resourceFileName = resourceFileName;
            }
        } else {
            mus = new MusicSetting() {
                volume = 1f,
                pitch = 1f,
                isExpanded = true,
                audLocation = pList.bulkLocationMode
            };

            switch (pList.bulkLocationMode) {
                case MasterAudio.AudioLocation.Clip:
                    mus.clip = aClip;
                    if (aClip != null) {
                        mus.songName = aClip.name;
                    }
                    break;
                case MasterAudio.AudioLocation.ResourceFile:
                    var unused = false;
                    var resourceFileName = DTGUIHelper.GetResourcePath(aClip, ref unused);
                    if (string.IsNullOrEmpty(resourceFileName)) {
                        resourceFileName = aClip.name;
                    }

                    mus.clip = null;
                    mus.resourceFileName = resourceFileName;
                    mus.songName = aClip.name;
                    break;
            }

            pList.MusicSettings.Add(mus);
        }
    }

    private static void CreateVariation(Transform groupTrans, AudioClip aClip) {
        var resourceFileName = string.Empty;
        var useLocalization = false;

        if (_sounds.bulkLocationMode == MasterAudio.AudioLocation.ResourceFile) {
            resourceFileName = DTGUIHelper.GetResourcePath(aClip, ref useLocalization);
            if (string.IsNullOrEmpty(resourceFileName)) {
                resourceFileName = aClip.name;
            }
        }

        var newVariation = (GameObject)Instantiate(_sounds.soundGroupVariationTemplate.gameObject, groupTrans.position, Quaternion.identity);

        var clipName = UtilStrings.TrimSpace(aClip.name);

        newVariation.name = clipName;
        newVariation.transform.parent = groupTrans;
        newVariation.gameObject.layer = _sounds.gameObject.layer;

        var variation = newVariation.GetComponent<SoundGroupVariation>();

        if (_sounds.bulkLocationMode == MasterAudio.AudioLocation.ResourceFile) {
            variation.audLocation = MasterAudio.AudioLocation.ResourceFile;
            variation.resourceFileName = resourceFileName;
            variation.useLocalization = useLocalization;
        } else {
            variation.VarAudio.clip = aClip;
        }

        CopyFromAudioSourceTemplate(variation.VarAudio, false);

        newVariation.transform.name = clipName;
    }

    private static Transform CreateSoundGroup(AudioClip aClip) {
        var groupName = aClip.name;

        if (_sounds.soundGroupTemplate == null || _sounds.soundGroupVariationTemplate == null) {
            DTGUIHelper.ShowAlert("Your MasterAudio prefab has been altered and cannot function properly. Please Revert it before continuing.");
            return null;
        }

        if (_sounds.transform.GetChildTransform(groupName) != null) {
            DTGUIHelper.ShowAlert("You already have a Sound Group named '" + groupName + "'. Please rename one of them when finished.");
        }

        var resourceFileName = string.Empty;
        var useLocalization = false;

        if (_sounds.bulkLocationMode == MasterAudio.AudioLocation.ResourceFile) {
            resourceFileName = DTGUIHelper.GetResourcePath(aClip, ref useLocalization);
            if (string.IsNullOrEmpty(resourceFileName)) {
                resourceFileName = aClip.name;
            }
        }

        var newGroup = (GameObject)Instantiate(_sounds.soundGroupTemplate.gameObject, _sounds.transform.position, Quaternion.identity);

        var grp = newGroup.GetComponent<MasterAudioGroup>();

#if UNITY_5 || UNITY_2017_1_OR_NEWER
        if (_sounds.mixerSpatialBlendType == MasterAudio.AllMixerSpatialBlendType.AllowDifferentPerGroup) {
            grp.spatialBlendType = _sounds.newGroupSpatialType;
            grp.spatialBlend = _sounds.newGroupSpatialBlend;
        }
#endif

        var groupTrans = newGroup.transform;
        groupTrans.name = UtilStrings.TrimSpace(groupName);

        var sName = groupName;

        var newVariation = (GameObject)Instantiate(_sounds.soundGroupVariationTemplate.gameObject, groupTrans.position, Quaternion.identity);

        var variation = newVariation.GetComponent<SoundGroupVariation>();

        if (_sounds.bulkLocationMode == MasterAudio.AudioLocation.ResourceFile) {
            variation.audLocation = MasterAudio.AudioLocation.ResourceFile;
            variation.resourceFileName = resourceFileName;
            variation.useLocalization = useLocalization;
            grp.bulkVariationMode = MasterAudio.AudioLocation.ResourceFile;
        } else {
            variation.VarAudio.clip = aClip;
        }

        CopyFromAudioSourceTemplate(variation.VarAudio, false);

        newVariation.transform.name = sName;
        newVariation.transform.parent = groupTrans;
        newVariation.gameObject.layer = _sounds.gameObject.layer;

        groupTrans.parent = _sounds.transform;
        groupTrans.gameObject.layer = _sounds.gameObject.layer;

        MasterAudioGroupInspector.RescanChildren(grp);

        return groupTrans;
    }

    private static MasterAudioGroup CreateSoundGroupFromTemplate(AudioClip aClip, int groupTemplateIndex) {
        var groupName = aClip.name;

        if (_sounds.transform.GetChildTransform(groupName) != null) {
            DTGUIHelper.ShowAlert("You already have a Sound Group named '" + groupName + "'. Please rename one of them when finished.");
        }

        var resourceFileName = string.Empty;
        var useLocalization = false;

        if (_sounds.bulkLocationMode == MasterAudio.AudioLocation.ResourceFile) {
            resourceFileName = DTGUIHelper.GetResourcePath(aClip, ref useLocalization);
            if (string.IsNullOrEmpty(resourceFileName)) {
                resourceFileName = aClip.name;
            }
        }

        var newGroup = (GameObject)Instantiate(_sounds.groupTemplates[groupTemplateIndex], _sounds.transform.position, Quaternion.identity);

        var grp = newGroup.GetComponent<MasterAudioGroup>();

#if UNITY_5 || UNITY_2017_1_OR_NEWER
        if (_sounds.mixerSpatialBlendType == MasterAudio.AllMixerSpatialBlendType.AllowDifferentPerGroup) {
            grp.spatialBlendType = _sounds.newGroupSpatialType;
            grp.spatialBlend = _sounds.newGroupSpatialBlend;
        }
#endif

        var groupTrans = newGroup.transform;
        groupTrans.name = UtilStrings.TrimSpace(groupName);

        var sName = groupName;

        var audSrcTemplate = SelectedAudioSourceTemplate;

        for (var i = 0; i < newGroup.transform.childCount; i++) {
            var aVar = newGroup.transform.GetChild(i);

            aVar.gameObject.layer = _sounds.gameObject.layer;
            if (aVar.name == "Silence") {
                continue; // no clip
            }

            var variation = aVar.GetComponent<SoundGroupVariation>();

            if (_sounds.bulkLocationMode == MasterAudio.AudioLocation.ResourceFile) {
                variation.audLocation = MasterAudio.AudioLocation.ResourceFile;
                variation.resourceFileName = resourceFileName;
                variation.useLocalization = useLocalization;
                grp.bulkVariationMode = MasterAudio.AudioLocation.ResourceFile;
            } else {
                variation.VarAudio.clip = aClip;
            }

            if (audSrcTemplate != null) {
                var varAudio = variation.VarAudio;
                CopyFromAudioSourceTemplate(varAudio, false);
            }

            variation.transform.name = sName + (i + 1);
        }

        groupTrans.parent = _sounds.transform;
        groupTrans.gameObject.layer = _sounds.gameObject.layer;

        MasterAudio.RescanGroupsNow();

        return grp;
    }

    /// <summary>
    /// Returns bus name
    /// </summary>
    /// <param name="groupIndex"></param>
    /// <returns></returns>
    private string CreateBus(int groupIndex) {
        var sourceGroup = groups[groupIndex];

        var affectedObjects = new Object[] {
            _sounds,
            sourceGroup
        };

        if (!Application.isPlaying) {
            AudioUndoHelper.RecordObjectsForUndo(affectedObjects, "create Bus");
        }

        var newBus = new GroupBus() {
            busName = RenameMeBusName
        };
        _sounds.groupBuses.Add(newBus);

        var newBusIndex = MasterAudio.HardCodedBusOptions + _sounds.groupBuses.Count - 1;

        if (Application.isPlaying) {
            MasterAudio.RouteGroupToBus(sourceGroup.name, newBus.busName);
        } else {
            sourceGroup.busIndex = newBusIndex;
        }

        return newBus.busName;
    }

    private void SoloBus(int busIndex) {
        var bus = _sounds.groupBuses[busIndex];

        var willSolo = !bus.isSoloed;

        var affectedGroups = new List<MasterAudioGroup>();

        foreach (var t in groups) {
            var aGroup = t;

            if (aGroup.busIndex != MasterAudio.HardCodedBusOptions + busIndex) {
                continue;
            }

            affectedGroups.Add(aGroup);
        }

        var allObjects = new List<Object> { _sounds };

        foreach (var g in affectedGroups) {
            allObjects.Add(g);
        }

        AudioUndoHelper.RecordObjectsForUndo(allObjects.ToArray(), "solo Bus");

        //change everything
        bus.isSoloed = willSolo;
        if (bus.isSoloed) {
            bus.isMuted = false;
        }

        foreach (var g in affectedGroups) {
            var sType = g.name;

            if (Application.isPlaying) {
                if (willSolo) {
                    MasterAudio.SoloGroup(sType, false);
                } else {
                    MasterAudio.UnsoloGroup(sType, false);
                }
            }

            g.isSoloed = willSolo;
            if (willSolo) {
                g.isMuted = false;
            }
        }

        if (Application.isPlaying) {
            MasterAudio.SilenceOrUnsilenceGroupsFromSoloChange();
        }
    }

    private void MuteBus(int busIndex) {
        var bus = _sounds.groupBuses[busIndex];

        var willMute = !bus.isMuted;

        var affectedGroups = new List<MasterAudioGroup>();

        foreach (var aGroup in groups) {
            if (aGroup.busIndex != MasterAudio.HardCodedBusOptions + busIndex) {
                continue;
            }

            affectedGroups.Add(aGroup);
        }

        var allObjects = new List<Object> { _sounds };
        foreach (var g in affectedGroups) {
            allObjects.Add(g);
        }

        AudioUndoHelper.RecordObjectsForUndo(allObjects.ToArray(), "mute Bus");

        // change everything
        bus.isMuted = willMute;

        if (bus.isSoloed) {
            bus.isSoloed = false;
        }

        foreach (var g in affectedGroups) {
            if (Application.isPlaying) {
                if (!willMute) {
                    MasterAudio.UnmuteGroup(g.name, false);
                } else {
                    MasterAudio.MuteGroup(g.name, false);
                }
            } else {
                g.isMuted = willMute;
                if (bus.isMuted) {
                    g.isSoloed = false;
                }
            }
        }

        if (Application.isPlaying) {
            MasterAudio.SilenceOrUnsilenceGroupsFromSoloChange();
        }
    }

    private void DeleteBus(int busIndex) {
        var groupsWithBus = new List<MasterAudioGroup>();
        var groupsWithHigherBus = new List<MasterAudioGroup>();

        foreach (var aGroup in groups) {
            if (aGroup.busIndex == -1) {
                continue;
            }
            if (aGroup.busIndex == busIndex + MasterAudio.HardCodedBusOptions) {
                groupsWithBus.Add(aGroup);
            } else if (aGroup.busIndex > busIndex + MasterAudio.HardCodedBusOptions) {
                groupsWithHigherBus.Add(aGroup);
            }
        }

        var allObjects = new List<Object> { _sounds };
        foreach (var g in groupsWithBus) {
            allObjects.Add(g);
        }

        foreach (var g in groupsWithHigherBus) {
            allObjects.Add(g);
        }

        if (Application.isPlaying) {
            var deadBus = _sounds.groupBuses[busIndex];
            if (deadBus != null) {
                if (deadBus.isSoloed) {
                    MasterAudio.UnsoloBus(deadBus.busName, false);
                } else if (deadBus.isMuted) {
                    MasterAudio.UnmuteBus(deadBus.busName, false);
                }
            }
        }

        AudioUndoHelper.RecordObjectsForUndo(allObjects.ToArray(), "delete Bus");

        if (Application.isPlaying) {
            MasterAudio.DeleteBusByIndex(busIndex + MasterAudio.HardCodedBusOptions);
            MasterAudio.SilenceOrUnsilenceGroupsFromSoloChange();

            return;
        }

        // change all
        _sounds.groupBuses.RemoveAt(busIndex);

        foreach (var group in groupsWithBus) {
            group.busIndex = -1;
        }

        foreach (var group in groupsWithHigherBus) {
            group.busIndex--;
        }
    }

    private void ExpandCollapseAllPlaylists(bool expand) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle Expand / Collapse Playlists");

        foreach (var aList in _sounds.musicPlaylists) {
            aList.isExpanded = expand;
        }
    }

    private void ScanGroups() {
        _isValid = true;
        invalidReason = string.Empty;

        if (groups != null && !MasterAudio.ShouldRescanGroups) {
            return;
        }

        groups = new List<MasterAudioGroup>();

        var names = new List<string>();

        for (var i = 0; i < _sounds.transform.childCount; i++) {
            var aChild = _sounds.transform.GetChild(i);
            if (names.Contains(aChild.name)) {
                invalidReason = "You have more than one group named '" + aChild.name + "'. Please rename one of them before continuing.";
                _isValid = false;
                return;
            }

            names.Add(aChild.name);
            var aGroup = aChild.GetComponent<MasterAudioGroup>();

            if (aGroup == null) {
                if (!ArrayListUtil.IsExcludedChildName(aChild.name)) {
                    Debug.LogError("No MasterAudioGroup script found on Group '" + aChild.name + "'. This should never happen. Please delete and recreate this Group.");
                }
                continue;
            }

            groups.Add(aGroup);
        }

        if (_sounds.groupByBus) {
            groups.Sort(delegate (MasterAudioGroup g1, MasterAudioGroup g2) {
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (g1.busIndex == g2.busIndex) {
                    return g1.name.CompareTo(g2.name);
                }

                return g1.busIndex.CompareTo(g2.busIndex);
            });
        } else {
            groups.Sort(delegate (MasterAudioGroup g1, MasterAudioGroup g2) {
                return g1.name.CompareTo(g2.name);
            });
        }

        if (MasterAudio.SafeInstance != null) {
            // done rescanning, don't do it next frame hopefully.
            MasterAudio.DoneRescanningGroups();
        }
    }

    private List<string> GroupNameList {
        get {
            var groupNames = new List<string> { MasterAudio.NoGroupName };

            foreach (var t in groups) {
                groupNames.Add(t.name);
            }

            return groupNames;
        }
    }

    public static void DisplayJukebox(List<PlaylistController> controllers, List<string> playlistNames) {
        EditorGUILayout.Separator();

        var songNames = new List<string>();

        foreach (var t in controllers) {
            var pl = t;

            GUI.backgroundColor = Color.white;
            GUI.color = DTGUIHelper.ActiveHeaderColor;

            EditorGUILayout.BeginVertical(DTGUIHelper.CornerGUIStyle);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            GUI.color = DTGUIHelper.BrightButtonColor;
            var playlistIndex = -1;
            if (!string.IsNullOrEmpty(pl.startPlaylistName) && pl.HasPlaylist && pl.CurrentPlaylist != null) {
                playlistIndex = playlistNames.IndexOf(pl.CurrentPlaylist.playlistName);

                songNames.Clear();
                foreach (var aSong in pl.CurrentPlaylist.MusicSettings) {
                    var songName = string.Empty;

                    switch (aSong.audLocation) {
                        case MasterAudio.AudioLocation.Clip:
                            songName = aSong.clip == null ? string.Empty : aSong.clip.name;
                            break;
                        case MasterAudio.AudioLocation.ResourceFile:
                            songName = aSong.resourceFileName;
                            break;
                    }

                    if (string.IsNullOrEmpty(songName)) {
                        continue;
                    }

                    songNames.Add(songName);
                }
            }

            GUILayout.Label(pl.name);

            GUILayout.FlexibleSpace();

            GUILayout.Label("V " + pl._playlistVolume.ToString("N2"));

            var newVol = GUILayout.HorizontalSlider(pl._playlistVolume, 0f, 1f, GUILayout.Width(100));
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (newVol != pl._playlistVolume) {
                pl.PlaylistVolume = newVol;
            }

            GUI.color = Color.white;
            var muteButtonPressed = DTGUIHelper.AddPlaylistControllerSetupButtons(pl, "Playlist Controller", true);

            if (muteButtonPressed == DTGUIHelper.DTFunctionButtons.Mute) {
                pl.ToggleMutePlaylist();
            }

            GUILayout.Space(4);
            DTGUIHelper.AddHelpIcon("http://www.dtdevtools.com/docs/masteraudio/PlaylistSettings.htm#Jukeboxes");

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            GUI.backgroundColor = DTGUIHelper.BrightButtonColor;
            GUI.color = DTGUIHelper.BrightButtonColor;

            EditorGUILayout.BeginVertical(DTGUIHelper.CornerGUIStyle);

            var clip = pl.CurrentPlaylistClip;
            var clipPosition = "";
            AudioSource playingSource = null;
            if (clip != null) {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                playingSource = pl == null ? null : pl.CurrentPlaylistSource;
                if (playingSource != null) {
                    var secondsRemaining = DTGUIHelper.AdjustAudioClipDurationForPitch(clip.length - playingSource.time, playingSource); // adjust time remaining to account for pitch.

                    clipPosition = "(-" + (secondsRemaining).ToString("N2") + " secs)";
                }
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Playlist:");
            GUILayout.Space(30);
            var playlistIndexToStart = EditorGUILayout.Popup(playlistIndex, playlistNames.ToArray(), GUILayout.Width(180));

            if (playlistIndex != playlistIndexToStart) {
                pl.ChangePlaylist(playlistNames[playlistIndexToStart]);
            }
            GUILayout.Label(string.Format("[{0}]", pl.PlaylistState));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Active Clip:");
            GUILayout.Space(9);

            var songIndex = -1;
            if (pl.CurrentPlaylistClip != null) {
                songIndex = songNames.IndexOf(pl.CurrentPlaylistClip.name);
            }
            var newSong = EditorGUILayout.Popup(songIndex, songNames.ToArray(), GUILayout.Width(180));
            if (newSong != songIndex) {
                pl.TriggerPlaylistClip(songNames[newSong]);
            }

            if (!string.IsNullOrEmpty(clipPosition)) {
                GUILayout.Label(clipPosition);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var fadingClip = pl == null ? null : pl.FadingPlaylistClip;
            var fadingClipName = fadingClip == null ? "[None]" : fadingClip.name;
            var fadingClipPosition = "";
            if (fadingClip != null) {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                var fadingSource = pl == null ? null : pl.FadingSource;
                if (fadingSource != null) {
                    fadingClipPosition = "(-" + DTGUIHelper.AdjustAudioClipDurationForPitch(fadingClip.length - fadingSource.time, fadingSource).ToString("N2") + " secs)";
                }
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Fading Clip:");
            GUILayout.Space(7);
            GUILayout.Label(fadingClipName + "  " + fadingClipPosition);
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            GUI.color = DTGUIHelper.BrightButtonColor;

            // ReSharper disable once RedundantAssignment
            var buttonPressed = DTGUIHelper.JukeboxButtons.None;

            GUI.backgroundColor = DTGUIHelper.ActiveHeaderColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.miniButton);
            buttonPressed = DTGUIHelper.AddJukeboxIcons();
            if (playingSource != null) {
                var oldtime = playingSource.time;
                var newTime = EditorGUILayout.Slider("", oldtime, 0f, clip.length);
                if (oldtime != newTime) {
                    playingSource.time = newTime;
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Separator();

            switch (buttonPressed) {
                case DTGUIHelper.JukeboxButtons.Stop:
                    pl.StopPlaylist();
                    break;
                case DTGUIHelper.JukeboxButtons.NextSong:
                    pl.PlayTheNextSong(PlaylistController.AudioPlayType.PlayNow);
                    break;
                case DTGUIHelper.JukeboxButtons.Pause:
                    pl.PausePlaylist();
                    break;
                case DTGUIHelper.JukeboxButtons.Play:
                    if (!pl.UnpausePlaylist()) {
                        if (pl.CurrentPlaylist != null) {
                            pl.StartPlaylist(pl.CurrentPlaylist.playlistName);
                        }
                    }
                    break;
                case DTGUIHelper.JukeboxButtons.RandomSong:
                    pl.PlayARandomSong(PlaylistController.AudioPlayType.PlayNow);
                    break;
            }
        }
    }

    private void MuteSoloReset() {
        var allObjects = new List<Object> { _sounds };
        foreach (var g in groups) {
            allObjects.Add(g);
        }

        AudioUndoHelper.RecordObjectsForUndo(allObjects.ToArray(), "Mute/Solo Reset");

        //reset everything

        foreach (var aBus in _sounds.groupBuses) {
            if (Application.isPlaying) {
                if (aBus.isSoloed) {
                    MasterAudio.UnsoloBus(aBus.busName, false);
                } else if (aBus.isMuted) {
                    MasterAudio.UnmuteBus(aBus.busName, false);
                }
            } else {
                aBus.isSoloed = false;
                aBus.isMuted = false;
            }
        }

        foreach (var gr in groups) {
            if (Application.isPlaying) {
                MasterAudio.UnsoloGroup(gr.name, false);
                MasterAudio.UnmuteGroup(gr.name, false);
            } else {
                gr.isSoloed = false;
                gr.isMuted = false;
            }
        }

        _sounds.mixerMuted = false;

        if (Application.isPlaying) {
            MasterAudio.SilenceOrUnsilenceGroupsFromSoloChange();
        }
    }

    private void EqualizePlaylistVolumes(List<MusicSetting> playlistClips) {
        var clips = new Dictionary<MusicSetting, float>();

        if (playlistClips.Count < 2) {
            DTGUIHelper.ShowAlert("You must have at least 2 clips in a Playlist to use this function.");
            return;
        }

        var lowestVolume = 1f;

        foreach (var setting in playlistClips) {
            var ac = setting.clip;

            if (setting.audLocation == MasterAudio.AudioLocation.Clip && ac == null) {
                continue;
            }

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (setting.audLocation == MasterAudio.AudioLocation.FileOnInternet) {
                continue;
            }

            if (setting.audLocation == MasterAudio.AudioLocation.ResourceFile) {
                ac = Resources.Load(setting.resourceFileName) as AudioClip;
                if (ac == null) {
                    Debug.LogError("Song '" + setting.resourceFileName + "' could not be loaded and is being skipped.");
                    continue;
                }
            }

            var average = 0f;
            var buffer = new float[ac.samples];

            Debug.Log("Measuring amplitude of '" + ac.name + "'.");

            try {
                ac.GetData(buffer, 0);
            }
            catch {
                Debug.Log("Could not read data from compressed sample. Skipping '" + setting.clip.name + "'.");
                continue;
            }

            for (var c = 0; c < ac.samples; c++) {
                average += Mathf.Pow(buffer[c], 2);
            }

            // ReSharper disable once RedundantCast
            average = Mathf.Sqrt(1f / (float)ac.samples * average);

            if (average == 0) {
                Debug.LogError("Song '" + setting.songName + "' is being excluded because it's compressed or streaming.");
                continue;
            }

            if (average < lowestVolume) {
                lowestVolume = average;
            }

            clips.Add(setting, average);
        }

        if (clips.Count < 2) {
            DTGUIHelper.ShowAlert("You must have at least 2 clips in a Playlist to use this function.");
            return;
        }

        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Equalize Song Volumes");

        foreach (var kv in clips) {
            var adjustedVol = lowestVolume / kv.Value;
            //set your volume for each song in your playlist.
            kv.Key.volume = adjustedVol;
        }
    }

    private static void CreateCustomEvent(string newEventName, string defaultCategory) {
        if (_sounds.customEvents.FindAll(delegate (CustomEvent obj) {
            return obj.EventName == newEventName;
        }).Count > 0) {
            DTGUIHelper.ShowAlert("You already have a Custom Event named '" + newEventName + "'. Please choose a different name.");
            return;
        }

        var newEvent = new CustomEvent(newEventName);
        newEvent.categoryName = defaultCategory;

        _sounds.customEvents.Add(newEvent);
    }

    private void RenameEvent(CustomEvent cEvent) {
        var match = _sounds.customEvents.FindAll(delegate (CustomEvent obj) {
            return obj.EventName == cEvent.ProspectiveName;
        });

        if (match.Count > 0) {
            DTGUIHelper.ShowAlert("You already have a Custom Event named '" + cEvent.ProspectiveName + "'. Please choose a different name.");
            return;
        }

        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Rename Custom Event");
        cEvent.EventName = cEvent.ProspectiveName;
    }

    private void AddGroupTemplate(GameObject temp) {
        if (_groupTemplateNames.Contains(temp.name)) {
            Debug.LogError("There is already a Sound Group template named '" + temp.name + "'. The names of templates must be unique.");
            return;
        }

        if (temp.GetComponent<MasterAudioGroup>() == null) {
            Debug.LogError("This is not a Sound Group template. It must have a Master Audio Group script in the top-level.");
            return;
        }

        var hasVariation = false;
        if (temp.transform.childCount > 0) {
            var aChild = temp.transform.GetChild(0);
            hasVariation = aChild.GetComponent<SoundGroupVariation>() != null;
        }

        if (!hasVariation) {
            Debug.LogError("This is not a Sound Group template. It must have a at least one child with a Sound Group Variation script in it.");
            return;
        }

        var hasAudioClips = false;
        for (var i = 0; i < temp.transform.childCount; i++) {
            var aChild = temp.transform.GetChild(i);
            var aud = aChild.transform.GetComponent<AudioSource>();
            if (aud == null) {
                continue;
            }

            if (aud.clip != null) {
                hasAudioClips = true;
            }
        }

        if (hasAudioClips) {
            Debug.LogError("Sound Group templates cannot include any Audio Clips in their Variations. Please remove them and try again.");
            return;
        }

        var hasFilters = false;
        for (var i = 0; i < temp.transform.childCount; i++) {
            var aChild = temp.transform.GetChild(i);

            if (aChild.GetComponent<AudioDistortionFilter>() != null
                || aChild.GetComponent<AudioHighPassFilter>() != null
                || aChild.GetComponent<AudioLowPassFilter>() != null
                || aChild.GetComponent<AudioReverbFilter>() != null
                || aChild.GetComponent<AudioEchoFilter>() != null
                || aChild.GetComponent<AudioChorusFilter>() != null) {

                hasFilters = true;
            }
        }

        if (hasFilters) {
            Debug.LogError("Sound Group templates cannot include any Filter FX in their Variations. Please remove them and try again.");
            return;
        }


        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Add Group Template");
        _sounds.groupTemplates.Add(temp.gameObject);
        _sounds.groupTemplates.Sort(delegate (GameObject x, GameObject y) {
            return x.name.CompareTo(y.name);
        });

        Debug.Log("Added Group Template '" + temp.name + "'");
    }

    private void AddAudioSourceTemplate(GameObject temp) {
        if (_audioSourceTemplateNames.Contains(temp.name)) {
            Debug.LogError("There is already an Audio Source Template named '" + temp.name + "'. The names of Templates must be unique.");
            return;
        }

        if (temp.GetComponent<AudioSource>() == null) {
            Debug.LogError("This is not an Audio Source Template. It must have an Audio Source component in the top-level.");
            return;
        }

        var hasAudioClips = false;
        for (var i = 0; i < temp.transform.childCount; i++) {
            var aChild = temp.transform.GetChild(i);
            var aud = aChild.transform.GetComponent<AudioSource>();
            if (aud == null) {
                continue;
            }

            if (aud.clip != null) {
                hasAudioClips = true;
            }
        }

        if (hasAudioClips) {
            Debug.LogError("Audio Source Templates cannot include any Audio Clips. Please remove them and try again.");
            return;
        }

        var hasFilters = false;
        for (var i = 0; i < temp.transform.childCount; i++) {
            var aChild = temp.transform.GetChild(i);

            if (aChild.GetComponent<AudioDistortionFilter>() != null
                || aChild.GetComponent<AudioHighPassFilter>() != null
                || aChild.GetComponent<AudioLowPassFilter>() != null
                || aChild.GetComponent<AudioReverbFilter>() != null
                || aChild.GetComponent<AudioEchoFilter>() != null
                || aChild.GetComponent<AudioChorusFilter>() != null) {

                hasFilters = true;
            }
        }

        if (hasFilters) {
            Debug.LogError("Audio Source Templates cannot include any Filter FX in their Variations. Please remove them and try again.");
            return;
        }

        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Add Audio Source Template");
        _sounds.audioSourceTemplates.Add(temp.gameObject);
        _sounds.audioSourceTemplates.Sort(delegate (GameObject x, GameObject y) {
            return x.name.CompareTo(y.name);
        });

        Debug.Log("Added Audio Source Template '" + temp.name + "'");
    }

    private void ExpandCollapseCustomEvents(bool shouldExpand) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Expand / Collapse All Custom Events");

        foreach (var t in _sounds.customEvents) {
            t.eventExpanded = shouldExpand;
        }
    }

    private void SortSongsAlpha(MasterAudio.Playlist aList) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Sort Playlist Songs Alpha");

        aList.MusicSettings.Sort(delegate (MusicSetting x, MusicSetting y) {
            return x.songName.CompareTo(y.songName);
        });
    }

    private void ExpandCollapseSongs(MasterAudio.Playlist aList, bool shouldExpand) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Expand / Collapse Playlist Songs");

        foreach (var t in aList.MusicSettings) {
            t.isExpanded = shouldExpand;
        }
    }

    public static void StopPreviewer() {
        var pv = GetPreviewer();
        if (pv == null) {
            return;
        }

        pv.clip = null;
        pv.Stop();
    }

    public static AudioSource GetPreviewer() {
        var listener = MasterAudio.ListenerTrans;

        if (listener == null) {
            Debug.LogError("You have no AudioListener in the Scene. You cannot preview any audio.");
            return null;
        }

        var listenerGO = listener.gameObject;

        var aud = listener.GetComponent<AudioSource>();
        if (aud != null) {
            return aud;
        }

        if (MasterAudio.SafeInstance == null) {
            listenerGO.AddComponent<AudioSource>();
        } else {
            UnityEditorInternal.ComponentUtility.CopyComponent(
                MasterAudio.Instance.soundGroupVariationTemplate.GetComponent<AudioSource>());
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(listenerGO);
        }

        aud = listenerGO.GetComponent<AudioSource>();

        return aud;
    }

    private static GroupBus GetGroupBus(MasterAudioGroup aGroup) {
        if (_sounds == null) { // this is needed when calling from Group Inspector (mute / solo)
            _sounds = MasterAudio.Instance;
        }

        GroupBus groupBus = null;
        var groupBusIndex = aGroup.busIndex - MasterAudio.HardCodedBusOptions;
        if (groupBusIndex >= 0 && groupBusIndex < _sounds.groupBuses.Count) {
            groupBus = _sounds.groupBuses[groupBusIndex];
        }

        return groupBus;
    }

    public static bool MuteMixerGroup(MasterAudioGroup aGroup, ref bool groupDirty, bool isBulk, bool wasWarningShown) {
        var groupBus = GetGroupBus(aGroup);

        var warningShown = false;

        if (groupBus != null && (groupBus.isMuted || groupBus.isSoloed)) {
            if (wasWarningShown) {
                return false;
            }

            if (Application.isPlaying) {
                Debug.LogWarning(NoMuteSoloAllowed);
                warningShown = true;
            } else {
                DTGUIHelper.ShowAlert(NoMuteSoloAllowed);
                warningShown = true;
            }
        } else {
            if (!isBulk) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref groupDirty, aGroup, "toggle Group mute");
            }

            if (Application.isPlaying) {
                var sType = aGroup.name;

                if (aGroup.isMuted) {
                    MasterAudio.UnmuteGroup(sType);
                } else {
                    MasterAudio.MuteGroup(sType);
                }
            } else {
                aGroup.isMuted = !aGroup.isMuted;
                if (aGroup.isMuted) {
                    aGroup.isSoloed = false;
                }
            }
        }

        return warningShown;
    }

    public static bool SoloMixerGroup(MasterAudioGroup aGroup, ref bool groupDirty, bool isBulk, bool wasWarningShown) {
        var groupBus = GetGroupBus(aGroup);

        var warningShown = false;

        if (groupBus != null && (groupBus.isMuted || groupBus.isSoloed)) {
            if (wasWarningShown) {
                return false;
            }
            if (Application.isPlaying) {
                Debug.LogWarning(NoMuteSoloAllowed);
                warningShown = true;
            } else {
                DTGUIHelper.ShowAlert(NoMuteSoloAllowed);
                warningShown = true;
            }
        } else {
            if (!isBulk) {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref groupDirty, aGroup, "toggle Group solo");
            }

            if (Application.isPlaying) {
                var sType = aGroup.name;

                if (aGroup.isSoloed) {
                    MasterAudio.UnsoloGroup(sType);
                } else {
                    MasterAudio.SoloGroup(sType);
                }
            } else {
                aGroup.isSoloed = !aGroup.isSoloed;
                if (aGroup.isSoloed) {
                    aGroup.isMuted = false;
                }
            }
        }

        return warningShown;
    }

    private static void SetMixerGroupVolume(MasterAudioGroup aGroup, ref bool groupDirty, float newVol, bool isBulk) {
        if (!isBulk) {
            AudioUndoHelper.RecordObjectPropertyForUndo(ref groupDirty, aGroup, "change Group Volume");
        }

        aGroup.groupMasterVolume = newVol;
        if (Application.isPlaying) {
            MasterAudio.SetGroupVolume(aGroup.name, aGroup.groupMasterVolume);
        }
    }

    private static void SelectActiveVariationsInGroup(MasterAudioGroup aGroup) {
        var actives = new List<GameObject>(aGroup.transform.childCount);

        for (var i = 0; i < aGroup.transform.childCount; i++) {
            var aVar = aGroup.transform.GetChild(i).GetComponent<SoundGroupVariation>();
            if (!aVar.IsPlaying) {
                continue;
            }

            actives.Add(aVar.GameObj);
        }

        if (actives.Count == 0) {
            return;
        }

        Selection.objects = actives.ToArray();
    }

    private static void SelectActiveVariationsInBus(GroupBus aBus) {
        var objects = MasterAudio.GetAllPlayingVariationsInBus(aBus.busName);
        Selection.objects = objects.ToArray();
    }

    private static GameObject SelectedAudioSourceTemplate {
        get {
            if (MasterAudio.Instance.audioSourceTemplates.Count == 0) {
                return null;
            }

            var selTemplate = MasterAudio.Instance.audioSourceTemplates.Find(delegate (GameObject obj) {
                return obj.name == MasterAudio.Instance.audioSourceTemplateName;
            });

            return selTemplate;
        }
    }

    public static void CopyFromAudioSourceTemplate(AudioSource oldAudSrc, bool showError) {
        var selSource = SelectedAudioSourceTemplate;
        if (selSource == null) {
            if (showError) {
                Debug.LogError("No Audio Source Template selected.");
            }
            return;
        }

        var templateAudio = selSource.GetComponent<AudioSource>();

        var oldPitch = oldAudSrc.pitch;
        var oldLoop = oldAudSrc.loop;
        var oldClip = oldAudSrc.clip;
        var oldVol = oldAudSrc.volume;

        UnityEditorInternal.ComponentUtility.CopyComponent(templateAudio);
        UnityEditorInternal.ComponentUtility.PasteComponentValues(oldAudSrc);

        oldAudSrc.pitch = oldPitch;
        oldAudSrc.loop = oldLoop;
        oldAudSrc.clip = oldClip;
        oldAudSrc.volume = oldVol;
    }

    private void CreateCategory() {
        if (string.IsNullOrEmpty(_sounds.newCustomEventCategoryName)) {
            DTGUIHelper.ShowAlert("You cannot have a blank Category name.");
            return;
        }

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var c = 0; c < _sounds.customEventCategories.Count; c++) {
            var cat = _sounds.customEventCategories[c];
            // ReSharper disable once InvertIf
            if (cat.CatName == _sounds.newCustomEventCategoryName) {
                DTGUIHelper.ShowAlert("You already have a Category named '" + _sounds.newCustomEventCategoryName + "'. Category names must be unique.");
                return;
            }
        }

        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Create New Category");

        var newCat = new CustomEventCategory {
            CatName = _sounds.newCustomEventCategoryName,
            ProspectiveName = _sounds.newCustomEventCategoryName
        };

        _sounds.customEventCategories.Add(newCat);
    }

    private void CreateMetadataProperty(MasterAudio.Playlist list, string propName, SongMetadataProperty.MetadataPropertyType propType, bool required, bool canHaveMult) {
        if (propName != null) {
            propName = propName.Replace(" ", "");
        }
        if (string.IsNullOrEmpty(propName)) {
            DTGUIHelper.ShowAlert("You must give a name to your new Meta Property.");
            return;
        }

        var match = list.songMetadataProps.Find(delegate (SongMetadataProperty p) {
            return p.PropertyName == propName;
        });

        if (match != null) {
            DTGUIHelper.ShowAlert("You already have a Metadata Property named '" + propName + "'. Names must be unique.");
            return;
        }

        if (propType == SongMetadataProperty.MetadataPropertyType.Boolean) {
            canHaveMult = false;
        }

        var newProp = new SongMetadataProperty(propName, propType, required, canHaveMult);

        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Create Metadata Property");

        if (required) {
            for (var s = 0; s < list.MusicSettings.Count; s++) {
                var aSong = list.MusicSettings[s];

                switch (propType) {
                    case SongMetadataProperty.MetadataPropertyType.Boolean:
                        var bVal = new SongMetadataBoolValue(newProp);
                        aSong.metadataBoolValues.Add(bVal);
                        break;
                    case SongMetadataProperty.MetadataPropertyType.String:
                        var sVal = new SongMetadataStringValue(newProp);
                        aSong.metadataStringValues.Add(sVal);
                        break;
                    case SongMetadataProperty.MetadataPropertyType.Integer:
                        var iVal = new SongMetadataIntValue(newProp);
                        aSong.metadataIntValues.Add(iVal);
                        break;
                    case SongMetadataProperty.MetadataPropertyType.Float:
                        var fVal = new SongMetadataFloatValue(newProp);
                        aSong.metadataFloatValues.Add(fVal);
                        break;
                }
            }
        }

        list.songMetadataProps.Add(newProp);
    }

    private void DeleteMetadataProperty(MasterAudio.Playlist aList, int index) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Delete Metadata Property");

        var deadProperty = aList.songMetadataProps[index];
        var deadName = deadProperty.PropertyName;

        switch (deadProperty.PropertyType) {
            case SongMetadataProperty.MetadataPropertyType.String:
                for (var i = 0; i < aList.MusicSettings.Count; i++) {
                    var aSong = aList.MusicSettings[i];

                    aSong.metadataStringValues.RemoveAll(delegate (SongMetadataStringValue val) {
                        return val.PropertyName == deadName;
                    });
                }
                break;
            case SongMetadataProperty.MetadataPropertyType.Boolean:
                for (var i = 0; i < aList.MusicSettings.Count; i++) {
                    var aSong = aList.MusicSettings[i];
                    aSong.metadataBoolValues.RemoveAll(delegate (SongMetadataBoolValue val) {
                        return val.PropertyName == deadName;
                    });
                }
                break;
            case SongMetadataProperty.MetadataPropertyType.Integer:
                for (var i = 0; i < aList.MusicSettings.Count; i++) {
                    var aSong = aList.MusicSettings[i];
                    aSong.metadataIntValues.RemoveAll(delegate (SongMetadataIntValue val) {
                        return val.PropertyName == deadName;
                    });
                }
                break;
            case SongMetadataProperty.MetadataPropertyType.Float:
                for (var i = 0; i < aList.MusicSettings.Count; i++) {
                    var aSong = aList.MusicSettings[i];
                    aSong.metadataFloatValues.RemoveAll(delegate (SongMetadataFloatValue val) {
                        return val.PropertyName == deadName;
                    });
                }
                break;
        }

        aList.songMetadataProps.Remove(deadProperty);
    }

    private void MakePropertyRequired(SongMetadataProperty property, MasterAudio.Playlist aList) {
        // add one if there isn't one.
        for (var i = 0; i < aList.MusicSettings.Count; i++) {
            var aSong = aList.MusicSettings[i];

            switch (property.PropertyType) {
                case SongMetadataProperty.MetadataPropertyType.String:
                    if (aSong.metadataStringValues.FindIndex(delegate (SongMetadataStringValue v) {
                        return v.PropertyName == property.PropertyName;
                    }) < 0) {
                        aSong.metadataStringValues.Add(new SongMetadataStringValue(property));
                    }
                    break;
                case SongMetadataProperty.MetadataPropertyType.Boolean:
                    if (aSong.metadataBoolValues.FindIndex(delegate (SongMetadataBoolValue b) {
                        return b.PropertyName == property.PropertyName;
                    }) < 0) {
                        aSong.metadataBoolValues.Add(new SongMetadataBoolValue(property));
                    }
                    break;
                case SongMetadataProperty.MetadataPropertyType.Integer:
                    if (aSong.metadataIntValues.FindIndex(delegate (SongMetadataIntValue n) {
                        return n.PropertyName == property.PropertyName;
                    }) < 0) {
                        aSong.metadataIntValues.Add(new SongMetadataIntValue(property));
                    }
                    break;
                case SongMetadataProperty.MetadataPropertyType.Float:
                    if (aSong.metadataFloatValues.FindIndex(delegate (SongMetadataFloatValue f) {
                        return f.PropertyName == property.PropertyName;
                    }) < 0) {
                        aSong.metadataFloatValues.Add(new SongMetadataFloatValue(property));
                    }
                    break;
            }
        }
    }

    private void ExpandCollapseCategory(string category, bool isExpand) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "toggle expand / collapse all items in Category");

        foreach (var item in _sounds.customEvents) {
            if (item.categoryName != category) {
                continue;
            }

            item.eventExpanded = isExpand;
        }
    }

    private void CopySongVolumes(MasterAudio.Playlist aList, float newVol) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Volume(s)");

        var changed = 0;

        foreach (var aSong in aList.MusicSettings) {
            if (!aSong.isChecked) {
                continue;
            }

            aSong.volume = newVol;
            changed++;
        }

        Debug.LogWarning(changed + " Volume(s) changed.");
    }

    private void CopySongPitches(MasterAudio.Playlist aList, float newPitch) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Pitch(es)");

        var changed = 0;

        foreach (var aSong in aList.MusicSettings) {
            if (!aSong.isChecked) {
                continue;
            }

            aSong.pitch = newPitch;
            changed++;
        }

        Debug.LogWarning(changed + " Volume(s) changed.");
    }

    private void CopySongLoops(MasterAudio.Playlist aList, bool newLoop) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Loop(s)");

        var changed = 0;

        foreach (var aSong in aList.MusicSettings) {
            if (!aSong.isChecked) {
                continue;
            }

            aSong.isLoop = newLoop;
            changed++;
        }

        Debug.LogWarning(changed + " Loop(s) changed.");
    }

    private void CopySongStartTimeMode(MasterAudio.Playlist aList, MasterAudio.CustomSongStartTimeMode mode) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Start Time Mode(s)");

        var changed = 0;

        foreach (var aSong in aList.MusicSettings) {
            if (!aSong.isChecked) {
                continue;
            }

            aSong.songStartTimeMode = mode;
            changed++;
        }

        Debug.LogWarning(changed + " Start Time Mode(s) changed.");
    }

    private void CopySongCustomStartTime(MasterAudio.Playlist aList, float customStartTime) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Custom Start Time(s)");

        var changed = 0;

        foreach (var aSong in aList.MusicSettings) {
            if (!aSong.isChecked) {
                continue;
            }

            aSong.customStartTime = customStartTime;
            changed++;
        }

        Debug.LogWarning(changed + " Custom Start Time(s) changed.");
    }

    private void CopySongStartTimeMin(MasterAudio.Playlist aList, float startTimeMin) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Start Time Min(s)");

        var changed = 0;

        foreach (var aSong in aList.MusicSettings) {
            if (!aSong.isChecked) {
                continue;
            }

            aSong.customStartTime = startTimeMin;
            changed++;
        }

        Debug.LogWarning(changed + " Start Time Min(s) changed.");
    }

    private void CopySongStartTimeMax(MasterAudio.Playlist aList, float startTimeMax) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "change Start Time Max(s)");

        var changed = 0;

        foreach (var aSong in aList.MusicSettings) {
            if (!aSong.isChecked) {
                continue;
            }

            aSong.customStartTimeMax = startTimeMax;
            changed++;
        }

        Debug.LogWarning(changed + " Start Time Max(s) changed.");
    }

    private void CheckAllSongs(MasterAudio.Playlist aList) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Select all Songs");
        for (var i = 0; i < aList.MusicSettings.Count; i++) {
            var aSong = aList.MusicSettings[i];
            aSong.isChecked = true;
        }
    }

    private void UncheckAllSongs(MasterAudio.Playlist aList) {
        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, _sounds, "Select all Songs");
        for (var i = 0; i < aList.MusicSettings.Count; i++) {
            var aSong = aList.MusicSettings[i];
            aSong.isChecked = false;
        }
    }

    private int GetNumCheckedSongs(MasterAudio.Playlist aList) {
        var numChecked = 0;
        for (var i = 0; i < aList.MusicSettings.Count; i++) {
            var aSong = aList.MusicSettings[i];
            if (aSong.isChecked) {
                numChecked++;
            }
        }

        return numChecked;
    }
}
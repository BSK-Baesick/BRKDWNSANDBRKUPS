using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    /// <summary>
    /// This class contains the actual Audio Source, Unity Filter FX components and other convenience methods having to do with playing sound effects.
    /// </summary>
    [AudioScriptOrder(-40)]
    [RequireComponent(typeof(SoundGroupVariationUpdater))]
    // ReSharper disable once CheckNamespace
    public class SoundGroupVariation : MonoBehaviour {
        /*! \cond PRIVATE */
        // ReSharper disable InconsistentNaming
        public int weight = 1;

        [Range(0f, 1f)]
        public int probabilityToPlay = 100;

        public bool useLocalization = false;

        public bool useRandomPitch = false;
        public RandomPitchMode randomPitchMode = RandomPitchMode.AddToClipPitch;
        public float randomPitchMin = 0f;
        public float randomPitchMax = 0f;

        public bool useRandomVolume = false;
        public RandomVolumeMode randomVolumeMode = RandomVolumeMode.AddToClipVolume;
        public float randomVolumeMin = 0f;
        public float randomVolumeMax = 0f;

        public MasterAudio.AudioLocation audLocation = MasterAudio.AudioLocation.Clip;
        public string resourceFileName;
        public string internetFileUrl;
        public MasterAudio.InternetFileLoadStatus internetFileLoadStatus = MasterAudio.InternetFileLoadStatus.Loading;
        public float original_pitch;
        public float original_volume;
        public bool isExpanded = true;
        public bool isChecked = true;

        public bool useFades = false;
        public float fadeInTime = 0f;
        public float fadeOutTime = 0f;

        public bool useCustomLooping = false;
        public int minCustomLoops = 1;
        public int maxCustomLoops = 5;

        public bool useRandomStartTime = false;
        public float randomStartMinPercent = 0f;
        public float randomStartMaxPercent = 100f;
        public float randomEndPercent = 100f;

        public bool useIntroSilence = false;
        public float introSilenceMin = 0f;
        public float introSilenceMax = 0f;
        // ReSharper restore InconsistentNaming

        // ReSharper disable InconsistentNaming
        public float fadeMaxVolume;
        public FadeMode curFadeMode = FadeMode.None;
        public PitchMode curPitchMode = PitchMode.None;
        public DetectEndMode curDetectEndMode = DetectEndMode.None;
        public int frames = 0;
        // ReSharper restore InconsistentNaming

        private AudioSource _audioSource;

        private readonly PlaySoundParams _playSndParam = new PlaySoundParams(string.Empty, 1f, 1f, 1f, null, false, 0f, null, false, false);

        private AudioDistortionFilter _distFilter;
        private AudioEchoFilter _echoFilter;
        private AudioHighPassFilter _hpFilter;
        private AudioLowPassFilter _lpFilter;
        private AudioReverbFilter _reverbFilter;
        private AudioChorusFilter _chorusFilter;
        private float _maxVol = 1f;
        private int _instanceId = -1;
        private bool? _audioLoops;
        private int _maxLoops;
        private SoundGroupVariationUpdater _varUpdater;
        private int _previousSoundFinishedFrame = -1;
        private string _soundGroupName;

		/*! \endcond */

		/// <summary>
		/// This event will notify you when a Variation ends
		/// </summary>
		public delegate void SoundFinishedEventHandler();

        /// <summary>
        /// Subscribe to this event to be notified when the sound stops playing.
        /// </summary>
        public event SoundFinishedEventHandler SoundFinished;

        /// <summary>
        /// Used for the SoundLooped event you can subscribe to
        /// </summary>
        public delegate void SoundLoopedEventHandler(int loopNumberStarted);

        /// <summary>
        /// This event will notify you when a Variation loops (ends a loop and starts the same clip again via looping).
        /// </summary>
        public event SoundLoopedEventHandler SoundLooped;

		/*! \cond PRIVATE */

        private Transform _trans;
        private GameObject _go;
        private Transform _objectToFollow;
        private Transform _objectToTriggerFrom;
        private MasterAudioGroup _parentGroupScript;
        private bool _attachToSource;
        private string _resFileName = string.Empty;
        private bool _hasStartedEndLinkedGroups;


        public class PlaySoundParams {
            public string SoundType;
            public float VolumePercentage;
            public float? Pitch;
            public double? TimeToSchedulePlay;
            public Transform SourceTrans;
            public bool AttachToSource;
            public float DelaySoundTime;
            public bool IsChainLoop;
            public bool IsSingleSubscribedPlay;
            public float GroupCalcVolume;
            public bool IsPlaying;
            public PlaySoundParams(string soundType, float volPercent, float groupCalcVolume, float? pitch,
                Transform sourceTrans, bool attach, float delaySoundTime, double? timeToSchedulePlay, bool isChainLoop, bool isSingleSubscribedPlay) {
                SoundType = soundType;
                VolumePercentage = volPercent;
                GroupCalcVolume = groupCalcVolume;
                Pitch = pitch;
                SourceTrans = sourceTrans;
                AttachToSource = attach;
                DelaySoundTime = delaySoundTime;
                TimeToSchedulePlay = timeToSchedulePlay;
                IsChainLoop = isChainLoop;
                IsSingleSubscribedPlay = isSingleSubscribedPlay;
                IsPlaying = false;
            }
        }

        public enum PitchMode {
            None,
            Gliding
        }

        public enum FadeMode {
            None,
            FadeInOut,
            FadeOutEarly,
            GradualFade
        }

        public enum RandomPitchMode {
            AddToClipPitch,
            IgnoreClipPitch
        }

        public enum RandomVolumeMode {
            AddToClipVolume,
            IgnoreClipVolume
        }

        public enum DetectEndMode {
            None,
            DetectEnd
        }
        /*! \endcond */

        // ReSharper disable once UnusedMember.Local
        private void Awake() {
            original_pitch = VarAudio.pitch;
            original_volume = VarAudio.volume;
            _audioLoops = VarAudio.loop;
            var c = VarAudio.clip; // pre-warm the clip access
            var g = GameObj; // pre-warm the game object clip access

            if (c != null || g != null) { } // to disable the warning for not using it.

            if (VarAudio.playOnAwake) {
                Debug.LogWarning("The 'Play on Awake' checkbox in the Audio Source for Sound Group '" + ParentGroup.name +
                                 "', Variation '" + name +
                                 "' is checked. This is not used in Master Audio and can lead to buggy behavior. Make sure to uncheck it before hitting Play next time. To play ambient sounds, use an EventSounds component and activate the Start event to play a Sound Group of your choice.");
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void Start() {
            // this code needs to wait for cloning (for weight).
            var theParent = ParentGroup;
            if (theParent == null) {
                Debug.LogError("Sound Variation '" + name + "' has no parent!");
                return;
            }

            GameObj.layer = MasterAudio.Instance.gameObject.layer;

            switch (audLocation) {
                case MasterAudio.AudioLocation.FileOnInternet:
                    if (internetFileLoadStatus == MasterAudio.InternetFileLoadStatus.Loading) {
                        LoadInternetFile();
                    }
                    break;
            }

#if UNITY_5 || UNITY_2017_1_OR_NEWER
            SetMixerGroup();
            SetSpatialBlend();
#endif

            SetPriority();

            SetOcclusion();

            SpatializerHelper.TurnOnSpatializerIfEnabled(VarAudio);
        }

#if UNITY_5 || UNITY_2017_1_OR_NEWER
        public void SetMixerGroup() {
            var aBus = ParentGroup.BusForGroup;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (aBus != null) {
                VarAudio.outputAudioMixerGroup = aBus.mixerChannel;
            } else {
                VarAudio.outputAudioMixerGroup = null;
            }
        }

        public void SetSpatialBlend() {
            var blend = ParentGroup.SpatialBlendForGroup;
            if (blend != MasterAudioGroup.UseCurveSpatialBlend) {
                VarAudio.spatialBlend = blend;
            }

            var aBus = ParentGroup.BusForGroup;
            if (aBus != null && MasterAudio.Instance.mixerSpatialBlendType != MasterAudio.AllMixerSpatialBlendType.ForceAllTo2D && aBus.forceTo2D) {
                VarAudio.spatialBlend = 0;
            }
        }
#endif

        /*! \cond PRIVATE */
        public void LoadInternetFile() {
            StartCoroutine(AudioResourceOptimizer.PopulateSourceWithInternetFile(internetFileUrl, this, InternetFileLoaded, InternetFileFailedToLoad));
        }

        private void SetOcclusion() {
            VariationUpdater.UpdateCachedObjects();
            var doesGroupUseOcclusion = UsesOcclusion;

            if (!doesGroupUseOcclusion) {
                return;
            }

            // set occlusion default
            if (LowPassFilter == null) {
                _lpFilter = GetComponent<AudioLowPassFilter>();
                if (_lpFilter == null) {
                    _lpFilter = gameObject.AddComponent<AudioLowPassFilter>();
                }
            } else {
                _lpFilter = GetComponent<AudioLowPassFilter>();
            }

            // ReSharper disable once PossibleNullReferenceException
            LowPassFilter.cutoffFrequency = AudioUtil.MinCutoffFreq(VariationUpdater);
        }
        /*! \endcond */

        private void SetPriority() {
            if (!MasterAudio.Instance.prioritizeOnDistance) {
                return;
            }
            if (ParentGroup.alwaysHighestPriority) {
                AudioPrioritizer.Set2DSoundPriority(VarAudio);
            } else {
                AudioPrioritizer.SetSoundGroupInitialPriority(VarAudio);
            }
        }

		/*! \cond PRIVATE */
		/// <summary>
        /// Do not call this! It's called by Master Audio after it is  done initializing.
        /// </summary>
        public void DisableUpdater() {
            if (VariationUpdater == null) {
                return;
            }

            VariationUpdater.enabled = false;
        }
		/*! \endcond */

        // ReSharper disable once UnusedMember.Local
        private void OnDestroy() {
            StopSoundEarly();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDisable() {
            StopSoundEarly();
        }

        private void StopSoundEarly() {
            if (MasterAudio.AppIsShuttingDown) {
                return;
            }

            Stop(); // maybe unload clip from Resources
        }

        /*! \cond PRIVATE */
        /// <summary>
        /// Never call this method. Used internally.
        /// </summary>
        /// <param name="pitch">Pitch.</param>
        /// <param name="maxVolume">Max volume.</param>
        /// <param name="gameObjectName">Game object name.</param>
        /// <param name="volPercent">Vol percent.</param>
        /// <param name="targetVol">Target vol.</param>
        /// <param name="targetPitch">Target pitch.</param>
        /// <param name="sourceTrans">Source trans.</param>
        /// <param name="attach">If set to <c>true</c> attach.</param>
        /// <param name="delayTime">Delay time.</param>
        /// <param name="timeToSchedulePlay"><b>Optional</b> - used to pass in the DSP time to play the sound. Play now if null.</param>
        /// <param name="isChaining">If set to <c>true</c> is chaining.</param>
        /// <param name="isSingleSubscribedPlay">If set to <c>true</c> is single subscribed play.</param>
		public void Play(float? pitch, float maxVolume, string gameObjectName, float volPercent, float targetVol,
            float? targetPitch, Transform sourceTrans, bool attach, float delayTime,
            double? timeToSchedulePlay, bool isChaining, bool isSingleSubscribedPlay) {

            if (!MasterAudio.IsWarming && audLocation == MasterAudio.AudioLocation.FileOnInternet) {
                switch (internetFileLoadStatus) {
                    case MasterAudio.InternetFileLoadStatus.Loading:
                        if (ParentGroup.LoggingEnabledForGroup) {
                            MasterAudio.LogWarning("Cannot play Variation '" + name +
                                                   "' because its Internet file has not been downloaded yet.");
                        }
                        return;
                    case MasterAudio.InternetFileLoadStatus.Failed:
                        if (ParentGroup.LoggingEnabledForGroup) {
                            MasterAudio.LogWarning("Cannot play Variation '" + name +
                                                   "' because its Internet file failed downloading.");
                        }
                        return;
                }
            }

            MaybeCleanupFinishedDelegate();
            _hasStartedEndLinkedGroups = false;

            SetPlaySoundParams(gameObjectName, volPercent, targetVol, targetPitch, sourceTrans, attach, delayTime, timeToSchedulePlay, isChaining, isSingleSubscribedPlay);

            SetPriority(); // reset it back to normal priority in case you're playing 2D this time.

            if (MasterAudio.HasAsyncResourceLoaderFeature() && ShouldLoadAsync) {
                StopAllCoroutines(); // The only Coroutine right now requires pro version and Unity 4.5.3
            }

            // compute pitch
            if (pitch.HasValue) {
                VarAudio.pitch = pitch.Value;
            } else if (useRandomPitch) {
                var randPitch = Random.Range(randomPitchMin, randomPitchMax);

                switch (randomPitchMode) {
                    case RandomPitchMode.AddToClipPitch:
                        randPitch += OriginalPitch;
                        break;
                }

                VarAudio.pitch = randPitch;
            } else {
                // non random pitch
                VarAudio.pitch = OriginalPitch;
            }

#if UNITY_5 || UNITY_2017_1_OR_NEWER
            // in case it was changed at runtime.
            SetSpatialBlend();
#endif

            // set fade mode
            curFadeMode = FadeMode.None;
            curPitchMode = PitchMode.None;
            curDetectEndMode = DetectEndMode.DetectEnd;
            _maxVol = maxVolume;
            if (maxCustomLoops == minCustomLoops) {
                _maxLoops = minCustomLoops;
            } else {
                _maxLoops = Random.Range(minCustomLoops, maxCustomLoops + 1);
            }

            switch (audLocation) {
                case MasterAudio.AudioLocation.Clip:
                    FinishSetupToPlay();
                    return;
                case MasterAudio.AudioLocation.ResourceFile:
                    if (MasterAudio.HasAsyncResourceLoaderFeature() && ShouldLoadAsync) {
                        StartCoroutine(AudioResourceOptimizer.PopulateSourcesWithResourceClipAsync(ResFileName, this,
                            FinishSetupToPlay, ResourceFailedToLoad));
                    } else {
                        if (!AudioResourceOptimizer.PopulateSourcesWithResourceClip(ResFileName, this)) {
                            return; // audio file not found!
                        }

                        FinishSetupToPlay();
                    }
                    return;
                case MasterAudio.AudioLocation.FileOnInternet:
                    FinishSetupToPlay();
                    return;
            }
        }

        /// <summary>
        /// Never call this method. Used internally.
        /// </summary>
        /// <param name="gameObjectName">Game object name.</param>
        /// <param name="volPercent">Vol percent.</param>
        /// <param name="targetVol">Target vol.</param>
        /// <param name="targetPitch">Target pitch.</param>
        /// <param name="sourceTrans">Source trans.</param>
        /// <param name="attach">If set to <c>true</c> attach.</param>
        /// <param name="delayTime">Delay time.</param>
        /// <param name="timeToSchedulePlay"><b>Optional</b> - used to pass in the DSP time to play the sound. Play now if null.</param>
        /// <param name="isChaining">If set to <c>true</c> is chaining.</param>
        /// <param name="isSingleSubscribedPlay">If set to <c>true</c> is single subscribed play.</param>
        public void SetPlaySoundParams(string gameObjectName, float volPercent, float targetVol, float? targetPitch, Transform sourceTrans, bool attach, float delayTime, double? timeToSchedulePlay, bool isChaining, bool isSingleSubscribedPlay) {
            _playSndParam.SoundType = gameObjectName;
            _playSndParam.VolumePercentage = volPercent;
            _playSndParam.GroupCalcVolume = targetVol;
            _playSndParam.Pitch = targetPitch;
            _playSndParam.SourceTrans = sourceTrans;
            _playSndParam.AttachToSource = attach;
            _playSndParam.DelaySoundTime = delayTime;
            _playSndParam.TimeToSchedulePlay = timeToSchedulePlay;
            _playSndParam.IsChainLoop = isChaining ||
                ParentGroup.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain;
            _playSndParam.IsSingleSubscribedPlay = isSingleSubscribedPlay;
            _playSndParam.IsPlaying = true;
        }
        /*! \endcond */

        private void InternetFileFailedToLoad() {
            internetFileLoadStatus = MasterAudio.InternetFileLoadStatus.Failed;
        }

        private void MaybeCleanupFinishedDelegate() {
            if (ParentGroup.willCleanUpDelegatesAfterStop) {
                ClearSubscribers();
            }
        }

        private void InternetFileLoaded() {
            if (ParentGroup.LoggingEnabledForGroup) {
                MasterAudio.LogWarning("Internet file: '" + internetFileUrl + "' loaded successfully.");
            }

            internetFileLoadStatus = MasterAudio.InternetFileLoadStatus.Loaded;
        }

        private void ResourceFailedToLoad() {
            Stop(); // to stop other behavior and disable the Updater script.
        }

        private void FinishSetupToPlay() {
            if (!VarAudio.isPlaying && VarAudio.time > 0f) {
                // paused. Do nothing except Play
            } else if (useFades && (fadeInTime > 0f || fadeOutTime > 0f)) {
                fadeMaxVolume = _maxVol;

                if (fadeInTime > 0f) {
                    VarAudio.volume = 0f;
                }

                if (VariationUpdater != null) {
                    EnableUpdater(false);
                    VariationUpdater.FadeInOut();
                }
            }

            VarAudio.loop = AudioLoops;
            // restore original loop setting in case it got lost by loop setting code below for a previous play.

            if (_playSndParam.IsPlaying && (_playSndParam.IsChainLoop || _playSndParam.IsSingleSubscribedPlay || useRandomStartTime)) {
                VarAudio.loop = false;
            }

            if (!_playSndParam.IsPlaying) {
                return; // has already been "stop" 'd.
            }

            ParentGroup.AddActiveAudioSourceId(InstanceId);

            EnableUpdater(true);

            _attachToSource = false;

            var useClipAgePriority = MasterAudio.Instance.prioritizeOnDistance &&
                                     (MasterAudio.Instance.useClipAgePriority || ParentGroup.useClipAgePriority);

            if (!_playSndParam.AttachToSource && !useClipAgePriority) {
                return;
            }
            _attachToSource = _playSndParam.AttachToSource;

            if (VariationUpdater != null) {
                VariationUpdater.FollowObject(_attachToSource, ObjectToFollow, useClipAgePriority);
            }
        }

        /// <summary>
        /// This method allows you to jump to a specific time in an already playing or just triggered Audio Clip.
        /// </summary>
        /// <param name="timeToJumpTo">The time in seconds to jump to.</param>
        public void JumpToTime(float timeToJumpTo) {
            if (!_playSndParam.IsPlaying) {
                return;
            }

            VarAudio.time = timeToJumpTo;
        }

        /// <summary>
        /// This method allows you to slowly change to a new pitch.
        /// </summary>
        /// <param name="pitchAddition">The pitch to add to the current pitch.</param>
        /// <param name="glideTime">The time it will take to change to that pitch.</param>
        /// <param name="completionCallback">(Optional) - a method to execute when the pitch glide has completed.</param>
        public void GlideByPitch(float pitchAddition, float glideTime, System.Action completionCallback = null) {
            if (pitchAddition == 0) { // nothing to do
                if (completionCallback != null) {
                    completionCallback();
                }
                return;
            }

            var targetPitch = VarAudio.pitch + pitchAddition;

            if (targetPitch < -3f) {
                targetPitch = -3f;
            }

            if (targetPitch > 3f) {
                targetPitch = 3f;
            }

            if (!VarAudio.clip.IsClipReadyToPlay()) {
                if (ParentGroup.LoggingEnabledForGroup) {
                    MasterAudio.LogWarning("Cannot GlideToPitch Variation '" + name + "' because it is still loading.");
                }
                return;
            }

            if (glideTime <= MasterAudio.InnerLoopCheckInterval) {
                if (VarAudio.pitch != targetPitch) {
                    VarAudio.pitch = targetPitch; // time really short, just do it at once.
                }
                if (completionCallback != null) {
                    completionCallback();
                }
                return;
            }

            if (VariationUpdater != null) {
                VariationUpdater.GlidePitch(targetPitch, glideTime, completionCallback);
            }
        }

        /// <summary>
        /// This method allows you to adjust the volume of an already playing clip, accounting for bus volume, mixer volume and group volume.
        /// </summary>
        /// <param name="volumePercentage"></param>
        public void AdjustVolume(float volumePercentage) {
            if (!_playSndParam.IsPlaying) {
                return;
            }

            var newVol = _playSndParam.GroupCalcVolume * volumePercentage;
            VarAudio.volume = newVol;

            _playSndParam.VolumePercentage = volumePercentage;

            // SET LastVolumePercentage for the AudioInfo so a bus fade will work with respect to this value.
            var grpInfo = MasterAudio.GetAllVariationsOfGroup(ParentGroup.name);
            for (var i = 0; i < grpInfo.Count; i++) {
                var aVar = grpInfo[i];
                if (aVar.Variation != this) {
                    continue;
                }

                aVar.LastPercentageVolume = volumePercentage;
                break;
            }
        }

        /// <summary>
        /// This method allows you to pause the audio being played by this Variation. This is automatically called by MasterAudio.PauseSoundGroup and MasterAudio.PauseBus.
        /// </summary>
        public void Pause() {
            if (!MasterAudio.Instance.resourceClipsPauseDoNotUnload) {
                switch (audLocation) {
                    case MasterAudio.AudioLocation.ResourceFile:
                        Stop();
                        return;
                    case MasterAudio.AudioLocation.Clip:
                        if (!AudioUtil.AudioClipWillPreload(VarAudio.clip)) {
                            Stop();
                            return;
                        }
                        break;
                }
            }

            VarAudio.Pause();
            if (VariationUpdater.enabled) {
                VariationUpdater.Pause();
            }
            curFadeMode = FadeMode.None;
            curPitchMode = PitchMode.None;
        }

        /*! \cond PRIVATE */
        /// <summary>
        /// Do not call this method. Used internally for ver specific operations. This will set params for "silent" or randomly skipped variations so that a Chained Loop may continue when no audio is played.
        /// </summary>
        public void DoNextChain(float volumePercentage, float? pitch, Transform transActor, bool attach) {
            EnableUpdater(false);
            SetPlaySoundParams(ParentGroup.GameObjectName, volumePercentage, volumePercentage, pitch, transActor, attach, 0f, null, true, false);

            VariationUpdater.MaybeChain();
            VariationUpdater.StopWaitingForFinish();
        }

        public void PlayEndLinkedGroups(double? timeToPlayClip = null) {
            if (MasterAudio.AppIsShuttingDown || MasterAudio.IsWarming || ParentGroup.endLinkedGroups.Count == 0 || _hasStartedEndLinkedGroups) {
                return;
            }

            _hasStartedEndLinkedGroups = true;

            if (VariationUpdater == null || VariationUpdater.FramesPlayed == 0) {
                return;
            }

            switch (ParentGroup.linkedStopGroupSelectionType) {
                case MasterAudio.LinkedGroupSelectionType.All:
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < ParentGroup.endLinkedGroups.Count; i++) {
                        PlayEndLinkedGroup(ParentGroup.endLinkedGroups[i], timeToPlayClip);
                    }
                    break;
                case MasterAudio.LinkedGroupSelectionType.OneAtRandom:
                    var randomIndex = Random.Range(0, ParentGroup.endLinkedGroups.Count);
                    PlayEndLinkedGroup(ParentGroup.endLinkedGroups[randomIndex], timeToPlayClip);
                    break;
            }
        }

        /*! \endcond */

        private void EnableUpdater(bool waitForSoundFinish = true) {
            if (VariationUpdater != null) {
                VariationUpdater.enabled = true;
                VariationUpdater.Initialize();
                if (waitForSoundFinish) {
                    VariationUpdater.WaitForSoundFinish();
                }
            }
        }

        private void MaybeUnloadClip() {
            if (audLocation == MasterAudio.AudioLocation.ResourceFile) {
                AudioResourceOptimizer.UnloadClipIfUnused(_resFileName);
            }

            AudioUtil.UnloadNonPreloadedAudioData(VarAudio.clip, GameObj);
        }

        private void PlayEndLinkedGroup(string sType, double? timeToPlayClip = null) {
            if (_playSndParam.AttachToSource && _playSndParam.SourceTrans != null) {
                MasterAudio.PlaySound3DFollowTransformAndForget(sType, _playSndParam.SourceTrans, _playSndParam.VolumePercentage, _playSndParam.Pitch, 0, null, timeToPlayClip);
            } else if (_playSndParam.SourceTrans != null) {
                MasterAudio.PlaySound3DAtTransformAndForget(sType, _playSndParam.SourceTrans, _playSndParam.VolumePercentage, _playSndParam.Pitch, 0, null, timeToPlayClip);
            } else {
                MasterAudio.PlaySound3DAtVector3AndForget(sType, Trans.position, _playSndParam.VolumePercentage, _playSndParam.Pitch, 0, null, timeToPlayClip);
            }
        }

        /// <summary>
        /// This method allows you to stop the audio being played by this Variation. 
        /// </summary>
        /// <param name="stopEndDetection">Do not ever pass this in.</param>
        /// <param name="skipLinked">Do not ever pass this in.</param>
        public void Stop(bool stopEndDetection = false, bool skipLinked = false) {
            var waitStopped = false;

            if (stopEndDetection) {
                if (VariationUpdater != null) {
                    VariationUpdater.StopWaitingForFinish(); // turn off the chain loop endless repeat
                    waitStopped = true;
                }
            }

            if (!skipLinked) {
                PlayEndLinkedGroups();
            }

            _objectToFollow = null;
            _objectToTriggerFrom = null;

            VarAudio.pitch = OriginalPitch;
            ParentGroup.RemoveActiveAudioSourceId(InstanceId);
            MasterAudio.StopTrackingOcclusionForSource(GameObj);

            VarAudio.Stop();

            VarAudio.time = 0f;
            if (VariationUpdater != null) {
                VariationUpdater.StopFollowing();
                VariationUpdater.StopFading();
                VariationUpdater.StopPitchGliding();
            }

            if (!waitStopped) {
                if (VariationUpdater != null) {
                    VariationUpdater.StopWaitingForFinish();
                }
            }

            _playSndParam.IsPlaying = false;

            if (SoundFinished != null) {
                var willAbort = _previousSoundFinishedFrame == AudioUtil.FrameCount;
                _previousSoundFinishedFrame = AudioUtil.FrameCount;

                if (!willAbort) {
                    SoundFinished(); // parameters aren't used
                }

                MaybeCleanupFinishedDelegate();
            }

            Trans.localPosition = Vector3.zero;

            MaybeUnloadClip();
        }

        /// <summary>
        /// This method allows you to fade the sound from this Variation to a specified volume over X seconds.
        /// </summary>
        /// <param name="newVolume">The target volume to fade to.</param>
        /// <param name="fadeTime">The time it will take to fully fade to the target volume.</param>
        public void FadeToVolume(float newVolume, float fadeTime) {
            if (newVolume < 0f || newVolume > 1f) {
                Debug.LogError("Illegal volume passed to FadeToVolume: '" + newVolume + "'. Legal volumes are between 0 and 1.");
                return;
            }

            if (!VarAudio.clip.IsClipReadyToPlay()) {
                if (ParentGroup.LoggingEnabledForGroup) {
                    MasterAudio.LogWarning("Cannot Fade Variation '" + name + "' because it is still loading.");
                }
                return;
            }

            if (fadeTime <= MasterAudio.InnerLoopCheckInterval) {
                VarAudio.volume = newVolume; // time really short, just do it at once.
                if (VarAudio.volume <= 0f) {
                    Stop();
                }
                return;
            }

            if (VariationUpdater != null) {
                VariationUpdater.FadeOverTimeToVolume(newVolume, fadeTime);
            }
        }

        /// <summary>
        /// This method will fully fade out the sound from this Variation to zero using its existing fadeOutTime.
        /// </summary>
        public void FadeOutNow() {
            if (MasterAudio.AppIsShuttingDown) {
                return;
            }

            if (IsPlaying && useFades && VariationUpdater != null) {
                VariationUpdater.FadeOutEarly(fadeOutTime);
            }
        }

        /// <summary>
        /// This method will fully fade out the sound from this Variation to zero using over X seconds.
        /// </summary>
        /// <param name="fadeTime">The time it will take to fully fade to the target volume.</param>
        public void FadeOutNow(float fadeTime) {
            if (MasterAudio.AppIsShuttingDown) {
                return;
            }

            if (IsPlaying && VariationUpdater != null) {
                VariationUpdater.FadeOutEarly(fadeTime);
            }
        }

        /*! \cond PRIVATE */
        public bool WasTriggeredFromTransform(Transform trans) {
            if (ObjectToFollow == trans || ObjectToTriggerFrom == trans) {
                return true;
            }

            return false;
        }

        public bool WasTriggeredFromAnyOfTransformMap(HashSet<Transform> transMap) {
            if (ObjectToFollow != null && transMap.Contains(ObjectToFollow)) {
                return true;
            }

            if (ObjectToTriggerFrom != null && transMap.Contains(ObjectToTriggerFrom)) {
                return true;
            }

            return false;
        }

        /*! \endcond */

        /// <summary>
        /// This property returns you a lazy-loaded reference to the Unity Distortion Filter FX component.
        /// </summary>
        public AudioDistortionFilter DistortionFilter {
            get {
                if (_distFilter != null) {
                    return _distFilter;
                }
                _distFilter = GetComponent<AudioDistortionFilter>();

                return _distFilter;
            }
        }

        /// <summary>
        /// This property returns you a lazy-loaded reference to the Unity Reverb Filter FX component.
        /// </summary>
        public AudioReverbFilter ReverbFilter {
            get {
                if (_reverbFilter != null) {
                    return _reverbFilter;
                }
                _reverbFilter = GetComponent<AudioReverbFilter>();

                return _reverbFilter;
            }
        }

        /// <summary>
        /// This property returns you a lazy-loaded reference to the Unity Chorus Filter FX component.
        /// </summary>
        public AudioChorusFilter ChorusFilter {
            get {
                if (_chorusFilter != null) {
                    return _chorusFilter;
                }
                _chorusFilter = GetComponent<AudioChorusFilter>();

                return _chorusFilter;
            }
        }

        /// <summary>
        /// This property returns you a lazy-loaded reference to the Unity Echo Filter FX component.
        /// </summary>
        public AudioEchoFilter EchoFilter {
            get {
                if (_echoFilter != null) {
                    return _echoFilter;
                }
                _echoFilter = GetComponent<AudioEchoFilter>();

                return _echoFilter;
            }
        }

        /// <summary>
        /// This property returns you a reference to the Unity Low Pass Filter FX component.
        /// </summary>
        public AudioLowPassFilter LowPassFilter {
            get {
                return _lpFilter;
            }
            set {
                _lpFilter = value;
            }
        }

        /// <summary>
        /// This property returns you a lazy-loaded reference to the Unity High Pass Filter FX component.
        /// </summary>
        public AudioHighPassFilter HighPassFilter {
            get {
                if (_hpFilter != null) {
                    return _hpFilter;
                }
                _hpFilter = GetComponent<AudioHighPassFilter>();

                return _hpFilter;
            }
        }

        /*! \cond PRIVATE */
        public Transform ObjectToFollow {
            get { return _objectToFollow; }
            set {
                _objectToFollow = value;
                UpdateTransformTracker(value);
            }
        }

        public Transform ObjectToTriggerFrom {
            get { return _objectToTriggerFrom; }
            set {
                _objectToTriggerFrom = value;
                UpdateTransformTracker(value);
            }
        }

        public void UpdateTransformTracker(Transform sourceTrans) {
            if (sourceTrans == null) {
                return;
            }

            if (!Application.isEditor) { // no tracking outside of editor, because it creates garbage.
                return;
            }

            if (MasterAudio.IsWarming) {
                return;
            }

            if (sourceTrans.GetComponent<AudioTransformTracker>() == null) {
                sourceTrans.gameObject.AddComponent<AudioTransformTracker>();
            }
        }
        /*! \endcond */

        /// <summary>
        /// This property will return whether there are any Unity FX Filters enabled on this Variation.
        /// </summary>
        public bool HasActiveFXFilter {
            get {
                if (HighPassFilter != null && HighPassFilter.enabled) {
                    return true;
                }
                if (LowPassFilter != null && LowPassFilter.enabled) {
                    return true;
                }
                if (ReverbFilter != null && ReverbFilter.enabled) {
                    return true;
                }
                if (DistortionFilter != null && DistortionFilter.enabled) {
                    return true;
                }
                if (EchoFilter != null && EchoFilter.enabled) {
                    return true;
                }
                if (ChorusFilter != null && ChorusFilter.enabled) {
                    return true;
                }

                return false;
            }
        }

        /*! \cond PRIVATE */
        public MasterAudioGroup ParentGroup {
            get {
                if (Trans.parent == null) {
                    return null; // project view
                }

                if (_parentGroupScript == null) {
                    _parentGroupScript = Trans.parent.GetComponent<MasterAudioGroup>();
                }

                if (_parentGroupScript == null) {
                    Debug.LogError("The Group that Sound Variation '" + name +
                                   "' is in does not have a MasterAudioGroup script in it!");
                }

                return _parentGroupScript;
            }
        }
        /*! \endcond */

        /// <summary>
        /// This property will return the original pitch of the Variation.
        /// </summary>
        public float OriginalPitch {
            get {
                if (original_pitch == 0f) {
                    // lazy lookup for race conditions.
                    original_pitch = VarAudio.pitch;
                }

                return original_pitch;
            }
        }

        /// <summary>
        /// This property will return the original volume of the Variation.
        /// </summary>
        public float OriginalVolume {
            get {
                if (original_volume == 0f) {
                    // lazy lookup for race conditions.
                    original_volume = VarAudio.volume;
                }

                return original_volume;
            }
        }

        /// <summary>
        /// This returns the name of the Sound Group the Variation belongs to.
        /// </summary>
        public string SoundGroupName {
            get {
                if (_soundGroupName != null) {
                    return _soundGroupName;
                }

                _soundGroupName = ParentGroup.GameObjectName;
                return _soundGroupName;
            }
        }

        /*! \cond PRIVATE */
        public bool IsAvailableToPlay {
            get {
                if (weight == 0) {
                    return false;
                }

                if (!_playSndParam.IsPlaying && VarAudio.time == 0f) {
                    return true; // paused aren't available
                }

                return AudioUtil.GetAudioPlayedPercentage(VarAudio) >= ParentGroup.retriggerPercentage;
            }
        }
        /*! \endcond */

        /// <summary>
        /// This property will return the time of the last play of this Variation.
        /// </summary>
        public float LastTimePlayed { get; set; }

        /// <summary>
        /// This returns whether the clip is playing or not.
        /// </summary>
        public bool IsPlaying {
            get { return _playSndParam.IsPlaying; }
        }

        /*! \cond PRIVATE */
        public int InstanceId {
            get {
                if (_instanceId < 0) {
                    _instanceId = GetInstanceID();
                }

                return _instanceId;
            }
        }

        public Transform Trans {
            get {
                if (_trans != null) {
                    return _trans;
                }
                _trans = transform;

                return _trans;
            }
        }

        public GameObject GameObj {
            get {
                if (_go != null) {
                    return _go;
                }
                _go = gameObject;

                return _go;
            }
        }

        public AudioSource VarAudio {
            get {
                if (_audioSource != null) {
                    return _audioSource;
                }

                _audioSource = GetComponent<AudioSource>();

                return _audioSource;
            }
        }

        public bool AudioLoops {
            get {
                if (!_audioLoops.HasValue) {
                    _audioLoops = VarAudio.loop;
                }

                return _audioLoops.Value;
            }
        }

        public string ResFileName {
            get {
                if (string.IsNullOrEmpty(_resFileName)) {
                    _resFileName = AudioResourceOptimizer.GetLocalizedFileName(useLocalization, resourceFileName);
                }

                return _resFileName;
            }
        }

        public SoundGroupVariationUpdater VariationUpdater {
            get {
                if (_varUpdater != null) {
                    return _varUpdater;
                }

                _varUpdater = GetComponent<SoundGroupVariationUpdater>();

                return _varUpdater;
            }
        }

        public PlaySoundParams PlaySoundParm {
            get { return _playSndParam; }
        }

        public float SetGroupVolume {
            get { return _playSndParam.GroupCalcVolume; }
            set { _playSndParam.GroupCalcVolume = value; }
        }

        public int MaxLoops {
            get { return _maxLoops; }
        }

        private bool Is2D {
            get {
#if UNITY_5 || UNITY_2017_1_OR_NEWER
                return VarAudio.spatialBlend <= 0;
#else
				return false;
#endif
            }
        }

        private bool ShouldLoadAsync {
            get {
                if (MasterAudio.Instance.resourceClipsAllLoadAsync) {
                    return true;
                }

                return ParentGroup.resourceClipsAllLoadAsync;
            }
        }

        public bool UsesOcclusion {
            get {
                if (!VariationUpdater.MAThisFrame.useOcclusion) {
                    return false;
                }

                if (Is2D) {
                    return false;
                }

                switch (VariationUpdater.MAThisFrame.occlusionSelectType) {
                    default:
                    case MasterAudio.OcclusionSelectionType.AllGroups:
                        return true;
                    case MasterAudio.OcclusionSelectionType.TurnOnPerBusOrGroup:
                        if (ParentGroup.isUsingOcclusion) {
                            return true;
                        }

                        var theBus = ParentGroup.BusForGroup;
                        if (theBus != null && theBus.isUsingOcclusion) {
                            return true;
                        }

                        return false;
                }
            }
        }

        public void SoundLoopStarted(int numberOfLoops) {
            if (SoundLooped == null) {
                return;
            }

            this.SoundLooped(numberOfLoops);
        }

        public void ClearSubscribers() {
            SoundFinished = null; // clear it out so subscribers don't have to clean up
            SoundLooped = null;
        }
        /*! \endcond */
    }
}
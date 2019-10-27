//----------------------------------------------
//            	   Koreographer                 
//    Copyright © 2014-2016 Sonic Bloom, LLC    
//----------------------------------------------

using UnityEngine;
using HutongGames.PlayMaker;
using SonicBloom.Koreo.Players;
using Tooltip=HutongGames.PlayMaker.TooltipAttribute;

namespace SonicBloom.Koreo.PlayMaker
{
	[ActionCategory("Koreographer")]
	[Note("If any Koreography and/or AudioClip objects are specified the \"Playing\" event will only be triggered if ALL of them are playing.")]
	[Tooltip("Sends an event based on the playing state of a MultiMusicPlayer.  (For purposes of this test, \"Paused\" is not playing.)  If a Koreography is specified, the check will see if the AudioClip it references is playing.")]
	public class MultiMusicPlayerIsPlaying : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(MultiMusicPlayer))]
		[Tooltip("The GameObject that hosts the MultiMusicPlayer component to stop.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event sent if MultiMusicPlayer is playing the Koreography.")]
		public FsmEvent playing;

		[Tooltip("Event sent if MultiMusicPlayer is not playing.")]
		public FsmEvent notPlaying;

		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		[ActionSection("Optional")]
		
		[UIHint(UIHint.Variable)]
		public FsmBool store;

		[Tooltip("A list of Koreography objects for which to check the playback state.")]
		[ObjectType(typeof(Koreography))]
		public FsmObject[] koreographies;

		[Tooltip("A list of AudioClip objects for which to check the playback state.")]
		[ObjectType(typeof(AudioClip))]
		public FsmObject[] audioClips;

		public override void Reset()
		{
			gameObject = null;
			playing = null;
			notPlaying = null;
			everyFrame = false;
			store = null;
			koreographies = null;
			audioClips = null;
		}
		
		public override void OnEnter()
		{
			DoIsPlaying();

			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoIsPlaying();
		}

		void DoIsPlaying()
		{
			if (gameObject != null)
			{
				GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
				
				if (go != null)
				{
					MultiMusicPlayer player = go.GetComponent<MultiMusicPlayer>();
					
					if (player != null)
					{
						bool bIsPlaying = true;

						if ((koreographies == null || koreographies.Length == 0) &&
						    (audioClips == null || audioClips.Length == 0))
						{
							// Basic IsPlaying check.
							bIsPlaying = player.GetIsPlaying(player.GetCurrentClipName());
						}
						else
						{
							if (koreographies != null)
							{
								for (int i = 0; i < koreographies.Length; ++i)
								{
									Koreography koreo = koreographies[i].Value as Koreography;
									if (koreo == null)
									{
										LogWarning("Koreography variable was null!  Triggering the \"Not Playing\" event.");
										bIsPlaying = false;
										break;
									}
									else if (!player.GetIsPlaying(koreo.SourceClipName))
									{
										bIsPlaying = false;
										break;
									}
								}
							}

							if (bIsPlaying && audioClips != null)
							{
								for (int i = 0; i < audioClips.Length; ++i)
								{
									AudioClip clip = audioClips[i].Value as AudioClip;
									if (clip == null)
									{
										LogWarning("AudioClip variable was null!  Triggering the \"Not Playing\" event.");
										bIsPlaying = false;
										break;
									}
									if (!player.GetIsPlaying(clip.name))
									{
										bIsPlaying = false;
										break;
									}
								}
							}
						}

						store.Value = bIsPlaying;

						if (bIsPlaying)
						{
							Fsm.Event(playing);
						}
						else
						{
							Fsm.Event(notPlaying);
						}
					}
					else
					{
						LogWarning("No MultiMusicPlayer on GameObject!");
					}
				}
			}
		}

		public override string ErrorCheck()
		{
			string bErrorStr = string.Empty;
			if (!everyFrame && 
			    FsmEvent.IsNullOrEmpty(playing) &&
			    FsmEvent.IsNullOrEmpty(notPlaying))
			{
				bErrorStr = "Action sends no events!";
			}
			return bErrorStr;
		}
	}
}

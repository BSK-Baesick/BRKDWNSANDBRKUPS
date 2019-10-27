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
	[Tooltip("Sends an event based on the playing state of a SimpleMusicPlayer.  (For purposes of this test, \"Paused\" is not playing.)  If a Koreography is specified, the check will see if that specific audio is playing.")]
	public class SimpleMusicPlayerIsPlaying : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(SimpleMusicPlayer))]
		[Tooltip("The GameObject that hosts the SimpleMusicPlayer component to stop.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Event sent if SimpleMusicPlayer is playing.")]
		public FsmEvent playing;

		[Tooltip("Event sent if SimpleMusicPlayer is not playing.")]
		public FsmEvent notPlaying;

		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		[ActionSection("Optional")]
		
		[UIHint(UIHint.Variable)]
		public FsmBool store;

		[Tooltip("A specific Koreography to check playback state of.  If not loaded, will trigger the \"Not Playing\" event.")]
		[ObjectType(typeof(Koreography))]
		public FsmObject koreography;

		public override void Reset()
		{
			gameObject = null;
			playing = null;
			notPlaying = null;
			everyFrame = false;
			store = null;
			koreography = null;
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
					SimpleMusicPlayer player = go.GetComponent<SimpleMusicPlayer>();
					
					if (player != null)
					{
						bool bIsPlaying = false;
						if (koreography != null && koreography.Value != null)
						{
							Koreography koreo = koreography.Value as Koreography;
							bIsPlaying = player.GetIsPlaying(koreo.SourceClipName);
						}
						else
						{
							bIsPlaying = player.GetIsPlaying(player.GetCurrentClipName());
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
						LogWarning("No SimpleMusicPlayer on GameObject!");
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

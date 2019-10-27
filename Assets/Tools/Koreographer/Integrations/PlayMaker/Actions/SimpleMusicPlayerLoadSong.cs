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
	[Tooltip("Loads Koreography into a SimpleMusicPlayer for playback.  Optionally begins playback of the audio specified by the new Koreography.  This will stop and unload any previously playing Koreography.")]
	public class SimpleMusicPlayerLoadSong : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(SimpleMusicPlayer))]
		[Tooltip("The GameObject that hosts the SimpleMusicPlayer component to play.")]
		public FsmOwnerDefault gameObject;
		
		[Tooltip("The Koreography to Load for playback.")]
		[ObjectType(typeof(Koreography))]
		public FsmObject koreography;

		[Title("Start Playback Immediately")]
		[Tooltip("The time at which to begin playing back the specified Koreography.  Specify 0 to start at the beginning.")]
		public FsmBool autoStart;

		[Tooltip("The time at which to begin playing back the specified Koreography.  Specify 0 to start at the beginning.")]
		public FsmInt startSampleTime;
		
		public override void Reset()
		{
			gameObject = null;
			koreography = null;
			autoStart = true;
			startSampleTime = 0;
		}
		
		public override void OnEnter()
		{
			DoLoadSong();
			Finish();
		}

		void DoLoadSong()
		{
			if (gameObject != null)
			{
				GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
				
				if (go != null)
				{
					SimpleMusicPlayer player = go.GetComponent<SimpleMusicPlayer>();
					
					if (player != null)
					{
						if (koreography != null && koreography.Value != null)
						{
							player.LoadSong(koreography.Value as Koreography, startSampleTime.Value, autoStart.Value);
						}
						else
						{
							LogWarning("No Koreography to Load into SimpleMusicPlayer!");
						}
					}
					else
					{
						LogWarning("No SimpleMusicPlayer on GameObject!");
					}
				}
			}
		}
	}
}

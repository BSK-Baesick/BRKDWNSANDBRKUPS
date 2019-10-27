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
	[Tooltip("Seeks the playhead of a SimpleMusicPlayer, causing any playing or paused audio to jump.")]
	public class SimpleMusicPlayerSeekToSample : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(SimpleMusicPlayer))]
		[Tooltip("The GameObject that hosts the SimpleMusicPlayer component to stop.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("The sample position to which to seek.  0 is the beginning of the audio.")]
		public FsmInt targetSample;

		public override void Reset()
		{
			gameObject = null;
			targetSample = 0;
		}
		
		public override void OnEnter()
		{
			DoSeek();
			Finish();
		}

		void DoSeek()
		{
			if (gameObject != null)
			{
				GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
				
				if (go != null)
				{
					SimpleMusicPlayer player = go.GetComponent<SimpleMusicPlayer>();
					
					if (player != null)
					{
						player.SeekToSample(targetSample.Value);
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

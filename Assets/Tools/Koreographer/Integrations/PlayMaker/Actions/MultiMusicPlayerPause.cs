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
	[Tooltip("Pauses a MultiMusicPlayer.")]
	public class MultiMusicPlayerPause : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(MultiMusicPlayer))]
		[Tooltip("The GameObject that hosts the MultiMusicPlayer component to pause.")]
		public FsmOwnerDefault gameObject;

		public override void Reset()
		{
			gameObject = null;
		}
		
		public override void OnEnter()
		{
			DoPause();
			Finish();
		}

		void DoPause()
		{
			if (gameObject != null)
			{
				GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
				
				if (go != null)
				{
					MultiMusicPlayer player = go.GetComponent<MultiMusicPlayer>();
					
					if (player != null)
					{
						player.Pause();
					}
					else
					{
						LogWarning("No MultiMusicPlayer on GameObject!");
					}
				}
			}
		}
	}
}

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
	[Tooltip("Stops a MultiMusicPlayer.")]
	public class MultiMusicPlayerStop : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(MultiMusicPlayer))]
		[Tooltip("The GameObject that hosts the MultiMusicPlayer component to stop.")]
		public FsmOwnerDefault gameObject;

		public override void Reset()
		{
			gameObject = null;
		}
		
		public override void OnEnter()
		{
			DoStop();
			Finish();
		}

		void DoStop()
		{
			if (gameObject != null)
			{
				GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
				
				if (go != null)
				{
					MultiMusicPlayer player = go.GetComponent<MultiMusicPlayer>();
					
					if (player != null)
					{
						player.Stop();
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

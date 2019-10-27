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
	[Tooltip("Pauses a SimpleMusicPlayer.")]
	public class SimpleMusicPlayerPause : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(SimpleMusicPlayer))]
		[Tooltip("The GameObject that hosts the SimpleMusicPlayer component to pause.")]
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
					SimpleMusicPlayer player = go.GetComponent<SimpleMusicPlayer>();
					
					if (player != null)
					{
						player.Pause();
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

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
	[Tooltip("Causes a MultiMusicPlayer to play.  You may want to consider unchecking the \"Auto Play On Awake\" checkbox on the MultiMusicPlayer component if you plan to control initial playback later.")]
	public class MultiMusicPlayerPlay : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(MultiMusicPlayer))]
		[Tooltip("The GameObject that hosts the MultiMusicPlayer component to play.")]
		public FsmOwnerDefault gameObject;

		// TODO: Add support for MusicLayer loading.  This will likely require new functionality from PlayMaker.
		
		public override void Reset()
		{
			gameObject = null;
		}
		
		public override void OnEnter()
		{
			DoPlay();
			Finish();
		}

		void DoPlay()
		{
			if (gameObject != null)
			{
				GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
				
				if (go != null)
				{
					MultiMusicPlayer player = go.GetComponent<MultiMusicPlayer>();
					
					if (player != null)
					{
						player.Play();
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

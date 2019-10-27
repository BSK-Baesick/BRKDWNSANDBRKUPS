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
	[Tooltip("Causes a SimpleMusicPlayer to play.  If a Koreography is specified, it will first load that Koreography.  You may want to consider unchecking the \"Auto Play On Awake\" checkbox on the SimpleMusicPlayer component if you plan to control initial playback later.")]
	public class SimpleMusicPlayerPlay : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(SimpleMusicPlayer))]
		[Tooltip("The GameObject that hosts the SimpleMusicPlayer component to play.")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Optional")]

		[Tooltip("A specific Koreography to Load for playback.")]
		[ObjectType(typeof(Koreography))]
		public FsmObject koreography;

		[Tooltip("The time at which to begin playing back the specified Koreography.  WARNING: This value only has an effect if you specify a Koreography in this Action.")]
		public FsmInt startSampleTime;
		
		public override void Reset()
		{
			gameObject = null;
			koreography = null;
			startSampleTime = 0;
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
					SimpleMusicPlayer player = go.GetComponent<SimpleMusicPlayer>();
					
					if (player != null)
					{
						if (koreography != null && koreography.Value != null)
						{
							player.LoadSong(koreography.Value as Koreography, startSampleTime.Value, true);
						}
						else
						{
							player.Play();
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

using DarkTonic.MasterAudio;
using UnityEngine;
using HutongGames.PlayMaker;
using TooltipAttribute = HutongGames.PlayMaker.TooltipAttribute;

[ActionCategory(ActionCategory.Audio)]
[Tooltip("Fire a Custom Event in Master Audio")]
public class MasterAudioFireCustomEvent : FsmStateAction {
	[RequiredField]
    [Tooltip("The Custom Event (defined in Master Audio prefab) to fire.")]
    public FsmString customEventName;
	
    [Tooltip("The origin object of the Custom Event")]	
	public FsmOwnerDefault eventOrigin;

	public override void OnEnter() {
		Transform trans;
		if (eventOrigin.GameObject.Value != null) {
			trans = eventOrigin.GameObject.Value.transform;
		} else {
			trans = Owner.transform;
		}

		MasterAudio.FireCustomEvent(customEventName.Value, trans);
		
		Finish();
	}
	
	public override void Reset() {
		customEventName = new FsmString(string.Empty);
		eventOrigin = null;
	}
}

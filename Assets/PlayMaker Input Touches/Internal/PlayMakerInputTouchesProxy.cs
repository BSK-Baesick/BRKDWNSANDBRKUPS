using UnityEngine;
using System.Collections;
using HutongGames;


public class PlayMakerInputTouchesProxy : MonoBehaviour {
	
	
	
	#region event data
	
	static public Vector2 LastEventPosition { get; private set; }
	
	static public float LastEventMagnitude { get; private set; }
	
	static public DragInfo LastDragInfo { get; private set; }
	
	static public ChargedInfo LastChargedInfo { get; private set; }
	
	static public SwipeInfo LastSwipeInfo { get; private set; }

	static public PinchInfo LastPinchInfo { get; private set; }
	
	static public RotateInfo LastRotateInfo { get; private set; }
	
	static public Tap LastTapInfo { get; private set; }
	
	#endregion
	
	public int SwipeAngleThreshold = 10;
	
	public bool debug = true;
	
	#region raw touch events
	[HideInInspector]
	public bool On;
	
	[SerializeField]
	private bool _isOn;
	public bool isOn
	{
		get{
			return _isOn;
		}
		set{
			_isOn = value;
			if (value)
			{
				IT_Gesture.onTouchE += OnOn;
			}else{
				IT_Gesture.onTouchE -= OnOn;
			}
		}
	}
	
	[HideInInspector]
	public bool Up;
	
	[SerializeField]
	private bool _isUp;
	public bool isUp
	{
		get{
			return _isUp;
		}
		set{
			_isUp = value;
			if (_isUp)
			{
				IT_Gesture.onTouchUpE += OnUp;
			}else{
				IT_Gesture.onTouchUpE -= OnUp;
			}
		}
	}
	
	[HideInInspector]
	public bool Down;
	
	[SerializeField]
	private bool _isDown;
	public bool isDown
	{
		get{
			return _isDown;
		}
		set{
			_isDown = value;
			if (_isDown)
			{
				IT_Gesture.onTouchDownE += OnDown;
			}else{
				IT_Gesture.onTouchDownE -= OnDown;
			}
		}
	}
	
	#endregion
	
	#region gestures events
	
	[HideInInspector]
	public bool Swipe;
	
	[SerializeField]
	private bool _isSwipe;
	public bool isSwipe
	{
		get{
			return _isSwipe;
		}
		set{
			_isSwipe = value;
			if (_isSwipe)
			{
				IT_Gesture.onSwipeE += OnSwipe;
			}else{
				IT_Gesture.onSwipeE -= OnSwipe;
			}
		}
	}	

	[HideInInspector]
	public bool Pinch;
	
	[SerializeField]
	private bool _isPinch;
	public bool isPinch
	{
		get{
			return _isPinch;
		}
		set{
			_isPinch = value;
			if (_isPinch)
			{
				IT_Gesture.onPinchE += OnPinch;
			}else{
				IT_Gesture.onPinchE -= OnPinch;
			}
		}
	}	

	[HideInInspector]
	public bool Rotate;
	
	[SerializeField]
	private bool _isRotate;
	public bool isRotate
	{
		get{
			return _isRotate;
		}
		set{
			_isRotate = value;
			if (_isRotate)
			{
				IT_Gesture.onRotateE += OnRotate;
			}else{
				IT_Gesture.onRotateE -= OnRotate;
			}
		}
	}	
	

	[HideInInspector]
	public bool Charging;
	
	[SerializeField]
	private bool _isCharging;
	public bool isCharging
	{
		get{
			return _isCharging;
		}
		set{
			_isCharging = value;
			if (_isCharging)
			{
				IT_Gesture.onChargingE += OnCharging;
				IT_Gesture.onChargeStartE += OnChargeStart;
				IT_Gesture.onChargeEndE += OnChargeEnd;
			}else{
				IT_Gesture.onChargingE -= OnCharging;
				IT_Gesture.onChargeStartE -= OnChargeStart;
				IT_Gesture.onChargeEndE -= OnChargeEnd;
			}
		}
	}	
	
	
	[HideInInspector]
	public bool Dragging;
	
	[SerializeField]
	private bool _isDragging;
	public bool isDragging
	{
		get{
			return _isDragging;
		}
		set{
			Debug.Log ("isDragging "+value);
			
			_isDragging = value;
			
			if ( Application.isPlaying )
			{
				if (_isDragging)
				{
					
					Debug.Log("binding to Dragging gestures delegates");
					IT_Gesture.onDraggingE += OnDragging;
					IT_Gesture.onDraggingStartE += OnDraggingStart;
					IT_Gesture.onDraggingEndE += OnDraggingEnd;
					
				}else{
					IT_Gesture.onDraggingE -= OnDragging;
					IT_Gesture.onDraggingStartE -= OnDraggingStart;
					IT_Gesture.onDraggingEndE -= OnDraggingEnd;
				}
			}
		}
	}	

	[HideInInspector]
	public bool MultiFingersCharging;
	
	[SerializeField]
	private bool _isMultiFingersCharging;
	public bool isMultiFingersCharging
	{
		get{
			return _isMultiFingersCharging;
		}
		set{
			_isMultiFingersCharging = value;
			if (_isMultiFingersCharging)
			{
				IT_Gesture.onMFChargingE += OnMFCharging;
				IT_Gesture.onMFChargeStartE +=OnMFChargeStart;
				IT_Gesture.onMFChargeEndE += OnMFChargeEnd;
			}else{
				IT_Gesture.onMFChargingE -= OnMFCharging;
				IT_Gesture.onMFChargeStartE -=OnMFChargeStart;
				IT_Gesture.onMFChargeEndE -=OnMFChargeEnd;
			}
		}
	}	
	
	
	[HideInInspector]
	public bool MultiFingersDragging;
	
	[SerializeField]
	private bool _isMultiFingersDragging;
	public bool isMultiFingersDragging
	{
		get{
			return _isMultiFingersDragging;
		}
		set{
			_isMultiFingersDragging = value;
			if (_isMultiFingersDragging)
			{
				IT_Gesture.onMFDraggingE += OnMFDragging;
				IT_Gesture.onMFDraggingStartE += OnMFDraggingStart;
				IT_Gesture.onMFDraggingEndE += OnMFDraggingEnd;
			}else{
				IT_Gesture.onMFDraggingE -= OnMFDragging;
				IT_Gesture.onMFDraggingStartE -= OnMFDraggingStart;
				IT_Gesture.onMFDraggingEndE -= OnMFDraggingEnd;
			}
		}
	}		
	#endregion
	
	
	#region Tap touch events
	
	[HideInInspector]
	public bool MultiTap;
	
	[SerializeField]
	private bool _isMultiTap;
	public bool isMultiTap
	{
		get{
			return _isMultiTap;
		}
		set{
			_isMultiTap = value;
			if (_isMultiTap)
			{
				IT_Gesture.onMultiTapE += OnMultiTap;
			}else{
				IT_Gesture.onMultiTapE -= OnMultiTap;
			}
		}
	}	
	
	[HideInInspector]
	public bool LongTap;
	
	[SerializeField]
	private bool _isLongTap;
	public bool isLongTap
	{
		get{
			return _isLongTap;
		}
		set{
			_isLongTap = value;
			if (_isLongTap)
			{
				IT_Gesture.onLongTapE += OnLongTap;
			}else{
				IT_Gesture.onLongTapE -= OnLongTap;
			}
		}
	}	
	
	
	[HideInInspector]
	public bool MultiFingersMultiTap;
	
	[SerializeField]
	private bool _isMultiFingersMultiTap;
	public bool isMultiFingersMultiTap
	{
		get{
			return _isMultiFingersMultiTap;
		}
		set{
			_isMultiFingersMultiTap = value;
			if (_isMultiFingersMultiTap)
			{
				IT_Gesture.onMFMultiTapE += OnMFMultiTap;
			}else{
				IT_Gesture.onMFMultiTapE -= OnMFMultiTap;
			}
		}
	}	
	
	[HideInInspector]
	public bool MultiFingersLongTap;
	
	[SerializeField]
	private bool _isMultiFingersLongTap;
	public bool isMultiFingersLongTap
	{
		get{
			return _isMultiFingersLongTap;
		}
		set{
			_isMultiFingersLongTap = value;
			if (_isMultiFingersLongTap)
			{
				IT_Gesture.onMFLongTapE += OnMFLongTap;
			}else{
				IT_Gesture.onMFLongTapE -= OnMFLongTap;
			}
		}
	}	

	
	#endregion
	 
	
	
	/// <summary>
	/// Force the initialization of all listeners
	/// </summary>
	void Start()
	{
		
		isOn = On;
		isDown = Down;
		isUp = Up;
		
		isMultiTap = MultiTap;
		isMultiFingersMultiTap = MultiFingersMultiTap;
		isLongTap = LongTap;
		isMultiFingersLongTap = MultiFingersLongTap;
		
		
		isCharging = Charging;
		isDragging = Dragging;
		
		
		isMultiFingersCharging = MultiFingersCharging;
		isMultiFingersDragging = MultiFingersDragging;
		
		isSwipe = Swipe;
		isPinch = Pinch;
		isRotate = Rotate;
		
		
	}

	
	#region RAW
	void OnOn(Touch touch)
	{
		if (debug)
		{
			Debug.Log ("on "+touch.position);
		}

		LastEventPosition = touch.position;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / ON");
	}
	
	void OnUp(Touch touch)
	{
		if (debug)
		{
			Debug.Log ("up "+touch.position);
		}
		
		LastEventPosition = touch.position;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / UP");
	}
	
	void OnDown(Touch touch)
	{
		if (debug)
		{
			Debug.Log ("down "+touch.position);
		}
		
		LastEventPosition = touch.position;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / DOWN");
	}
	
	#endregion
	
	
	#region GESTURES
	
	/// <summary>
	/// fired when a swipe is detected.
	/// </summary>
	/// <param name='sw'>
	/// Sw.
	/// </param>
	void OnSwipe(SwipeInfo sw)
	{
		if (debug)
		{
			Debug.Log(sw.angle);
		}
		LastEventPosition = sw.endPoint;
		
		LastSwipeInfo = sw;
			
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / SWIPE");
		
		if (sw.angle>=0)
		{
			if (sw.angle<SwipeAngleThreshold || sw.angle>(360-SwipeAngleThreshold))
			{
				if (debug)
				{
					Debug.Log ("swipe right");
				}
				
				PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / SWIPE RIGHT");
				
			}else if (sw.angle<(90+SwipeAngleThreshold) && sw.angle>(90-SwipeAngleThreshold))
			{
				if (debug)
				{
					Debug.Log ("swipe up");
				}
				PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / SWIPE UP");
				
			}else if (sw.angle<(180+SwipeAngleThreshold) && sw.angle>(180-SwipeAngleThreshold))
			{
				if (debug)
				{
					Debug.Log ("swipe left");	
				}
				PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / SWIPE LEFT");
			}else if (sw.angle<(270+SwipeAngleThreshold) && sw.angle>(270-SwipeAngleThreshold))
			{
				if (debug)
				{
					Debug.Log ("swipe down");
				}
				PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / SWIPE DOWN");
			}
		}else{
			if (debug)
			{
				Debug.Log ("oups, angle is negative");
			}
			
			if (sw.angle<(-90+SwipeAngleThreshold) && sw.angle>(-90-SwipeAngleThreshold))
			{
				if (debug)
				{
					Debug.Log ("swipe down");
				}
				PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / SWIPE DOWN");
			}
			
		}
		
				
	}
	
	/// <summary>
	/// Fired when a pinch event is detected. 
	/// </summary>
	/// <param name='pinchInfo'> 
	/// The magnitude of the pinch and the touch pos. The value passed is +ve if the pinch is inward pinch, -ve if outward pinch.
	/// </param>
	void OnPinch(PinchInfo pinchinfo)
	{
		if (debug)
		{
			Debug.Log ("pinch "+pinchinfo.magnitude);
		}
		
		LastPinchInfo = pinchinfo;
		LastEventMagnitude = pinchinfo.magnitude;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / PINCH");
	}
	
	/// <summary>
	/// Fired when a 2 fingers rotate gesture is detected. 
	/// </summary>
	/// <param name='rotateInfo'>
	/// The magnitude of the rotation and touch pos. When rotating direction is clockwise, the value is -ve. Otherwise it's +ve.
	/// </param>
	void OnRotate(RotateInfo rotateInfo)
	{
		if (debug)
		{
			Debug.Log ("rotate "+ rotateInfo.magnitude);
		}
		
		LastRotateInfo = rotateInfo;
		LastEventMagnitude = rotateInfo.magnitude;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / ROTATE");
	}
	
	
	/// <summary>
	/// when a holding tap is detected.
	/// </summary>
	/// <param name='chargeInfo'>
	/// The screen position where the event take place and the amount charged is passed.
	/// </param>
	void OnCharging(ChargedInfo chargeInfo)
	{
		if (debug)
		{
			Debug.Log ("Charging "+chargeInfo.percent +" "+chargeInfo.pos);
		}
		LastEventPosition = chargeInfo.pos;
		
		LastEventMagnitude = chargeInfo.percent;
		
		LastChargedInfo = chargeInfo;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / CHARGING");
	}
	
	/// <summary>
	/// fired when a charging tap is first detected. 
	/// </summary>
	/// <param name='chargeInfo'>
	/// The screen position where the event take place and the amount charged is passed.
	/// </param>
	void OnChargeStart(ChargedInfo chargeInfo)
	{
		if (debug)
		{
			Debug.Log ("Charging start "+chargeInfo.percent +" "+chargeInfo.pos);
		}
		
		LastEventPosition = chargeInfo.pos;
		
		LastEventMagnitude = chargeInfo.percent;
		
		LastChargedInfo = chargeInfo;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / CHARGING START");
	}	
	
	/// <summary>
	/// fired when a charging tap is released. 
	/// </summary>
	/// <param name='chargeInfo'>
	/// The screen position where the event take place and the amount charged is passed.
	/// </param>
	void OnChargeEnd(ChargedInfo chargeInfo)
	{
		if (debug)
		{
			Debug.Log ("Charging ended "+chargeInfo.percent +" "+chargeInfo.pos);
		}
		
		LastEventPosition = chargeInfo.pos;
		
		LastEventMagnitude = chargeInfo.percent;
		
		LastChargedInfo = chargeInfo;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / CHARGING END");
	}
	
	/// <summary>
	/// when a dragging tap is detected.
	/// </summary>
	/// <param name='chargeInfo'>
	/// The drag info
	/// </param>
	void OnDragging(DragInfo dragInfo)
	{
		if (debug)
		{
			Debug.Log ("Dragging "+dragInfo.index +" "+dragInfo.pos+" "+dragInfo.delta);
		}
		
		LastEventMagnitude = dragInfo.delta.magnitude;
		
		LastEventPosition = dragInfo.pos;
		
		LastDragInfo = dragInfo;
		
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / DRAGGING");
	}

	
	/// <summary>
	/// fired when a dragging tap is first detected. 
	/// </summary>
	/// <param name='pos'>
	/// The drag position on screen
	/// </param>
	void OnDraggingStart(DragInfo dragInfo)
	{
		if (debug)
		{
			Debug.Log ("Dragging start "+dragInfo.index +" "+dragInfo.pos+" "+dragInfo.delta);
		}
		
		LastEventMagnitude = dragInfo.delta.magnitude;
		
		LastEventPosition = dragInfo.pos;
		
		LastDragInfo = dragInfo;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / DRAGGING START");
	}
	
	/// <summary>
	/// fired when a dragging tap is released. 
	/// </summary>
	/// <param name='pos'>
	/// The drag release position on screen
	/// </param>
	void OnDraggingEnd(DragInfo dragInfo)
	{
		if (debug)
		{
			Debug.Log ("Dragging end "+dragInfo.index +" "+dragInfo.pos+" "+dragInfo.delta);
		}
		
		LastEventMagnitude = dragInfo.delta.magnitude;
		
		LastEventPosition = dragInfo.pos;
		
		LastDragInfo = dragInfo;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / DRAGGING END");
	}
	
	#endregion
	
	#region TAP
	
	/// <summary>
	/// Fired when a short tap is detected.
	/// </summary>
	/// <param name='pos'>
	/// The screen position where the event take place
	/// </param>
	void OnMultiTap(Tap tap)
	{
		if (debug)
		{
			Debug.Log ("tap * "+tap.count);
		}
		
		LastEventPosition = tap.pos;
		
		LastTapInfo = tap;
		
		if (tap.count ==1)
		{
			PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / SHORT TAP");
		}else if (tap.count ==2)
		{
			PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / DOUBLE TAP");
		}else{
			
			PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / MULTI TAP");
		}
		
		
		
	}
	
	
	/// <summary>
	/// Fired when a long tap is detected.
	/// </summary>
	/// <param name='pos'>
	/// The screen position where the event take place
	/// </param>
	void OnLongTap(Tap tap)
	{
		if (debug)
		{
			Debug.Log ("long tap ");
		}
		LastEventPosition = tap.pos;
		
		LastTapInfo = tap;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / LONG TAP");
	}
	
	#endregion TAP
	
	#region DUAL TAP
	
	/// <summary>
	/// Fired when a short tap is detected using two fingers.
	/// </summary>
	/// <param name='pos'>
	/// The screen position of the centre between the two fingers
	/// </param>
	void OnMFMultiTap(Tap tapInfo)
	{
		if (debug)
		{
			Debug.Log ("MultiFingers "+tapInfo.fingerCount+" tap * "+tapInfo.count);
		}
		LastEventPosition = tapInfo.pos;
		
		LastTapInfo = tapInfo;
		
		if (tapInfo.count == 1)
		{
			PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / MF / SHORT TAP");;
		}else if (tapInfo.count ==2)
		{
			PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / MF / DOUBLE TAP");
		}else{
			
			PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / MF / MULTI TAP");
		}
		
	}

	/// <summary>
	/// Fired when a long tap is detected using two fingers.
	/// </summary>
	/// <param name='pos'>
	/// The screen position of the centre between the two fingers
	/// </param>
	void OnMFLongTap(Tap tapInfo)
	{
		if (debug)
		{
			Debug.Log ("MultiFingers long tap "+tapInfo.count);
		}
		LastEventPosition = tapInfo.pos;
		
		LastTapInfo = tapInfo;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / MF / LONG TAP");
	}

	#endregion	
	
	#region DUAL CHARGING
	
	/// <summary>
	/// when a dual finger holding is detected.
	/// </summary>
	/// <param name='chargeInfo'>
	/// The screen position where the event take place and the amount charged is passed.
	/// </param>
	void OnMFCharging(ChargedInfo chargeInfo)
	{
		if (debug)
		{
			Debug.Log ("Multi Fingers Charging "+chargeInfo.percent +" "+chargeInfo.pos);
		}
		LastEventPosition = chargeInfo.pos;
		LastEventMagnitude = chargeInfo.percent;
		LastChargedInfo = chargeInfo;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / MF / CHARGING");
	}
	
	
	/// <summary>
	/// fired when a Multi fingers charging is first detected. 
	/// </summary>
	/// <param name='chargeInfo'>
	/// The screen position where the event take place and the amount charged is passed.
	/// </param>
	void OnMFChargeStart(ChargedInfo chargeInfo)
	{
		if (debug)
		{
			Debug.Log ("Multi Fingers charging start "+chargeInfo.percent +" "+chargeInfo.pos);
		}
		
		LastEventPosition = chargeInfo.pos;
		LastEventMagnitude = chargeInfo.percent;
		LastChargedInfo = chargeInfo;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / MF / CHARGING START");
	}
	
	/// <summary>
	/// fired when a Multi fingers charging is released. 
	/// </summary>
	/// <param name='chargeInfo'>
	/// The screen position where the event take place and the amount charged is passed.
	/// </param>
	void OnMFChargeEnd(ChargedInfo chargeInfo)
	{
		if (debug)
		{
			Debug.Log ("Multi Fingers charging end "+chargeInfo.percent +" "+chargeInfo.pos);
		}
		
		LastEventPosition = chargeInfo.pos;
		LastEventMagnitude = chargeInfo.percent;
		LastChargedInfo = chargeInfo;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / MF / CHARGING END");
	}
	#endregion
	
	#region MULTI FINGERS DRAGGING
	
	/// <summary>
	/// when a Multi Fingers dragging is detected.
	/// </summary>
	/// <param name='chargeInfo'>
	/// The drag info
	/// </param>
	void OnMFDragging(DragInfo dragInfo)
	{
		if (debug)
		{
			Debug.Log ("Multi Fingers Dragging "+dragInfo.index +" "+dragInfo.pos+" "+dragInfo.delta);
		}
		
		LastEventPosition = dragInfo.pos;
		LastEventMagnitude = dragInfo.delta.magnitude;
		LastDragInfo = dragInfo;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / MF / DRAGGING");
	}
	
	/// <summary>
	/// fired when a multi fingers dragging is first detected. 
	/// </summary>
	/// <param name='dragInfo'>
	/// The drag info
	/// </param>
	void OnMFDraggingStart(DragInfo dragInfo)
	{
		if (debug)
		{
			Debug.Log ("Multi Fingers Dragging Start "+dragInfo.index +" "+dragInfo.pos+" "+dragInfo.delta);
		}

		LastEventPosition = dragInfo.pos;
		LastEventMagnitude = dragInfo.delta.magnitude;
		LastDragInfo = dragInfo;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / MF / DRAGGING START");
	}	
	
	/// <summary>
	/// fired when a multi fingers dragging is released. 
	/// </summary>
	/// <param name='dragInfo'>
	/// The drag info
	/// </param>
	void OnMFDraggingEnd(DragInfo dragInfo)
	{
		if (debug)
		{
			Debug.Log ("Multi Fingers Dragging End "+dragInfo.index +" "+dragInfo.pos+" "+dragInfo.delta);
		}

		LastEventPosition = dragInfo.pos;
		LastEventMagnitude = dragInfo.delta.magnitude;
		LastDragInfo = dragInfo;
		
		PlayMakerFSM.BroadcastEvent("INPUT TOUCHES / MF / DRAGGING END");
	}
	#endregion		
	
}

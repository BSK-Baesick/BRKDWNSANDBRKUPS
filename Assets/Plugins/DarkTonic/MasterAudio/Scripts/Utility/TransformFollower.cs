/*! \cond PRIVATE */
/*! \cond PRIVATE */
using System.Net;
using DarkTonic.MasterAudio;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
public class TransformFollower : MonoBehaviour {
	[Tooltip("This is for diagnostic purposes only. Do not change or assign this field.")]
	public Transform RuntimeFollowingTransform;
	
	private GameObject _goToFollow;
	private Transform _trans;
	private GameObject _go;
	private SphereCollider _collider;
	private string _soundType;
    private string _variationName;
    private bool _willFollowSource;
	private bool _isInsideTrigger;
	private bool _hasPlayedSound;
	private bool _groupLoadFailed;
	private MasterAudioGroup _groupToPlay;
	private bool _positionAtClosestColliderPoint = false;
    private MasterAudio.AmbientSoundExitMode _exitMode;
    private float _exitFadeTime;
    private readonly List<Collider> _actorColliders = new List<Collider>();
	private readonly List<Collider2D> _actorColliders2D = new List<Collider2D>();
	private Vector3 _lastListenerPos = new Vector3(float.MinValue, float.MinValue, float.MinValue);
	private readonly Dictionary<Collider, Vector3> _lastPositionByCollider = new Dictionary<Collider, Vector3>();
	private readonly Dictionary<Collider2D, Vector3> _lastPositionByCollider2D = new Dictionary<Collider2D, Vector3>();
	private PlaySoundResult playingVariation;

	// ReSharper disable once UnusedMember.Local
	void Awake() {
		var trig = Trigger;
		if (trig == null || _actorColliders.Count == 0 || _actorColliders2D.Count == 0 || _positionAtClosestColliderPoint || _lastListenerPos == Vector3.zero || playingVariation != null) { } // get rid of warning
	}
	
	// ReSharper disable once UnusedMember.Local
	void Start() {
		_groupToPlay = MasterAudio.GrabGroup(_soundType, false);
	}
	
    void OnDisable() {
        AmbientUtil.RemoveTransformFollower(this);
    }

	public void StartFollowing(Transform transToFollow, string soundType, string variationName, float trigRadius, bool willFollowSource, bool positionAtClosestColliderPoint,
	                           bool useTopCollider, bool useChildColliders, MasterAudio.AmbientSoundExitMode exitMode, float exitFadeTime) {
		
		RuntimeFollowingTransform = transToFollow;
		_goToFollow = transToFollow.gameObject;
		Trigger.radius = trigRadius;
		_soundType = soundType;
        _variationName = variationName;
		_willFollowSource = willFollowSource;
        _exitMode = exitMode;
        _exitFadeTime = exitFadeTime;
		_lastPositionByCollider.Clear();
		_lastPositionByCollider2D.Clear();
		
		if (useTopCollider) {
			var col3D = transToFollow.GetComponent<Collider>();
			if (col3D != null) {
				_actorColliders.Add(col3D);
				_lastPositionByCollider.Add(col3D, transToFollow.position);
			} else {
				var col2D = transToFollow.GetComponent<Collider2D>();
				if (col2D != null) {
					_actorColliders2D.Add(col2D);
					_lastPositionByCollider2D.Add(col2D, transToFollow.position);
				}
			}
		}
		
		if (useChildColliders && transToFollow != null) {
            for (var i = 0; i < transToFollow.childCount; i++) {
				var child = transToFollow.GetChild(i);
				var col3D = child.GetComponent<Collider>();
				if (col3D != null) {
					_actorColliders.Add(col3D);
					_lastPositionByCollider.Add(col3D, transToFollow.position);
					continue;
				}
				
				var col2D = child.GetComponent<Collider2D>();
				if (col2D != null) {
					_actorColliders2D.Add(col2D);
					_lastPositionByCollider2D.Add(col2D, transToFollow.position);
				}
			}
		}
		
		_lastListenerPos = MasterAudio.ListenerTrans.position;
		
		#if UNITY_5_6_OR_NEWER

		if (_actorColliders.Count == 0 && _actorColliders2D.Count == 0 && positionAtClosestColliderPoint) {
			Debug.Log("Can't follow collider of '" + transToFollow.name + "' because it doesn't have any colliders.");
		} else {
			_positionAtClosestColliderPoint = positionAtClosestColliderPoint;
			if (_positionAtClosestColliderPoint) {
				MasterAudio.QueueTransformFollowerForColliderPositionRecalc(this);
			}
		}
		
        #endif
	}
	
	private void StopFollowing() {
		RuntimeFollowingTransform = null;
		GameObject.Destroy(GameObj);
	}
	
	// ReSharper disable once UnusedMember.Local
	private void OnTriggerEnter(Collider other) {
		if (RuntimeFollowingTransform == null) {
			return;
		}
		
		if (other == null || name == AmbientUtil.ListenerFollowerName || other.name != AmbientUtil.ListenerFollowerName) {
			return; // abort if this is the Listener or if not colliding with Listener.
		}
		
		_isInsideTrigger = true;
		
		if (_groupToPlay != null) {
			switch (_groupToPlay.GroupLoadStatus) {
			case MasterAudio.InternetFileLoadStatus.Loaded:
				break;
			case MasterAudio.InternetFileLoadStatus.Failed:
				if (MasterAudio.LogSoundsEnabled) {
					MasterAudio.LogWarning("TransformFollower: '" + name + "' not attempting to play Sound Group '" + _soundType + "' because the Sound Group failed to load.");
				}
				_groupLoadFailed = true;
				return;
			case MasterAudio.InternetFileLoadStatus.Loading:
				return;
			}
		}
		
		PlaySound();
	}
	
	private void PlaySound() {
        var hasSpecificVariation = !string.IsNullOrEmpty(_variationName);

        if (_willFollowSource) {
			if (_positionAtClosestColliderPoint) {
                if (hasSpecificVariation) {
                    playingVariation = MasterAudio.PlaySound3DFollowTransform(_soundType, RuntimeFollowingTransform, 1f, 1f, 0f, _variationName);
                } else {
                    playingVariation = MasterAudio.PlaySound3DFollowTransform(_soundType, RuntimeFollowingTransform);
                }
			} else {
                if (hasSpecificVariation) {
                    MasterAudio.PlaySound3DFollowTransformAndForget(_soundType, RuntimeFollowingTransform, 1f, 1f, 0f, _variationName);
                } else {
                    MasterAudio.PlaySound3DFollowTransformAndForget(_soundType, RuntimeFollowingTransform);
                }
			}
		} else {
			if (_positionAtClosestColliderPoint) {
                if (hasSpecificVariation) {
                    playingVariation = MasterAudio.PlaySound3DAtTransform(_soundType, RuntimeFollowingTransform, 1f, 1f, 0f, _variationName);
                } else {
                    playingVariation = MasterAudio.PlaySound3DAtTransform(_soundType, RuntimeFollowingTransform);
                }
			} else {
                if (hasSpecificVariation) {
                    MasterAudio.PlaySound3DAtTransformAndForget(_soundType, RuntimeFollowingTransform, 1f, 1f, 0f, _variationName);
                } else {
                    MasterAudio.PlaySound3DAtTransformAndForget(_soundType, RuntimeFollowingTransform);
                }
			}
		}
		
		_hasPlayedSound = true;
	}
	
	// ReSharper disable once UnusedMember.Local
	public void ManualUpdate() {
		if (RuntimeFollowingTransform == null || !DTMonoHelper.IsActive(_goToFollow)) {
			StopFollowing();
			return;
		}
		
		if (!_positionAtClosestColliderPoint) {
			Trans.position = RuntimeFollowingTransform.position;
		}
		
		if (!_isInsideTrigger || _hasPlayedSound || _groupLoadFailed) {
			return;
		}
		
		switch (_groupToPlay.GroupLoadStatus) {
		    case MasterAudio.InternetFileLoadStatus.Loaded:
			    PlaySound();
			    break;
		    case MasterAudio.InternetFileLoadStatus.Failed:
			    if (MasterAudio.LogSoundsEnabled) {
				    MasterAudio.LogWarning("TransformFollower: '" + name + "' not attempting to play Sound Group '" + _soundType + "' because the Sound Group failed to load.");
			    }
			    _groupLoadFailed = true;
			    break;
		    case MasterAudio.InternetFileLoadStatus.Loading:
			    break;
		}
	}

#if UNITY_5_6_OR_NEWER
	/// <summary>
	/// Called in a queue from MasterAudio to limit the number of times this calculation occurs per frame.
	/// </summary>
	/// <returns>true if is calculated "closest position on collider"</returns>
	public bool RecalcClosestColliderPosition() {
		// follow at closest point
		var listenerPos = MasterAudio.ListenerTrans.position;
		var hasListenerMoved = _lastListenerPos != listenerPos;
		var closestPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		var minDist = float.MaxValue;
		var hasPointMoved = false;
		
		if (_actorColliders.Count > 0) {
			if (_actorColliders.Count == 1) {
				var colZero = _actorColliders[0];
                if (colZero == null) {
                    return false;
                }
                
                var colPos = colZero.transform.position;
				
				if (_lastPositionByCollider[colZero] == colPos && !hasListenerMoved) {
					// same positions, no reason to calculate new position
					return false;
				}
				
				hasPointMoved = true;
				closestPoint = colZero.ClosestPoint(listenerPos);
				
				_lastPositionByCollider[colZero] = colPos;
			} else {
				// ReSharper disable once ForCanBeConvertedToForeach
				for (var i = 0; i < _actorColliders.Count; i++) {
					var col = _actorColliders[i];
					var colPos = col.transform.position;
					
					if (_lastPositionByCollider[col] == colPos && !hasListenerMoved) {
						continue; // has not moved, continue loop
					}
					
					hasPointMoved = true;
					
					var closestPointOnCollider = col.ClosestPoint(listenerPos);
					var dist = (listenerPos - closestPointOnCollider).sqrMagnitude;
					if (dist < minDist) {
						closestPoint = closestPointOnCollider;
						minDist = dist;
					}
					
					_lastPositionByCollider[col] = colPos;
				}
			}
		} else if (_actorColliders2D.Count > 0) {
			if (_actorColliders2D.Count == 1) {
				var colZero = _actorColliders2D[0];
				var colPos = colZero.transform.position;
				
				if (_lastPositionByCollider2D[colZero] == colPos && !hasListenerMoved) {
					// same positions, no reason to calculate new position
					return false;
				}
				
				hasPointMoved = true;
				closestPoint = colZero.bounds.ClosestPoint(listenerPos);
				
				_lastPositionByCollider2D[colZero] = colPos;
			} else {
				// ReSharper disable once ForCanBeConvertedToForeach
				for (var i = 0; i < _actorColliders2D.Count; i++) {
					var col = _actorColliders2D[i];
					var colPos = col.transform.position;
					
					if (_lastPositionByCollider2D[col] == colPos && !hasListenerMoved) {
						continue; // has not moved, continue loop
					}
					
					hasPointMoved = true;
					
					var closestPointOn2DCollider = col.bounds.ClosestPoint(listenerPos);
					var dist = (listenerPos - closestPointOn2DCollider).sqrMagnitude;
					if (dist < minDist) {
						closestPoint = closestPointOn2DCollider;
						minDist = dist;
					}
					
					_lastPositionByCollider2D[col] = colPos;
				}
			}
		} else {
			return false; // no colliders. Exit
		}
		
		if (!hasPointMoved) {
			return false; // nothing changed, exit.
		}
		
		Trans.position = closestPoint;
		Trans.LookAt(MasterAudio.ListenerTrans);
		if (playingVariation != null && playingVariation.ActingVariation != null) {
			playingVariation.ActingVariation.transform.position = closestPoint;
		}
		
		_lastListenerPos = listenerPos;
		
		return true;
	}
#endif

    // ReSharper disable once UnusedMember.Local
    private void OnTriggerExit(Collider other) {
		if (RuntimeFollowingTransform == null) {
			return;
		}
		
		if (other == null || other.name != AmbientUtil.ListenerFollowerName) {
			return; // abort if not colliding with Listener.
		}
		
		_isInsideTrigger = false;
		_hasPlayedSound = false;

        switch (_exitMode) {
            case MasterAudio.AmbientSoundExitMode.StopSound:
                MasterAudio.StopSoundGroupOfTransform(RuntimeFollowingTransform, _soundType);
                break;
            case MasterAudio.AmbientSoundExitMode.FadeSound:
                MasterAudio.FadeOutSoundGroupOfTransform(RuntimeFollowingTransform, _soundType, _exitFadeTime);
                break;
        }

        playingVariation = null;
	}
	
	public SphereCollider Trigger {
		get {
			if (_collider != null) {
				return _collider;
			}
			
			_collider = GameObj.AddComponent<SphereCollider>();
			_collider.isTrigger = true;
			
			return _collider;
		}
	}
	
	public GameObject GameObj {
		get {
			if (_go != null) {
				return _go;
			}
			
			_go = gameObject;
			return _go;
		}
	}
	
	public Transform Trans {
		get {
			// ReSharper disable once ConvertIfStatementToNullCoalescingExpression
			if (_trans == null) {
				_trans = transform;
			}
			
			return _trans;
		}
	}
}
/*! \endcond */

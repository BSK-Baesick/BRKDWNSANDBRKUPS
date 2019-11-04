using DarkTonic.MasterAudio;
using UnityEngine;
 
// ReSharper disable once CheckNamespace
// ReSharper disable once InconsistentNaming
public class MA_PlayerControl : MonoBehaviour {
    public GameObject ProjectilePrefab;
    // ReSharper disable InconsistentNaming
    public bool canShoot = true;
    // ReSharper restore InconsistentNaming

    private const float MoveSpeed = 10f;
    private Transform _trans;
    private float _lastMoveAmt;

    // Use this for initialization
    // ReSharper disable once UnusedMember.Local
    void Awake() {
        useGUILayout = false;
        _trans = transform;
    }

    // ReSharper disable once UnusedMember.Local
    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.name.StartsWith("Enemy(")) {
            gameObject.SetActive(false);
        }
    }

    // ReSharper disable UnusedMember.Local
    void OnDisable() {
    }

    void OnBecameInvisible() {
    }

    void OnBecameVisible() {
    }
    // ReSharper restore UnusedMember.Local

    // Update is called once per frame 
    // ReSharper disable once UnusedMember.Local
    void Update() {
        var moveAmt = Input.GetAxis("Horizontal") * MoveSpeed * AudioUtil.FrameTime;

        if (moveAmt != 0f) {
            if (_lastMoveAmt == 0f) {
                MasterAudio.FireCustomEvent("PlayerMoved", _trans);
            }
        } else {
            if (_lastMoveAmt != 0f) {
                MasterAudio.FireCustomEvent("PlayerStoppedMoving", _trans);
            }
        }

        _lastMoveAmt = moveAmt;

        var pos = _trans.position;
        pos.x += moveAmt;
        _trans.position = pos;

        if (!canShoot || !Input.GetMouseButtonDown(0)) {
            return;
        }

        var spawnPos = _trans.position;
        spawnPos.y += 1;

        Instantiate(ProjectilePrefab, spawnPos, ProjectilePrefab.transform.rotation);
    }
}

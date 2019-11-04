using DarkTonic.MasterAudio;
using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable once InconsistentNaming
public class MA_Laser : MonoBehaviour {
    private Transform _trans;

    // ReSharper disable once UnusedMember.Local
    void Awake() {
        useGUILayout = false;
        _trans = transform;
    }

    // ReSharper disable once UnusedMember.Local
    void OnCollisionEnter(Collision collision) {
        if (!collision.gameObject.name.StartsWith("Enemy(")) {
            return;
        }

        Destroy(collision.gameObject);
        Destroy(gameObject);
    }

    // Update is called once per frame
    // ReSharper disable once UnusedMember.Local
    void Update() {
        var moveAmt = 10f * AudioUtil.FrameTime;

        var pos = _trans.position;
        pos.y += moveAmt;
        _trans.position = pos;

        if (_trans.position.y > 7) {
            Destroy(gameObject);
        }
    }
}

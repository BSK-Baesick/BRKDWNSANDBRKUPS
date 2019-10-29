using UnityEngine;

/// <summary>
/// This class handles the glitch animation for the spalsh screen.
/// </summary>

public class GlitchAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CameraPlay.Glitch2(3);
    }
}

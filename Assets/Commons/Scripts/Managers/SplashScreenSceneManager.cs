using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenSceneManager : MonoBehaviour
{

    // Configuration Parameters

    /// <summary>
    /// The delay interval before the next scene loads.
    /// </summary>
    [SerializeField] float delayInterval = 8f;

    // Start is called before the first frame update
    void Start()
    {
            // Load next scene using the Scene Manager
            Invoke("LoadNextScene", delayInterval);
    }

    void LoadNextScene()
    {
            SceneManager.LoadScene(1);
    }
}

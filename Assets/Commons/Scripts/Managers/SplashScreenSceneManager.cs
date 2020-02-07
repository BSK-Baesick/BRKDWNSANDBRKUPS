using UnityEngine;
using UnityEngine.SceneManagement;


    public class SplashScreenSceneManager : MonoBehaviour
    {
    
        public float delayInterval = 0f;
        public string sceneName = null;
    
        // Start is called before the first frame update
        public void Start()
        {
            // Load next scene using the Scene Manager
            Invoke("LoadNextScene", delayInterval);
        }
    
        void LoadNextScene()
        {
            bl_SceneLoaderUtils.GetLoader.LoadLevel(sceneName);
        }
    }
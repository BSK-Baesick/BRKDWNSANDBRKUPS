using UnityEngine;
using System.Collections.Generic;

namespace Lovatto.SceneLoader
{
    public class bl_SceneLoaderManager : ScriptableObject
    {
        [Header("Scene Manager")]
        public List<bl_SceneLoaderInfo> SceneList = new List<bl_SceneLoaderInfo>();
        [Header("Tips")]
        public List<string> TipsList = new List<string>();

        public bl_SceneLoaderInfo GetSceneInfo(string scene)
        {
            foreach(bl_SceneLoaderInfo info in SceneList)
            {
                if(info.SceneName == scene)
                {
                    return info;
                }
            }
            
            Debug.Log("Not found any scene with this name: " + scene);
            return null;           
        }

        public bool HasTips
        {
            get
            {
                return (TipsList != null && TipsList.Count > 0);
            }
        }
    }
}
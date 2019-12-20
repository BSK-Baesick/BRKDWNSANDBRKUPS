using UnityEngine;
using System;

namespace Lovatto.SceneLoader
{
    [Serializable]
    public class bl_SceneLoaderInfo
    {
        [Header("Settings")]
        public string SceneName = "Scene Name";
        public string DisplayName = "Display Name";
        [TextArea(3,7)]public string Description = "";
        [Header("References")]
        public Sprite[] Backgrounds = null;
    }
}
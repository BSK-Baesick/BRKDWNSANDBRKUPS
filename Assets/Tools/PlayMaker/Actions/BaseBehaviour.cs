using UnityEngine;
using Chronos;

namespace HutongGames.PlayMaker.Actions
{
    public class BaseBehaviour : MonoBehaviour
    {
        public Timeline time
        {
            get
            {
                return GetComponent<Timeline>();
            }
        }
    }
}

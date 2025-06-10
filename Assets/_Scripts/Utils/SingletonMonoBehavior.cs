using UnityEngine;

namespace GravityGame.Utils
{
    public abstract class SingletonMonoBehavior<TSelf> : MonoBehaviour where TSelf : MonoBehaviour
    {
        static TSelf _instance;
        public static TSelf Instance
        {
            get {
                if (!_instance) _instance = FindFirstObjectByType<TSelf>();
                return _instance;
            }
        }
    }
}
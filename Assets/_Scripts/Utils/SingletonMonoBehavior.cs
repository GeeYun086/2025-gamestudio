using UnityEngine;

namespace GravityGame.Utils
{
    /// <summary>
    ///     Inherit from this class if
    ///     1. You want a globally accessible instance with, e.g., MyCoolSingleton.Instance
    ///     2. Guaranteed to exist in the scene (e.g. script is attached to a GameObject in the GameSystems Prefab)
    ///     3. only exists once in the scene
    ///     These singletons still work after hot reloading.
    ///     But they do not have a check if there are multiple instances in the scene (you need to ensure that yourself)
    /// </summary>
    /// <typeparam name="TSelf"> </typeparam>
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
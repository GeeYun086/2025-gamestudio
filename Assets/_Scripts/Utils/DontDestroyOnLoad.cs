using UnityEngine;

namespace GravityGame.Utils
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        void Awake() => DontDestroyOnLoad(gameObject);
    }
}
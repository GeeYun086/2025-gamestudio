using UnityEngine;

namespace GravityGame
{
    public abstract class RedstoneComponent : MonoBehaviour
    {
        public abstract bool IsPowered { get; set; }
    }
}
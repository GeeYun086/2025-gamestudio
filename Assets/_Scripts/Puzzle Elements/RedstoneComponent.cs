using UnityEngine;

namespace GravityGame
{
    /// <summary>
    /// Interface used for Doors and Levers. Classes that want to change state of one GameObject through another should
    /// inherit from this.
    /// </summary>
    public abstract class RedstoneComponent : MonoBehaviour
    {
        public abstract bool IsPowered { get; set; }
    }
}
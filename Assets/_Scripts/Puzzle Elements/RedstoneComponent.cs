using UnityEngine;

namespace GravityGame
{
    /// <summary>
    /// Let Doors inherit from this class.
    /// </summary>
    public abstract class RedstoneComponent : MonoBehaviour
    {
        public abstract bool IsPowered { get; set; }
    }
}
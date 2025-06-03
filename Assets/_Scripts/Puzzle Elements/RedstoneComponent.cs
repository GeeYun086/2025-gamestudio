using UnityEngine;

namespace GravityGame
{
    /// <summary>
    /// Let Doors inherit from this class.
    /// </summary>
    public interface IRedstoneComponent
    {
        public bool IsPowered { get; set; }
    }
}
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Abstract Class for Door like components
    /// </summary>
    public abstract class RedstoneComponent : MonoBehaviour
    {
        public abstract bool IsPowered { get; set; }
    }
}
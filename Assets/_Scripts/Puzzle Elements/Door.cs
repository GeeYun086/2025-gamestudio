using System;
using UnityEngine;

namespace GravityGame
{
    /// <summary>
    /// Add to Door GameObject
    /// </summary>
    public class Door : MonoBehaviour, IRedstoneComponent 
    {
        [Header("Door Settings")]
        [SerializeField] bool _isPowered;

        public bool IsPowered
        {
            get => _isPowered;
            set {
                _isPowered = value;
                ToggleDoor();
            }
        }

        /// <summary>
        /// Toggle GameObject in scene with this script
        /// </summary>
        void ToggleDoor()
        {
            gameObject.SetActive(IsPowered);
        }
    }
}
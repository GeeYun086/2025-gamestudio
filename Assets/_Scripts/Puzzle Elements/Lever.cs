using System;
using System.Collections.Generic;
using GravityGame.Puzzle_Elements;
using UnityEngine;

namespace GravityGame
{
    /// <summary>
    /// Add to Lever GameObject and drag Doors into List
    /// </summary>
    public class Lever : InteractableObject
    {
        [Header("Lever Settings")]
        [SerializeField] bool _isPowered;
        /// <summary>
        /// Need to Add Door script to doors and drag those GameObjects into this script of the Lever
        /// </summary>
        [SerializeField] public List<RedstoneComponent> LogicComponents = new List<RedstoneComponent>();
        public bool IsPowered
        {
            set {
                _isPowered = value;
                UpdateConnectedComponents();
            }
        }

        void Awake()
        {
            OnInteract.AddListener(() => IsPowered = !_isPowered);
        }

        /// <summary>
        /// Changes IsPowered of all Doors in List.
        /// Switch between on and off.
        /// Need to set in Door`s scripts which Doors are already powered.
        /// </summary>
        public void UpdateConnectedComponents()
        {
            foreach (var component in LogicComponents) {
                if (component != null)
                    component.IsPowered = !component.IsPowered;
            }
        }
    }
}
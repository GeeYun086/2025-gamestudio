using System.Collections.Generic;
using System.Linq;
using GravityGame.Puzzle_Elements;
using UnityEngine;

namespace GravityGame
{
    /// <summary>
    ///     Add to Lever GameObject and drag Doors into List
    /// </summary>
    public class Lever : InteractableObject
    {
        [Header("Lever Settings")]
        [SerializeField] bool _isPowered;
        [SerializeField] GameObject _leverOn;
        [SerializeField] GameObject _leverOff;
        /// <summary>
        ///     Need to Add Door script to doors and drag those GameObjects into this script of the Lever
        /// </summary>
        [SerializeField] List<RedstoneComponent> _logicComponents;
        public bool IsPowered
        {
            set {
                _isPowered = value;
                _leverOn.SetActive(_isPowered);
                _leverOff.SetActive(!_isPowered);
                UpdateConnectedComponents();
            }
        }
        
        void OnEnable() => OnInteract.AddListener(Toggle);
        void OnDisable() => OnInteract.RemoveListener(Toggle);

        void Toggle() => IsPowered = !_isPowered;

        /// <summary>
        ///     Changes IsPowered of all Doors in List.
        ///     Switch between on and off.
        ///     Need to set in Door`s scripts which Doors are already powered.
        /// </summary>
        void UpdateConnectedComponents()
        {
            foreach (var component in _logicComponents.Where(c => c != null)) {
                component.IsPowered = _isPowered;
            }
        }
        
        void OnValidate() => IsPowered = _isPowered;
    }
}
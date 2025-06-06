using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     Add to Lever GameObject and drag Doors into List
    /// </summary>
    public class Lever : InteractableObject
    {
        [SerializeField] GameObject _leverOn;
        [SerializeField] GameObject _leverOff;
        
        [Header("Lever Settings")]
        [SerializeField] bool _isPowered;
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
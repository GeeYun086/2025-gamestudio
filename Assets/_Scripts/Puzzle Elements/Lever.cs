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
        
        void SetPowered(bool value)
        {
            _isPowered = value;
            _leverOn.SetActive(_isPowered);
            _leverOff.SetActive(!_isPowered);
            // Update connected components
            foreach (var component in _logicComponents.Where(c => c != null)) {
                component.IsPowered = _isPowered;
            }
        }
        
        void OnEnable() => OnInteract.AddListener(Toggle);
        void OnDisable() => OnInteract.RemoveListener(Toggle);
        void Toggle() => SetPowered(!_isPowered);
        
        void OnValidate() => SetPowered(_isPowered);
    }
}
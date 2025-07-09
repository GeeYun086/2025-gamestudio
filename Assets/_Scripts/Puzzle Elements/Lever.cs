using System;
using System.Collections.Generic;
using System.Linq;
using GravityGame.SaveAndLoadSystem;
using UnityEngine;
using UnityEngine.Events;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     Add to Lever GameObject and drag Doors into List
    /// </summary>
    public class Lever : InteractableObject, ISaveData<bool>
    {
        [SerializeField] GameObject _leverOn;
        [SerializeField] GameObject _leverOff;
        
        [Header("Events für Audio")]
        public UnityEvent OnSwitchedOn;
        public UnityEvent OnSwitchedOff;

        [Header("Lever Settings")]
        [SerializeField] bool _isPowered;
        [SerializeField] List<RedstoneComponent> _logicComponents;

        void Start()
        {
            if (_logicComponents.Count == 0) Debug.LogWarning($"{gameObject.name} has no connected redstone components, did you forget to add them?");
        }

        void SetPowered(bool value)
        {
            _isPowered = value;
            _leverOn.SetActive(_isPowered);
            _leverOff.SetActive(!_isPowered);

            // Update connected components
            foreach (var component in _logicComponents.Where(c => c != null)) {
                component.IsPowered = _isPowered;
            }
            
            if (_isPowered)
                OnSwitchedOn?.Invoke();
            else
                OnSwitchedOff?.Invoke();
        }

        void OnEnable() => OnInteract.AddListener(Toggle);
        void OnDisable() => OnInteract.RemoveListener(Toggle);
        void Toggle() => SetPowered(!_isPowered);

        void OnValidate() => SetPowered(_isPowered);
        
    #region Save and Load

        public bool Save() => _isPowered;
        public void Load(bool data) => SetPowered(data);
        [field: SerializeField] public int SaveDataID { get; set; }

    #endregion
    }
}
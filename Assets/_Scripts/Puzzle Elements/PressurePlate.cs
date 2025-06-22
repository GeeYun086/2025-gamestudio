using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     Add to Pressure Plate GameObject and drag Doors into List
    ///     Activated by GameObject with Tag "Cube"
    /// </summary>
    public class PressurePlate : MonoBehaviour
    {
        [SerializeField] GameObject _pressurePlateOn;
        [SerializeField] GameObject _pressurePlateOff;

        [Header("Pressure Plate Settings")]
        [SerializeField] List<RedstoneComponent> _logicComponents;

        bool _isPowered;

        void Start()
        {
            if (_logicComponents.Count == 0) Debug.LogWarning($"{gameObject.name} has no connected redstone components, did you forget to add them?");
        }

        void FixedUpdate()
        {
            UpdateState();
            _isPowered = false;
        }

        /// OnTriggerStay is called once per physics update for every Collider other that is touching the trigger
        void OnTriggerStay(Collider other)
        {
            if (!TriggersOverlap(other)) return;
            _isPowered = true;
        }

        bool TriggersOverlap(Collider other) => other.CompareTag("Cube");

        void UpdateState()
        {
            _pressurePlateOn.SetActive(_isPowered);
            _pressurePlateOff.SetActive(!_isPowered);

            // Update connected components
            foreach (var component in _logicComponents.Where(c => c != null)) {
                component.IsPowered = _isPowered;
            }
        }

        void OnValidate() => UpdateState();
    }
}
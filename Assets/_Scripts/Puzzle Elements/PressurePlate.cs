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
        [SerializeField] bool _isPowered;
        [SerializeField] List<RedstoneComponent> _logicComponents;

        public readonly List<Collider> OverlappingObjects = new();

        void OnTriggerEnter(Collider other)
        {
            if (!TriggersOverlap(other)) return;
            OverlappingObjects.Add(other);
            UpdateState();
        }

        public void OnTriggerExit(Collider other)
        {
            if (!TriggersOverlap(other)) return;
            OverlappingObjects.Remove(other);
            UpdateState();
        }

        void Update()
        {
            UpdateState();
        }

        bool TriggersOverlap(Collider other) => other.CompareTag("Cube");

        void UpdateState()
        {
            OverlappingObjects.RemoveAll(o => o == null);

            _isPowered = OverlappingObjects.Count > 0;

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
using System.Collections.Generic;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Add to Pressure Plate GameObject and drag Doors into List
    /// Activated by GameObject with Tag "Cube"
    /// </summary>
    public class PressurePlate : MonoBehaviour
    {
        [Header("Pressure Plate Settings")]
        [SerializeField] bool _isPowered;
        [SerializeField] List<RedstoneComponent> _logicComponents;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Cube")) {
                UpdateConnectedComponents();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Cube")) {
                UpdateConnectedComponents();
            }
        }
        
        void UpdateConnectedComponents()
        {
            foreach (var component in _logicComponents) {
                if (component != null)
                    component.IsPowered = !component.IsPowered;
            }
        }
    }
}
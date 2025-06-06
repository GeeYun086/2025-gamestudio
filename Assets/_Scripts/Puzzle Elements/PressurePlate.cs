using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Add to Pressure Plate GameObject and drag Doors into List
    /// Activated by GameObject with Tag "Cube"
    /// </summary>
    public class PressurePlate : MonoBehaviour
    {
        [SerializeField] GameObject _leverOn;
        [SerializeField] GameObject _leverOff;
        
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
            foreach (var component in _logicComponents.Where(c => c != null)) {
                component.IsPowered = _isPowered;
            }
        }
    }
}
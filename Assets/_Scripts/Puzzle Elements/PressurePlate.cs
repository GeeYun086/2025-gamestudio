using System;
using Codice.Client.Common.EventTracking;
using UnityEngine;
using System.Collections.Generic;
using GravityGame.Puzzle_Elements;


namespace GravityGame
{
    public class PressurePlate : MonoBehaviour
    {
        [Header("Pressure Plate Settings")]
        [SerializeField] bool _isPowered;
        [SerializeReference, SerializeField] List<Door> _logicComponents = new List<Door>();

        void OnTriggerEnter(Collider other) // Changed from OnTriggerEvent
        {
            if (other.CompareTag("Cube")) {
                UpdateConnectedComponents(); // Set to powered when block enters
            }
        }

        void OnTriggerExit(Collider other) // Add this method
        {
            if (other.CompareTag("Cube")) {
                UpdateConnectedComponents(); // Set to unpowered when block leaves
            }
        }

        void UpdateConnectedComponents()
        {
            foreach (var component in _logicComponents) {
                if (component != null)
                    component.IsPowered = !component.IsPowered; // Set components to match our state
            }
        }
    }
}
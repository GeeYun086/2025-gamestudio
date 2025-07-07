using System;
using System.Linq;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     GameObjects with this component can be picked up, carried, and released.
    ///     Gravity doesn't affect objects while being carried.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Carryable : MonoBehaviour, IInteractable
    {
        [NonSerialized] public Rigidbody Rigidbody;
        [NonSerialized] public Collider Collider;
        Transform _carryPoint;
        
        void OnEnable()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Collider = GetComponents<Collider>().FirstOrDefault(c => !c.isTrigger);
        }

        public void Interact()
        {
            var playerCarry = FindFirstObjectByType<PlayerCarry>();
            if (playerCarry) playerCarry.AttemptPickUp(this);
        }

        public bool IsInteractable => true;
    }
}
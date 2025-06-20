using System;
using GravityGame.Gravity;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     GameObjects with this component can be picked up, carried, and released.
    ///     Gravity doesn't affect objects while being carried.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Carryable : MonoBehaviour, IInteractable
    {
        [NonSerialized] public Rigidbody Rigidbody;
        Transform _carryPoint;

        struct CarryState
        {
            public float Drag;
            public float AngularDrag;
            public float Mass;
        }
        CarryState _preCarryState;

        // Note FS: These have been tested and look the best. (still a bit of jittering)
        const float Force = 5000f;
        const float Damping = 10f;
        const float PositionThreshold = 0.2f;
        const float RotationThreshold = 2f;

        void OnEnable() => Rigidbody = GetComponent<Rigidbody>();

        public void Interact()
        {
            var playerCarry = FindFirstObjectByType<PlayerCarry>();
            if (playerCarry) playerCarry.AttemptPickUp(this);
        }

        public bool IsInteractable => true;

        int _previousLayer;

        public void PickUp()
        {
            //
            // _preCarryState = new CarryState {
            //     Drag = _rigidbody.linearDamping,
            //     AngularDrag = _rigidbody.angularDamping,
            //     Mass = _rigidbody.mass,
            // };
            //
            // _rigidbody.linearDamping = Damping;
            // _rigidbody.angularDamping = Damping;
            // _rigidbody.mass = 0;

            _previousLayer = gameObject.layer;
            gameObject.layer = LayerMask.NameToLayer("Player");

            DisableGravity();
        }

        public void Release()
        {
            // _rigidbody.linearDamping = _preCarryState.Drag;
            // _rigidbody.angularDamping = _preCarryState.AngularDrag;
            // _rigidbody.mass = _preCarryState.Mass;
            gameObject.layer = _previousLayer;
            ReactivateGravity();
        }

        void AlignWithCarryPointRotation()
        {
            if (!_carryPoint) return;

            var rotationDifference = _carryPoint.rotation * Quaternion.Inverse(Rigidbody.rotation);
            rotationDifference.ToAngleAxis(out float angle, out var rotationAxis);
            if (angle > 180f) angle -= 360f;

            if (Mathf.Abs(angle) < RotationThreshold) return;
            Rigidbody.AddTorque(rotationAxis.normalized * (angle * Mathf.Deg2Rad * Force), ForceMode.Force);
        }

        void DisableGravity()
        {
            if (transform.TryGetComponent(out GravityModifier gravityModifier))
                gravityModifier.enabled = false;
            else
                Rigidbody.useGravity = false;
        }

        void ReactivateGravity()
        {
            if (transform.TryGetComponent(out GravityModifier gravityModifier))
                gravityModifier.enabled = true;
            else
                Rigidbody.useGravity = true;
        }
    }
}
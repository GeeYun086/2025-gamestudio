using GravityGame.Gravity;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// GameObjects with this component can be picked up, carried, and released.
    /// Gravity doesn't affect objects while being carried.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Carryable : MonoBehaviour, IInteractable
    {
        Rigidbody _rigidbody;
        Transform _carryPoint;
        float _originalDrag;
        float _originalAngularDrag;

        // Note FS: These have been tested and look the best. (still a bit of jittering)
        const float Force = 5000f;
        const float Damping = 10f;
        const float PositionThreshold = 0.2f;
        const float RotationThreshold = 2f;

        void Awake() => _rigidbody = GetComponent<Rigidbody>();

        public void Interact()
        {
            var playerCarry = FindFirstObjectByType<PlayerCarry>();
            if (playerCarry) playerCarry.AttemptPickUp(this);
        }

        public bool IsInteractable => true;

        public void PickUp(Transform carryPoint)
        {
            _carryPoint = carryPoint;

            _originalDrag = _rigidbody.linearDamping;
            _originalAngularDrag = _rigidbody.angularDamping;

            _rigidbody.linearDamping = Damping;
            _rigidbody.angularDamping = Damping;

            DisableGravity();
        }

        public void Release()
        {
            _carryPoint = null;

            _rigidbody.linearDamping = _originalDrag;
            _rigidbody.angularDamping = _originalAngularDrag;

            ReactivateGravity();
        }

        void FixedUpdate()
        {
            if (_carryPoint) {
                MoveToCarryPoint();
                AlignWithCarryPointRotation();
            }
        }

        void MoveToCarryPoint()
        {
            if (!_carryPoint) return;
            var directionToCarryPoint = _carryPoint.position - _rigidbody.position;

            if (directionToCarryPoint.magnitude < PositionThreshold) return;
            _rigidbody.AddForce(directionToCarryPoint.normalized * Force, ForceMode.Force);
        }

        void AlignWithCarryPointRotation()
        {
            if (!_carryPoint) return;

            var rotationDifference = _carryPoint.rotation * Quaternion.Inverse(_rigidbody.rotation);
            rotationDifference.ToAngleAxis(out float angle, out var rotationAxis);
            if (angle > 180f) angle -= 360f;

            if (Mathf.Abs(angle) < RotationThreshold) return;
            _rigidbody.AddTorque(rotationAxis.normalized * (angle * Mathf.Deg2Rad * Force), ForceMode.Force);
        }

        void DisableGravity()
        {
            if (transform.TryGetComponent(out GravityModifier gravityModifier))
                gravityModifier.enabled = false;
            else
                _rigidbody.useGravity = false;
        }

        void ReactivateGravity()
        {
            if (transform.TryGetComponent(out GravityModifier gravityModifier))
                gravityModifier.enabled = true;
            else
                _rigidbody.useGravity = true;
        }
    }
}
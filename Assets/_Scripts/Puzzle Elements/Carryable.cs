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
        Transform _carryPointTransform;
        bool _isCarried;

        void Awake() => _rigidbody = GetComponent<Rigidbody>();

        public void Interact()
        {
            var playerCarry = FindFirstObjectByType<PlayerCarry>();
            if (playerCarry) playerCarry.AttemptPickUp(this);
        }

        public bool IsInteractable => true;

        public void PickUp(Transform carryPoint)
        {
            _carryPointTransform = carryPoint;
            _isCarried = true;
            DisableGravity();
        }

        public void Release()
        {
            _carryPointTransform = null;
            _isCarried = false;
            ReactivateGravity();
        }

        void FixedUpdate()
        {
            if (_isCarried && _carryPointTransform) {
                MoveToCarryPoint();
                AlignWithCarryPointRotation();
            }
        }

        void MoveToCarryPoint()
        {
            var targetPosition = _carryPointTransform.position;
            const float moveSpeed = 15f;

            var newPosition = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime * moveSpeed);
            _rigidbody.MovePosition(newPosition);
        }

        void AlignWithCarryPointRotation()
        {
            const float rotationSpeed = 10f;
            var targetRotation = _carryPointTransform.rotation;
            var newRotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
            _rigidbody.MoveRotation(newRotation);
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
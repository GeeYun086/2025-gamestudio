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
        Rigidbody _rigidbody;
        Transform _carryPoint;
        const float MoveSpeed = 15f;
        const float RotationSpeed = 10f;

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
            DisableGravity();
        }

        public void Release()
        {
            _carryPoint = null;
            ReactivateGravity();
        }

        void FixedUpdate()
        {
            if (_carryPoint) {
                MoveToCarryPoint();
                AlignWithCarryPointRotation();
            }
        }

        void MoveToCarryPoint() => _rigidbody.MovePosition(Vector3.Lerp(transform.position, _carryPoint.position, Time.fixedDeltaTime * MoveSpeed));

        void AlignWithCarryPointRotation() =>
            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, _carryPoint.rotation, Time.fixedDeltaTime * RotationSpeed));

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
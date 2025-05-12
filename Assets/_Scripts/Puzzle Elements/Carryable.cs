using GravityGame.Gravity;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     GameObjects with this component can be picked up, carried and released.
    ///     Gravity doesn't affect objects while being carried.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Carryable : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Transform _carryPointTransform;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void PickUp(Transform carryPointTransform)
        {
            _carryPointTransform = carryPointTransform;
            DisableGravity();
        }

        public void Release()
        {
            _carryPointTransform = null;
            ReactivateGravity();
        }

        void FixedUpdate()
        {
            if (_carryPointTransform)
                MoveToCarryPoint();
        }

        private void MoveToCarryPoint()
        {
            float followSpeed = 6f;
            float velocitySmoothing = 12f;
            float stopThreshold = 0.02f;
            float maxSpeed = 8f;

            Vector3 toTarget = _carryPointTransform.position - transform.position;

            if (toTarget.magnitude < stopThreshold)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                return;
            }

            // Calculate target velocity
            Vector3 targetVelocity = toTarget * followSpeed;

            // Smooth current velocity toward target velocity
            Vector3 smoothedVelocity = Vector3.Lerp(_rigidbody.linearVelocity, targetVelocity,
                Time.fixedDeltaTime * velocitySmoothing);

            // Clamp speed to avoid overshooting or jitter
            if (smoothedVelocity.magnitude > maxSpeed)
                smoothedVelocity = smoothedVelocity.normalized * maxSpeed;

            _rigidbody.linearVelocity = smoothedVelocity;
        }

        private void DisableGravity()
        {
            if (transform.TryGetComponent(out GravityModifier gravityModifier))
                gravityModifier.enabled = false;
            else
                _rigidbody.useGravity = false;
        }

        private void ReactivateGravity()
        {
            if (transform.TryGetComponent(out GravityModifier gravityModifier))
                gravityModifier.enabled = true;
            else
                _rigidbody.useGravity = true;
        }
    }
}
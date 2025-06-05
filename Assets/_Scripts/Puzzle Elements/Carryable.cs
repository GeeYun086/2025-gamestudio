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
        Rigidbody _rigidbody;
        Transform _carryPointTransform;
        [SerializeField] GameObject _player;

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
            if (_carryPointTransform) {
                MoveToCarryPoint();
                RotateToPlayer();
            }
        }

        void MoveToCarryPoint()
        {
            float followSpeed = 6f;
            float velocitySmoothing = 12f;
            float stopThreshold = 0.02f;
            float maxSpeed = 8f;

            Vector3 toTarget = _carryPointTransform.position - transform.position;

            if (toTarget.magnitude < stopThreshold) {
                _rigidbody.linearVelocity = Vector3.zero;
                return;
            }

            // Calculate target velocity
            Vector3 targetVelocity = toTarget * followSpeed;

            // Smooth current velocity toward target velocity
            Vector3 smoothedVelocity = Vector3.Lerp(
                _rigidbody.linearVelocity, targetVelocity,
                Time.fixedDeltaTime * velocitySmoothing
            );

            // Clamp speed to avoid overshooting or jitter
            if (smoothedVelocity.magnitude > maxSpeed)
                smoothedVelocity = smoothedVelocity.normalized * maxSpeed;

            _rigidbody.linearVelocity = smoothedVelocity;
        }

        void RotateToPlayer()
        {
            float rotationSpeed = 8f;

            Vector3 toTarget = _player.transform.position - transform.position;
            toTarget.y = 0;

            if (toTarget.sqrMagnitude < 0.001f) {
                _rigidbody.angularVelocity = Vector3.zero;
                return;
            }

            Quaternion currentRotation = _rigidbody.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized);

            // Calculate the rotation difference
            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(currentRotation);

            // Convert quaternion delta to angle-axis
            deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

            // Ensure shortest path (angle <= 180)
            if (angleInDegrees > 180f)
                angleInDegrees -= 360f;

            if (Mathf.Abs(angleInDegrees) < 0.1f) {
                _rigidbody.angularVelocity = Vector3.zero;
                return;
            }

            // Convert to angular velocity vector
            Vector3 angularVelocity = rotationAxis.normalized * (angleInDegrees * Mathf.Deg2Rad * rotationSpeed);

            _rigidbody.angularVelocity = angularVelocity;
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
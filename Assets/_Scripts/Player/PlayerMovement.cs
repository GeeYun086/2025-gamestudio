using GravityGame.Gravity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GravityGame.Player
{
    /// <summary>
    ///     Basic player ground movement including walking and jumping
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] float _moveSpeedMps = 8f;
        [SerializeField] float _maxAcceleration = 1f;
        [SerializeField] float _jumpForce = 5.0f;
        [SerializeField] float _airMovementModifier = 0.5f;
        [SerializeField] AnimationCurve _accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] float _jumpBufferTime = 0.15f;

        [SerializeField] InputActionReference _moveInput;
        [SerializeField] InputActionReference _jumpInput;

        Rigidbody _rigidbody;
        CapsuleCollider _collider;
        bool _isGrounded;
        float _jumpBufferTimer;
        bool _isJumping;

        void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();

            _rigidbody.constraints |= RigidbodyConstraints.FreezeRotation;
            _jumpInput.action.performed += _ => _jumpBufferTimer = 0;
        }

        void FixedUpdate()
        {
            const float margin = -0.05f;
            const float groundDistance = 0.15f;
            var feetPosition = transform.position - (_collider.height * 0.5f + margin) * transform.up;
            _isGrounded = Physics.Raycast(feetPosition, -transform.up, groundDistance);
            Debug.DrawRay(feetPosition, -transform.up * groundDistance, Color.red);

            var inputDirection = _moveInput.action.ReadValue<Vector2>();
            Move(inputDirection);

            if (_jumpBufferTimer < _jumpBufferTime)
                _jumpBufferTimer += Time.fixedDeltaTime;
            if (_isGrounded && _jumpBufferTimer < _jumpBufferTime)
                Jump();
        }

        void Move(Vector2 direction)
        {
            direction = direction.normalized;
            var groundNormal = GetGroundNormal();

            var velocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
            var desiredVelocity = new Vector3(direction.x, 0, direction.y) * _moveSpeedMps;
            desiredVelocity = transform.transform.TransformDirection(desiredVelocity);
            // project to enable walking on slopes
            desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, groundNormal);
            var velocityChange = desiredVelocity - velocity;

            // Stay one slope logic
            // Note TG: I have a feeling this might cause weird issues when standing on non-static objects. Needs to be tested in real puzzles
            {
                var gravity = GetComponent<GravityModifier>();
                if (_isGrounded && direction == Vector2.zero) {
                    const float maxSlopeAngle = 35;
                    if (Vector3.Angle(groundNormal, transform.up) > maxSlopeAngle) {
                        groundNormal = transform.up; // stay on slope
                    }
                }
                gravity.GravityDirection = -groundNormal;
            }

            if (direction == Vector2.zero) {
                return;
            }

            if (_isGrounded) {
                float diff = velocityChange.magnitude / _moveSpeedMps;
                float curveValue = _accelerationCurve.Evaluate(diff);
                float clampedAccel = _maxAcceleration * curveValue;
                velocityChange = Vector3.ClampMagnitude(velocityChange, clampedAccel);
            } else {
                // In air: only apply acceleration if trying to steer in a new direction
                if (Vector3.Dot(desiredVelocity.normalized, velocity.normalized) > 0.9f) {
                    if (desiredVelocity.magnitude < velocity.magnitude) {
                        velocityChange = Vector3.zero;
                    }
                }

                velocityChange = Vector3.ClampMagnitude(velocityChange, _maxAcceleration * _airMovementModifier);
            }
            _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        Vector3 GetGroundNormal()
        {
            // Determine angle of the ground, and project the desired velocity onto the ground plane (For movement on slopes)
            const int groundLayerMask = 1 << 0;
            var ray = new Ray(transform.position, -transform.up);
            if (Physics.Raycast(ray, out var hit, _collider.height * 0.5f + 0.3f, groundLayerMask)) {
                return hit.normal;
            }
            return transform.up;
        }

        void Jump()
        {
            _rigidbody.AddForce(new Vector3(0, _jumpForce - _rigidbody.linearVelocity.y, 0), ForceMode.VelocityChange);
        }
    }
}
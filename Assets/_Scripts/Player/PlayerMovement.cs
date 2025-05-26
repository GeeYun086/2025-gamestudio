using System;
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
        [SerializeField] float _jumpForward = 0.5f;
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
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
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
            var velocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
            var desiredVelocity = new Vector3(direction.x, 0, direction.y) * _moveSpeedMps;
            desiredVelocity = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * desiredVelocity;

            // Need to set this for stay on slop logic afterwards
            var gravity = GetComponent<GravityModifier>();
            gravity.GravityDirection = Vector3.down;
            
            if (direction == Vector2.zero) {
                return;
            }

            var workingVelocity = new Vector3();

            for (int i = 0; i < 3; i++) {
                var v = desiredVelocity[i];
                var d = velocity[i];
                workingVelocity[i] = Math.Abs(v) > Math.Abs(d) ? v : d;
            }

            const float maxSlopeAngle = 40;
            const int groundLayerMask = 1 << 0;
            var ray = new Ray(transform.position, -transform.up);
            if (Physics.Raycast(ray, out var ground, _collider.height * 0.5f + 0.3f, groundLayerMask)) {
                if (Vector3.Angle(ground.normal, transform.up) > maxSlopeAngle) {
                    // Project to enable walking on slopes
                    desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, ground.normal);
                    // Stay one slope logic
                    // Note TG: I have a feeling this might cause weird issues when standing on non-static objects. Needs to be tested in real puzzles
                    if (direction == Vector2.zero) {
                        gravity = GetComponent<GravityModifier>();
                        gravity.GravityDirection = -ground.normal;
                    }
                }

                // Make velocity relative to moving ground
                if (ground.rigidbody) {
                    var groundVelocity = new Vector3(ground.rigidbody.linearVelocity.x, 0, ground.rigidbody.linearVelocity.z);
                    desiredVelocity += groundVelocity; // relative to moving ground
                }
            } 

            // Steer current velocity towards move direction
            {
                var angle = Vector3.Angle(velocity, desiredVelocity);
                const float lostSpeedPerAngle = 0.01f;
                const float anglePerSecond = 360;

                if (angle < 90 && velocity.magnitude > _moveSpeedMps * 0.75f) {
                    var interpolatedVelocity = Vector3.Lerp(velocity, desiredVelocity, Time.fixedDeltaTime * anglePerSecond / angle);
                    var speed = interpolatedVelocity.magnitude;
                    var changedAngle = Vector3.Angle(velocity, interpolatedVelocity);
                    var steeredSpeed = velocity.magnitude * (1 - lostSpeedPerAngle * changedAngle);
                    speed = Math.Max(speed, steeredSpeed);
                    speed = Math.Max(speed, _moveSpeedMps);
                    workingVelocity = interpolatedVelocity.normalized * speed;
                    Debug.Log(changedAngle);

                }
            }

            var velocityChange = workingVelocity - velocity;

            var airModifier = _isGrounded ? 1f : _airMovementModifier;
            velocityChange = Vector3.ClampMagnitude(
                velocityChange,
                airModifier * _maxAcceleration * Time.fixedDeltaTime
            );
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
            var input = _moveInput.action.ReadValue<Vector2>();
            var input3 = new Vector3(input.x, 0, input.y);
            var jumpFwd = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * input3 * _jumpForward;
            var jumpUp = new Vector3(0, _jumpForce - _rigidbody.linearVelocity.y);
            _rigidbody.AddForce(jumpFwd + jumpUp, ForceMode.VelocityChange);
        }
    }
}
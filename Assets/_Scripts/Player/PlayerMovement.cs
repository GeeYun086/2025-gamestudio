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
            var groundNormal = GetGroundNormal();

            var velocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
            var desiredVelocity = new Vector3(direction.x, 0, direction.y) * _moveSpeedMps;
            desiredVelocity = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * desiredVelocity;
            // project to enable walking on slopes
            desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, groundNormal);

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

            var angle = Vector3.Angle(velocity, desiredVelocity);
            const float lostSpeedPerAngle = 0.01f;
            const float anglePerSecond = 360;
            var interpolatedVelocity = Vector3.Slerp(velocity, desiredVelocity, Time.fixedDeltaTime * anglePerSecond / angle);
            var speed = interpolatedVelocity.magnitude;
            if (angle < 90) {
                var changedAngle = Vector3.Angle(velocity, interpolatedVelocity);
                var steeredSpeed = velocity.magnitude * (1 - lostSpeedPerAngle * changedAngle);
                speed = Math.Max(speed, steeredSpeed);
                if (speed < _moveSpeedMps) {
                    var airModifier = _isGrounded ? 1f : _airMovementModifier;
                    speed = Math.Min(_moveSpeedMps, speed + _maxAcceleration * Time.fixedDeltaTime * airModifier);
                }
            }
            
            var newVelocity = interpolatedVelocity.normalized * speed;
            var velocityChange = newVelocity - velocity;

            
            
            
            // for (int i = 0; i < 3; i++) {
            //     var desired = desiredVelocity[i];
            //     var current = velocity[i];
            //
            //     if (desired == 0) continue;
            //     var div = current / desired;
            //     var sign = Math.Sign(desired);
            //     Debug.Log(div);
            //
            //     velocityChange[i] = div switch {
            //         < 0 => desired - current,
            //         < 1 => desired - current,
            //         _ => 0
            //     };
            // }
            //
            // var airModifier = _isGrounded ? 1f : _airMovementModifier;
            // velocityChange = Vector3.ClampMagnitude(
            //     velocityChange,
            //     airModifier * _maxAcceleration * Time.fixedDeltaTime
            // );

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
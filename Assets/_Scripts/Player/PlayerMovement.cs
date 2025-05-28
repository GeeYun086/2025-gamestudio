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
        [Header("Movement")]
        [SerializeField] float _maxMoveSpeed = 8f;
        [SerializeField] float _maxAcceleration = 1f;
        [SerializeField] float _airMovementModifier = 0.5f;
        [SerializeField] float _maxSlopeAngle = 40;

        [Header("Jumping")]
        [SerializeField] float _jumpUpwardSpeed = 9;
        [SerializeField] float _jumpForwardSpeed = 0.3f;
        [SerializeField] float _jumpBufferTime = 0.15f;
        [SerializeField] float _coyoteTime = 0.2f;

        [Header("Input")]
        [SerializeField] InputActionReference _moveInput;
        [SerializeField] InputActionReference _jumpInput;

        Rigidbody _rigidbody;
        CapsuleCollider _collider;
        Camera _camera;
        RaycastHit? _ground;
        float _lastJumpInputTime;
        float _lastJumpTime;
        float _coyoteLastGroundedTime;

        void OnEnable()
        {
            _collider = GetComponent<CapsuleCollider>();
            _rigidbody = GetComponent<Rigidbody>();
            _camera = GetComponentInChildren<Camera>();

            _rigidbody.constraints |= RigidbodyConstraints.FreezeRotation;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            _jumpInput.action.performed += _ => _lastJumpInputTime = Time.time;
        }

        void FixedUpdate()
        {
            _ground = CheckGround();
            if (_ground is not null) _coyoteLastGroundedTime = Time.time;

            var inputDirection = _moveInput.action.ReadValue<Vector2>();
            Move(inputDirection);

            bool hasJumpInput = _lastJumpInputTime + _jumpBufferTime > Time.time;
            bool canJump = _coyoteLastGroundedTime + _coyoteTime > Time.time;
            if (hasJumpInput && canJump) {
                _coyoteLastGroundedTime = 0;
                Jump();
            }
        }

        RaycastHit? CheckGround()
        {
            const float margin = -0.05f;
            const float groundDistance = 0.15f;
            const int groundLayerMask = 1 << 0;
            var feetPosition = transform.position - (_collider.height * 0.5f + margin) * transform.up;
            var down = -transform.up;
            bool hit = Physics.Raycast(feetPosition, down, out var ground, groundDistance, groundLayerMask);

            Debug.DrawRay(feetPosition, down * groundDistance, hit ? Color.green : Color.red);
            return hit ? ground : null;
        }

        void Move(Vector2 direction)
        {
            direction = direction.normalized;
            var velocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
            var desiredVelocity = new Vector3(direction.x, 0, direction.y) * _maxMoveSpeed;
            desiredVelocity = Quaternion.Euler(0, _camera.transform.eulerAngles.y, 0) * desiredVelocity;

            // Need to reset this incase this was changed by stay on slope logic
            var gravity = GetComponent<GravityModifier>();
            gravity.GravityDirection = Vector3.down;

            if (direction == Vector2.zero) {
                return;
            }

            var workingVelocity = new Vector3();

            // Take max velocity of each axis
            for (int i = 0; i < 3; i++) {
                float d = desiredVelocity[i];
                float v = velocity[i];
                workingVelocity[i] = Math.Abs(v) > Math.Abs(d) ? v : d;
            }

            if (_ground is { } ground) {
                if (Vector3.Angle(ground.normal, transform.up) > _maxSlopeAngle) {
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
                const float lostSpeedPerAngle = 0.01f;
                const float anglePerSecond = 360;
                float angle = Vector3.Angle(velocity, desiredVelocity);

                if (angle < 90 && velocity.magnitude > _maxMoveSpeed * 0.75f) {
                    var interpolatedVelocity = Vector3.Lerp(velocity, desiredVelocity, Time.fixedDeltaTime * anglePerSecond / angle);
                    float speed = interpolatedVelocity.magnitude;
                    float changedAngle = Vector3.Angle(velocity, interpolatedVelocity);
                    float steeredSpeed = velocity.magnitude * (1 - lostSpeedPerAngle * changedAngle);
                    speed = Math.Max(speed, steeredSpeed);
                    speed = Math.Max(speed, _maxMoveSpeed);
                    workingVelocity = interpolatedVelocity.normalized * speed;
                }
            }

            // Clamp Velocity Change
            var velocityChange = workingVelocity - velocity;
            float airModifier = _ground is not null ? 1f : _airMovementModifier;
            float maxVelocityChange = airModifier * _maxAcceleration * Time.fixedDeltaTime;
            velocityChange = Vector3.ClampMagnitude(velocityChange, maxVelocityChange);

            // Change Velocity
            _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        void Jump()
        {
            if (_lastJumpTime + 0.1f > Time.time) return; // cooldown to jump triggering multiple jumps in two consecutive frames
            _lastJumpTime = Time.time;
            var moveInput = _moveInput.action.ReadValue<Vector2>();
            var moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
            var fwd = Quaternion.Euler(0, _camera.transform.eulerAngles.y, 0) * moveDirection;

            var jumpFwd = fwd * _jumpForwardSpeed;
            float downwardsVelocity = Math.Min(0, _rigidbody.linearVelocity.y); // cancel downwards velocity
            var jumpUp = transform.up * (_jumpUpwardSpeed - downwardsVelocity);
            _rigidbody.AddForce(jumpFwd + jumpUp, ForceMode.VelocityChange);
        }
    }
}
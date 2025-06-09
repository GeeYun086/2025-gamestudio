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
        [SerializeField] float _groundFriction = 8f;

        [SerializeField] float _airMovementModifier = 0.5f;
        [SerializeField] float _maxSlopeAngle = 40;

        [Header("Jumping")]
        [SerializeField] float _jumpHeight = 1.4f;
        [SerializeField] float _jumpBufferTime = 0.15f;
        [SerializeField] float _coyoteTime = 0.2f;

        [Header("Input")]
        [SerializeField] InputActionReference _moveInput;
        [SerializeField] InputActionReference _jumpInput;

        Rigidbody _rigidbody;
        CapsuleCollider _collider;
        Camera _camera;
        float _lastJumpInputTime;
        float _lastJumpTime;
        float _coyoteLastGroundedTime;
        Vector3 _inputDirection;
        GroundInfo _ground;

        struct GroundInfo
        {
            public bool HasAnyGround;
            public bool HasStableGround;

            public RaycastHit Hit;
            public Vector3 Normal;
            public float Angle;
        }

        Vector3 Gravity => GetComponent<GravityModifier>().Gravity;

        GroundInfo CheckGround()
        {
            const float margin = 0.05f;
            const float groundDistance = 0.15f;
            int layerMask = ~LayerMask.GetMask("Player");
            var feetPosition = transform.position + margin * transform.up;
            var down = -transform.up;

            GroundInfo ground = default;
            if (Physics.Raycast(feetPosition, down, out var hit, groundDistance, layerMask)) {
                var info = new GroundInfo();
                info.Hit = hit;
                info.Normal = hit.normal;
                info.HasAnyGround = true;
                info.Angle = Vector3.Angle(info.Normal, transform.up);
                info.HasStableGround = info.Angle <= _maxSlopeAngle;
                ground = info;
            }

            Debug.DrawRay(feetPosition, down * groundDistance, ground.HasAnyGround ? Color.green : Color.red);
            return ground;
        }

        void OnEnable()
        {
            _collider = GetComponent<CapsuleCollider>();
            _rigidbody = GetComponent<Rigidbody>();
            _camera = GetComponentInChildren<Camera>();

            _rigidbody.constraints |= RigidbodyConstraints.FreezeRotation;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            _jumpInput.action.performed += _ => _lastJumpInputTime = Time.time;
        }

        void Update()
        {
            var input = _moveInput.action.ReadValue<Vector2>().normalized;
            _inputDirection = Quaternion.Euler(0, _camera.transform.eulerAngles.y, 0) * new Vector3(input.x, 0, input.y);
        }

        void FixedUpdate()
        {
            _ground = CheckGround();
            var gravity = Gravity;

            bool didJump = TryJump();

            var upVelocity = Vector3.Project(_rigidbody.linearVelocity, transform.up);
            var velocity = _rigidbody.linearVelocity - upVelocity;
            var desiredVelocity = _inputDirection * _maxMoveSpeed;
            Debug.Log(velocity);

            var workingVelocity = velocity;

            if (_ground.HasAnyGround) {
                float friction = 8.0f;
                workingVelocity *= 1f - friction * Time.fixedDeltaTime;
                if (_ground.HasStableGround) {
                    // Project to enable walking on slopes
                    desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, _ground.Normal);
                    // Stay one slope logic
                    // Note TG: I have a feeling this might cause weird issues when standing on non-static objects. Needs to be tested in real puzzles
                    if (_inputDirection == Vector3.zero) { }
                    if (_ground.Angle > 0.01f && !didJump) {
                        // TODO may need to disable on rigidbodies
                        gravity = -_ground.Normal * 80f;
                    }
                } else {
                    var slopePerpendicular = Vector3.Cross(_ground.Normal, transform.up);
                    var slopeUp = Vector3.Cross(_ground.Normal, slopePerpendicular);
                    var slopeUpVelocity = Vector3.Project(workingVelocity, slopeUp);
                    // if (Vector3.Dot(slopeUp, slopeUpVelocity) > 0)
                    {
                        workingVelocity -= slopeUpVelocity;
                    }
                }

                // Make velocity relative to moving ground
                if (_ground.Hit.rigidbody is { } dynamicGround) {
                    var groundVelocity = new Vector3(dynamicGround.linearVelocity.x, 0, dynamicGround.linearVelocity.z);
                    desiredVelocity += groundVelocity; // relative to moving ground
                }
            }


            // Take max velocity of each axis
            for (int i = 0; i < 3; i++) {
                float AbsMax(float a, float b) => Math.Abs(a) > Math.Abs(b) ? a : b;
                workingVelocity[i] = AbsMax(workingVelocity[i], desiredVelocity[i]);
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
            float airModifier = _ground.HasAnyGround ? 1f : _airMovementModifier;
            float maxVelocityChange = airModifier * _maxAcceleration * Time.fixedDeltaTime;
            velocityChange = Vector3.ClampMagnitude(velocityChange, maxVelocityChange);

            // Change Velocity
            _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
            _rigidbody.AddForce(gravity, ForceMode.Acceleration);
        }

        bool TryJump()
        {
            if (_ground.HasStableGround) _coyoteLastGroundedTime = Time.time;
            bool hasJumpInput = _lastJumpInputTime + _jumpBufferTime > Time.time;
            bool canJump = _coyoteLastGroundedTime + _coyoteTime > Time.time && _lastJumpTime + 0.5f < Time.time;
            if (!hasJumpInput || !canJump) return false;
            _coyoteLastGroundedTime = 0;
            _lastJumpTime = Time.time;

            float upVelocity = Vector3.Project(_rigidbody.linearVelocity, transform.up).magnitude;
            float downwardsVelocity = Math.Min(0, upVelocity); // cancel downwards velocity
            float jumpUpSpeed = Mathf.Sqrt(_jumpHeight * 2f * Gravity.magnitude);
            var jumpUp = transform.up * (jumpUpSpeed - downwardsVelocity);
            _rigidbody.AddForce(jumpUp, ForceMode.VelocityChange);
            _ground = default; // no ground
            return true;
        }
    }
}
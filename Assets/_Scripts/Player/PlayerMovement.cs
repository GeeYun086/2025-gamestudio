using System;
using GravityGame.Gravity;
using GravityGame.Utils;
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
        [SerializeField] float _maxStepHeight = 0.5f;

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
            float radius = _collider.radius * 0.9f;
            var feetPosition = transform.position + (radius + margin) * transform.up;
            float distance = groundDistance;
            var down = -transform.up;

            GroundInfo ground = default;
            if (Physics.SphereCast(feetPosition, radius, down, out var hit, distance, layerMask)) {
                var info = new GroundInfo();
                info.Hit = hit;
                info.Normal = hit.normal;
                info.HasAnyGround = true;
                info.Angle = Vector3.Angle(info.Normal, transform.up);
                info.HasStableGround = info.Angle <= _maxSlopeAngle;
                ground = info;
            }

            DebugDraw.DrawSphere(feetPosition + down * distance, radius, ground.HasAnyGround ? Color.green : Color.red);
            return ground;
        }

        void FindStep()
        {
            if (!_ground.HasStableGround) return;
            if (_inputDirection == Vector3.zero) return;
            // int layerMask = ~LayerMask.GetMask("Player");
            var input = _inputDirection.normalized;
            // input = Vector3.ProjectOnPlane(input, _ground.Normal);
            
            var rayFront = input.normalized * (_collider.radius + 0.15f);
            float distance = 0.8f;
            var rayUp = distance * transform.up;
            var origin = transform.position + rayUp + rayFront;
            var dir = -transform.up;

            bool didHit = false;
            if (Physics.Raycast(origin, dir, out var hit, distance)) {
                const float minStepHeight = 0.05f;
                var stepHeight = distance - hit.distance;
                if (stepHeight < _maxStepHeight && stepHeight > minStepHeight) {
                    didHit = true;
                    Debug.DrawRay(origin, dir * hit.distance, Color.green);
                } else {
                    Debug.DrawRay(origin, dir * hit.distance, Color.yellow);
                }
            }
            Debug.Log(didHit);

            Debug.DrawRay(origin, dir * distance, Color.red);
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
            _inputDirection = Vector3.ClampMagnitude(_inputDirection, 1f);
        }

        void FixedUpdate()
        {
            _ground = CheckGround();
            FindStep();
            var gravity = Gravity;

            bool didJump = TryJump();

            var upVelocity = Vector3.Project(_rigidbody.linearVelocity, transform.up);
            float upVelocityValue = Vector3.Dot(upVelocity, transform.up) > 0 ? upVelocity.magnitude : -upVelocity.magnitude;
            var velocity = _rigidbody.linearVelocity - upVelocity;
            var desiredVelocity = _inputDirection * _maxMoveSpeed;

            var workingVelocity = velocity;

            if (_ground.HasAnyGround) {
                workingVelocity *= 1f - _groundFriction * Time.fixedDeltaTime;
            }

            if (_ground.HasAnyGround) {
                if (_ground.HasStableGround) {
                    // Project to enable walking on slopes
                    desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, _ground.Normal);
                    // Stay one slope logic
                    // Note TG: I have a feeling this might cause weird issues when standing on non-static objects. Needs to be tested in real puzzles
                    if (_inputDirection == Vector3.zero) {
                        gravity = -_ground.Normal * 80f;
                    }
                    if (upVelocityValue > 0f && _ground.Angle > 0.01f && !didJump) {
                        // TODO may need to disable on rigidbodies
                        gravity = -_ground.Normal * 80f;
                    }
                } else {
                    var slopePerpendicular = Vector3.Cross(_ground.Normal, transform.up);
                    var slopeUp = Vector3.Cross(_ground.Normal, slopePerpendicular);
                    var slopeUpVelocity = Vector3.Project(_rigidbody.linearVelocity, slopeUp);
                    if (Vector3.Dot(slopeUp, slopeUpVelocity) > 0) {
                        workingVelocity -= slopeUpVelocity;
                    }
                    desiredVelocity -= Vector3.ProjectOnPlane(desiredVelocity, _ground.Normal);
                    // workingVelocity -= Vector3.Project(workingVelocity, slopePerpendicular) * _groundFriction * Time.fixedDeltaTime;
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
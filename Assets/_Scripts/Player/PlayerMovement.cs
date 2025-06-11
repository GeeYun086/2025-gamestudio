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
        [Header("Ground Movement")]
        public float MaxMoveSpeed = 8.0f;
        public float MoveAcceleration = 70.0f;
        public float GroundFriction = 8.0f;

        public float MaxStepHeight = 0.5f;
        public float MaxAirStepHeight = 0.1f;
        public float MaxSlopeAngle = 40.0f;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 4.0f;
        public float AirAcceleration = 30.0f;
        public float AirDrag = 0.1f;

        [Header("Jumping")]
        public float JumpHeight = 1.4f;
        public float JumpPreGroundedGraceTime = 0.15f;
        public float JumpPostGroundedGraceTime = 0.15f;
        public float MinTimeBetweenJumps = 0.2f;

        [Header("Input")]
        public InputActionReference MoveInput;
        public InputActionReference JumpInput;

        [Header("Debug")]
        public bool DebugStepDetection;

        Rigidbody _rigidbody;
        CapsuleCollider _collider;
        Camera _camera;
        float _lastJumpInputTime;
        float _lastJumpTime;
        float _coyoteLastGroundedTime;
        Vector3 _inputDirection;
        public GroundInfo Ground;

        public struct GroundInfo
        {
            public bool HasAnyGround;
            public bool HasStableGround;

            public RaycastHit Hit;
            public Vector3 Normal;
            public float Angle;
        }

        Vector3 Gravity => GetComponent<GravityModifier>().Gravity;

        void OnEnable()
        {
            _collider = GetComponent<CapsuleCollider>();
            _rigidbody = GetComponent<Rigidbody>();
            _camera = GetComponentInChildren<Camera>();

            _rigidbody.constraints |= RigidbodyConstraints.FreezeRotation;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            JumpInput.action.performed += _ => _lastJumpInputTime = Time.time;
        }

        void Update()
        {
            var input = MoveInput.action.ReadValue<Vector2>().normalized;
            var right = _camera.transform.right;
            var forward = Vector3.Cross(right, transform.up);
            _inputDirection = forward * input.y + right * input.x;
            _inputDirection = Vector3.ClampMagnitude(_inputDirection, 1f);
        }

        void FixedUpdate()
        {
            transform.up = -Gravity;
            Ground = CheckGround(transform.position);
            Move();
            TryStepUp();
        }

        void Move()
        {
            Rigidbody dynamicGround = null;
            var groundVelocity = Vector3.zero;
            if (Ground.HasStableGround && Ground.Hit.rigidbody != null) {
                dynamicGround = Ground.Hit.rigidbody;
                groundVelocity = Vector3.ProjectOnPlane(dynamicGround.linearVelocity, transform.up);
            }
            
            bool jumped = TryJump(out var jumpVelocity);

            float deltaTime = Time.fixedDeltaTime;
            var gravity = Gravity;

            var velocity = _rigidbody.linearVelocity;

            if (Ground.HasAnyGround) {
                // Ground Friction
                velocity -= Vector3.ProjectOnPlane(velocity, Ground.Normal) * GroundFriction * deltaTime;
                
                if (Ground.HasStableGround) {
                    // Project walk movement on slope
                    _inputDirection = Vector3.ProjectOnPlane(_inputDirection, Ground.Normal);

                    var upVelocity = Vector3.Project(velocity, transform.up);
                    float upVelocityValue = Vector3.Dot(upVelocity, transform.up) > 0 ? upVelocity.magnitude : -upVelocity.magnitude;

                    if (Ground.Angle > 0.01f) {
                        var stickToGround = -Ground.Normal * 50f;
                        if (dynamicGround) {
                            stickToGround = gravity * 0.1f;
                            if (_inputDirection == Vector3.zero) {
                                var slopeUp = Vector3.ProjectOnPlane(transform.up, Ground.Normal);
                                var slopeUpVelocity = Vector3.Project(velocity, slopeUp);
                                velocity -= slopeUpVelocity;   
                            }
                        }
                        if (_inputDirection == Vector3.zero) {
                            // stick to slope when standing still
                            gravity = stickToGround;
                        } else if (upVelocityValue > 0f && !jumped) {
                            // stick to slope when walking up it
                            gravity = stickToGround;
                        }
                    }
                    
                } else {
                    var slopeUp = Vector3.ProjectOnPlane(transform.up, Ground.Normal);
                    // Eliminate velocity up slopes
                    var slopeUpVelocity = Vector3.Project(velocity, slopeUp);
                    if (Vector3.Dot(slopeUp, slopeUpVelocity) > 0) {
                        velocity -= slopeUpVelocity;
                    }
                    // Eliminate input up slopes
                    var slopeUpInput = Vector3.Project(_inputDirection, slopeUp);
                    if (Vector3.Dot(slopeUp, slopeUpInput) > 0) {
                        _inputDirection -= slopeUpInput;
                    }
                }

                velocity = ElementWiseMax(velocity, groundVelocity);
            } else {
                // Air Drag
                velocity -= Vector3.ProjectOnPlane(velocity, transform.up) * AirDrag * deltaTime;
            }

            {
                // Ground Acceleration
                var velocityRelativeToGround = velocity - groundVelocity;
                var velocityInInputDir = Vector3.Project(velocityRelativeToGround, _inputDirection);
                bool movingInOppositeDirection = Vector3.Dot(velocityInInputDir, _inputDirection) < 0;

                var moveSpeed = Ground.HasStableGround ? MaxMoveSpeed : MaxAirMoveSpeed;
                var acceleration = Ground.HasStableGround ? MoveAcceleration : AirAcceleration;
                if (velocityInInputDir.magnitude < moveSpeed || movingInOppositeDirection) {
                    var desiredInputVelocity = _inputDirection.normalized * moveSpeed;
                    var newVelocityInInputDir = Vector3.MoveTowards(velocityInInputDir, desiredInputVelocity, acceleration * deltaTime);
                    velocityRelativeToGround += newVelocityInInputDir - velocityInInputDir;
                    velocity = velocityRelativeToGround + groundVelocity;
                }
            }

            // Jump
            if (jumped) {
                var upVelocity = Vector3.Project(velocity, transform.up);
                var onlyUpVelocity = Vector3.Dot(upVelocity, transform.up) > 0 ? upVelocity : Vector3.zero;

                // Jump eliminates velocity outside inputDirection
                var planeVelocity = Vector3.ProjectOnPlane(velocity, transform.up);
                var planeVelocityInInputDir = Vector3.Project(planeVelocity, _inputDirection);
                planeVelocityInInputDir = Vector3.Dot(planeVelocityInInputDir, _inputDirection) > 0 ? planeVelocityInInputDir : Vector3.zero;
                
                var jumpForwardVelocity = Mathf.Max(Mathf.Min(MaxMoveSpeed, planeVelocity.magnitude), planeVelocityInInputDir.magnitude);
                var jumpForward = _inputDirection.normalized * jumpForwardVelocity;
                
                velocity = jumpVelocity + jumpForward + onlyUpVelocity + groundVelocity;
                
                // push ground down
                dynamicGround?.AddForceAtPosition(-jumpVelocity * _rigidbody.mass, Ground.Hit.point, ForceMode.Impulse);
            }

            _rigidbody.AddForce(velocity - _rigidbody.linearVelocity, ForceMode.VelocityChange);
            _rigidbody.AddForce(gravity, ForceMode.Acceleration);
            return;

            Vector3 ElementWiseMax(Vector3 a, Vector3 b)
            {
                var result = Vector3.zero;
                for (int i = 0; i < 3; i++) {
                    result[i] = AbsMax(a[i], b[i]);
                }
                return result;
                float AbsMax(float l, float r) => Math.Abs(l) > Math.Abs(r) ? l : r;
            }
        }

        void TryStepUp()
        {
            if (FindStep() is { HasStableGround: true } step) {
                if (DebugStepDetection) Debug.Log("Player Stepped!");
                var difference = step.Hit.point - transform.position;
                var up = Vector3.Project(difference, transform.up);
                var fwd = difference - up;
                // Move up
                _rigidbody.MovePosition(transform.position + up.normalized * (up.magnitude + 0.05f));

                // Add fwd speed so you have enough to climb stair
                float climbStairFwdBoost = 1.0f;
                _rigidbody.AddForce(fwd.normalized * climbStairFwdBoost, ForceMode.VelocityChange);
                // eliminate downwards velocity
                var upVelocity = Vector3.Project(_rigidbody.linearVelocity, transform.up);
                var onlyUpVelocity = Vector3.Dot(upVelocity, transform.up) > 0 ? upVelocity : Vector3.zero;
                _rigidbody.AddForce(onlyUpVelocity - upVelocity, ForceMode.VelocityChange);
                _rigidbody.AddForce(transform.up * 1.0f, ForceMode.VelocityChange);
            }
        }

        bool TryJump(out Vector3 jumpVelocity)
        {
            jumpVelocity = Vector3.zero;
            if (Ground.HasStableGround) _coyoteLastGroundedTime = Time.time;
            bool hasJumpInput = _lastJumpInputTime + JumpPreGroundedGraceTime > Time.time;
            bool canJump = _coyoteLastGroundedTime + JumpPostGroundedGraceTime > Time.time && _lastJumpTime + MinTimeBetweenJumps < Time.time;
            if (!hasJumpInput || !canJump) return false;
            _coyoteLastGroundedTime = 0;
            _lastJumpTime = Time.time;

            float jumpUpSpeed = Mathf.Sqrt(JumpHeight * 2f * Gravity.magnitude);
            jumpVelocity = transform.up * jumpUpSpeed;
            
            // un-ground yourself
            Ground = default; 
            return true;
        }

        GroundInfo CheckGround(Vector3 position)
        {
            const float margin = 0.05f;
            const float groundDistance = 0.15f;
            int layerMask = ~LayerMask.GetMask("Player");
            float radius = _collider.radius * 0.9f;
            var feetPosition = position + (radius + margin) * transform.up;
            float distance = groundDistance;
            var down = -transform.up;

            GroundInfo ground = default;
            if (Physics.SphereCast(feetPosition, radius, down, out var hit, distance, layerMask)) {
                var info = new GroundInfo();
                info.Hit = hit;
                info.Normal = hit.normal;
                info.HasAnyGround = true;
                info.Angle = Vector3.Angle(info.Normal, transform.up);
                info.HasStableGround = info.Angle <= MaxSlopeAngle;
                ground = info;
            }

            DebugDraw.DrawSphere(feetPosition + down * distance, radius, ground.HasAnyGround ? Color.green : Color.red);
            return ground;
        }

        GroundInfo FindStep()
        {
            GroundInfo noStep = default;
            if (Ground is { HasAnyGround: true, HasStableGround: false }) return noStep; // no stepping on steep slope
            // if (!_ground.HasAnyGround && Vector3.Project(_rigidbody.linearVelocity, transform.up).magnitude > 1.0f)
            //     return noStep; // no air stepping when velocity is too high
            if (_inputDirection == Vector3.zero) return noStep; // no unintended stepping

            const float minStepHeight = 0.05f;
            const float stepForward = 0.05f;
            int layerMask = ~LayerMask.GetMask("Player");
            var input = Vector3.ProjectOnPlane(_inputDirection, Ground.HasStableGround ? Ground.Normal : transform.up);
            var maxStepHeight = Ground.HasStableGround ? MaxStepHeight : MaxAirStepHeight;

            float distance = _collider.height;
            var rayFront = input.normalized * (_collider.radius + stepForward);
            var rayUp = (distance + minStepHeight) * transform.up;
            var origin = transform.position + rayUp + rayFront;
            var dir = -transform.up;

            if (!Physics.Raycast(origin, dir, out var hit, distance, layerMask)) {
                // no hit
                if (DebugStepDetection) Debug.DrawRay(origin, dir * distance, Color.red);
                return noStep;
            }
            float stepHeight = distance - hit.distance;

            bool inStepThreshold = stepHeight <= maxStepHeight;
            var stepGround = CheckGround(hit.point);
            if (inStepThreshold && stepGround.HasStableGround) {
                // good hit
                if (DebugStepDetection) Debug.DrawRay(origin, dir * hit.distance, Color.green);
                return stepGround;
            }

            // bad hit
            if (DebugStepDetection) Debug.DrawRay(origin, dir * hit.distance, Color.yellow);
            return noStep;
        }
    }
}
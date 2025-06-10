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
        GroundInfo _ground;
        Vector3 _lastGroundVelocity;

        struct GroundInfo
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
            _ground = CheckGround(transform.position);
            Move();
            TryStepUp();
        }

        void Move()
        {
            float deltaTime = Time.fixedDeltaTime;
            var velocity = _rigidbody.linearVelocity;
            var gravity = Gravity;

            // Get ground velocity (e.g. moving platform)
            var dynamicGround = _ground.Hit.rigidbody; // or null
            var groundVelocity = _ground.HasStableGround && dynamicGround
                ? Vector3.ProjectOnPlane(dynamicGround.linearVelocity, transform.up)
                : Vector3.zero;
            var groundVelocityDelta = groundVelocity - _lastGroundVelocity;
            
            float platformStopThreshold = 1.0f;
            bool groundStoppedImmediately = Vector3.Dot(groundVelocityDelta, _lastGroundVelocity) < 0 
                                            && groundVelocityDelta.magnitude > platformStopThreshold;
            Debug.Log("-");
            if (Vector3.Dot(groundVelocityDelta, _lastGroundVelocity) < 0) {
                Debug.Log("switch dir");
                if(groundVelocityDelta.magnitude > platformStopThreshold) Debug.Log("large");
                else {
                    Debug.Log($"{groundVelocityDelta.magnitude} !> {platformStopThreshold}");
                }
            }
            if(groundStoppedImmediately) Debug.Log("stop");
            _lastGroundVelocity = groundVelocity;
            
            // Get jump velocity
            bool jumped = TryJump(out var jumpVelocity);

            // Friction and Slopes
            if (_ground.HasAnyGround) {
                // Ground Friction
                var velocityRelativeToGround = velocity - groundVelocity;
                velocity -= Vector3.ProjectOnPlane(velocityRelativeToGround, _ground.Normal) * GroundFriction * deltaTime;
                
                // On even ground or walkable slope
                if (_ground.HasStableGround) {
                    // Project walk movement on slope
                    _inputDirection = Vector3.ProjectOnPlane(_inputDirection, _ground.Normal);

                    var upVelocity = Vector3.Project(velocity, transform.up);
                    float upVelocityValue = Vector3.Dot(upVelocity, transform.up) > 0 ? upVelocity.magnitude : -upVelocity.magnitude;

                    // Stick to slope
                    if (_ground.Angle > 0.1f) {
                        var stickToGround = -_ground.Normal * 50f;
                        if (dynamicGround) {
                            stickToGround = gravity * 0.1f;
                            if (_inputDirection == Vector3.zero) {
                                var slopeUp = Vector3.ProjectOnPlane(transform.up, _ground.Normal);
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
                    
                }
                // On Steep Slope
                else {
                    var slopeUp = Vector3.ProjectOnPlane(transform.up, _ground.Normal);
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
                
                // Add moving ground velocity, don't apply if sudden platform movement -> player should get launched off
                if(!groundStoppedImmediately)
                    velocity += groundVelocityDelta;
                else {
                    Debug.Log("launch");
                }
            } else {
                // Air Drag
                velocity -= Vector3.ProjectOnPlane(velocity, transform.up) * AirDrag * deltaTime;
            }

            // Ground Acceleration
            {
                var velocityRelativeToGround = velocity - groundVelocity;
                var velocityInInputDir = Vector3.Project(velocityRelativeToGround, _inputDirection);
                bool movingInOppositeDirection = Vector3.Dot(velocityInInputDir, _inputDirection) < 0;

                var moveSpeed = _ground.HasStableGround ? MaxMoveSpeed : MaxAirMoveSpeed;
                var acceleration = _ground.HasStableGround ? MoveAcceleration : AirAcceleration;
                
                bool hasNotReachedMaxSpeed = velocityInInputDir.magnitude < moveSpeed || movingInOppositeDirection;
                if (hasNotReachedMaxSpeed) {
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
                dynamicGround?.AddForceAtPosition(-jumpVelocity * _rigidbody.mass, _ground.Hit.point, ForceMode.Impulse);
            }

            // Apply Force
            _rigidbody.AddForce(velocity - _rigidbody.linearVelocity, ForceMode.VelocityChange);
            _rigidbody.AddForce(gravity, ForceMode.Acceleration);
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
            if (_ground.HasStableGround) _coyoteLastGroundedTime = Time.time;
            bool hasJumpInput = _lastJumpInputTime + JumpPreGroundedGraceTime > Time.time;
            bool canJump = _coyoteLastGroundedTime + JumpPostGroundedGraceTime > Time.time && _lastJumpTime + MinTimeBetweenJumps < Time.time;
            if (!hasJumpInput || !canJump) return false;
            _coyoteLastGroundedTime = 0;
            _lastJumpTime = Time.time;

            float jumpUpSpeed = Mathf.Sqrt(JumpHeight * 2f * Gravity.magnitude);
            jumpVelocity = transform.up * jumpUpSpeed;
            
            // un-ground yourself
            _ground = default; 
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
            if (_ground is { HasAnyGround: true, HasStableGround: false }) return noStep; // no stepping on steep slope
            // if (!_ground.HasAnyGround && Vector3.Project(_rigidbody.linearVelocity, transform.up).magnitude > 1.0f)
            //     return noStep; // no air stepping when velocity is too high
            if (_inputDirection == Vector3.zero) return noStep; // no unintended stepping

            const float minStepHeight = 0.05f;
            const float stepForward = 0.05f;
            int layerMask = ~LayerMask.GetMask("Player");
            var input = Vector3.ProjectOnPlane(_inputDirection, _ground.HasStableGround ? _ground.Normal : transform.up);
            var maxStepHeight = _ground.HasStableGround ? MaxStepHeight : MaxAirStepHeight;

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